using Common.Logging;
using Grpc.Core;
using System.Globalization;
using TalentaReceiver.Models;
using TalentaReceiver.Repositories;
using TalentaReceiver.Utils;

namespace TalentaReceiver.UseCases
{
    public interface IPeriodDailyWorkScheduleUsecase
    {
        Task<Protos.ResponseMessage> PostEventPwsDws(Protos.RootEventPwsDws request, ServerCallContext ctx);
        Task<Protos.ResponseMessage> PostSingleDws(Protos.RootEventPwsDws request, ServerCallContext ctx);
        Task<Protos.ResponseMessage> PostPwsMaster(Protos.RootMasterPws request, ServerCallContext ctx);
        Task<Protos.ResponseMessage> PostDwsMaster(Protos.RootDwsMaster request, ServerCallContext ctx);
        
    }

    public class PeriodDailyScheduleUsecase : IPeriodDailyWorkScheduleUsecase
    {
        private readonly ILogger<IPeriodDailyWorkScheduleUsecase> _log;
        private readonly IMTEmployeeRepositories _repo;
        private readonly ITalentaReceiverUsecase _talReciverUsc;
        private readonly HttpClient _httpClient;
        private readonly IHelperService _helper;
        private readonly IMTJobLogRepository _logDb;
        private readonly IUrlSettingProvider _configUrl;

        public PeriodDailyScheduleUsecase(ILogger<IPeriodDailyWorkScheduleUsecase> log, IMTEmployeeRepositories repo, ITalentaReceiverUsecase talReciverUsc, HttpClient httpClient, IHelperService helper, IMTJobLogRepository logDb, IUrlSettingProvider configUrl)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _repo = repo;
            _talReciverUsc = talReciverUsc;
            _httpClient = httpClient;
            _helper = helper;
            _logDb = logDb;
            _configUrl = configUrl ?? throw new ArgumentNullException(nameof(configUrl));
        }

        public async Task<Protos.ResponseMessage> PostEventPwsDws(Protos.RootEventPwsDws request, ServerCallContext ctx)
        {
            try
            {
                var getUrlPws = _configUrl.GetEndpoint("/SapHcm/ZiHrPostPwsRfc");
                var getUrlDws = _configUrl.GetEndpoint("/SapHcm/ZiHrPostDwsRfc");
                var rootData = await PwsDwsPayload(request);

                #region Whitelist Nip Validation
                //var lEmpString = request.Data.ListEmployees.SelectMany(e => e.ListEmployee2.Select(emp => emp.EmployeeId)).Distinct().ToList();
                //var existingWhitelistNips = await _logDb.db().CheckExistingWhitelistNips(lEmpString);
                //if (existingWhitelistNips.Count() == 0)
                //{
                //    _log.LogError($"400 Bad Request: Employee Id not found on the whitelist: {string.Join(", ", lEmpString)}, endpoints: /v1/talenta-receiver/event-pws-dws");
                //    return new ResponseMessage
                //    {
                //        Code = 400,
                //        Message = $"The following NIPs are not found on the whitelist: {string.Join(", ", lEmpString)}"
                //    };
                //}
                #endregion

                await _repo.dbPwsDws().PostEventPwsDws(rootData);

                SDLogging.Log($"Successfully create/update pws-dws transaction, data = {request}", "INFO", SDLogging.INFO);

                #region Mapping to SAP
                foreach (var itemEvent in rootData.PwsDwsDetailDetails)
                {
                    #region Varible Declaration
                    var empId = itemEvent.employee_id;
                    var getStartEndTime = await _repo.dbPwsDws().GetStartEndTime(empId, itemEvent.daily_work_schedule_code); // get from daily_work_schedule_detaildetail
                    var getStartEndDate = rootData.EmployeesPwsDwsDetails.FirstOrDefault(s => s.employee_id == itemEvent.employee_id)?.start_date;
                    var employeeDetail = rootData.EmployeesPwsDwsDetails.FirstOrDefault(s => s.employee_id == empId);

                    if (employeeDetail == null || string.IsNullOrWhiteSpace(employeeDetail.start_date))
                    {
                        return new Protos.ResponseMessage
                        {
                            Code = 404,
                            Message = $"start-end date with employee id: {empId} is not found on daily_work_schedule"
                        };
                    }

                    if (getStartEndDate == null)
                    {
                        return new Protos.ResponseMessage
                        {
                            Code = 404,
                            Message = $"start-end date with employee id: {empId} is not found on daily_work_schedule"
                        };
                    }

                    if (getStartEndTime == null)
                    {
                        return new Protos.ResponseMessage
                        {
                            Code = 404,
                            Message = $"start-time time with employee id: {empId} is not found on daily_work_schedule_detaildetail"
                        };
                    }

                    #endregion

                    #region Mapping PWS Section
                    var itemPwsRfc = new SapPwsRfc()
                    {
                        pernr = empId.PadLeft(8, '0'),
                        begda = DateTime.ParseExact(getStartEndDate, "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd"),
                        endda = "99991231",
                        schkz = employeeDetail.work_schedule_rule ?? string.Empty,
                        zterf = "1",
                    };

                    var sapPws_response = await _helper.PostToSapApi(itemPwsRfc, getUrlPws, "PWS");

                    var lPwsRfcLog = new List<SapPwsRfcDTO>();

                    lPwsRfcLog.Add(new SapPwsRfcDTO
                    {
                        sapPwsRfc = itemPwsRfc,
                        responseMetadata = new ResponseMetadata
                        {
                            created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            created_by = "talenta-receiver",
                            status = sapPws_response?.Code == 200 ? "true" : "false",
                            response_message = sapPws_response?.Message!
                        }
                    });

                    await _logDb.db().PostPwsLog(lPwsRfcLog);

                    #endregion

                    //await Task.Delay(5000, ctx.CancellationToken);

                    #region Mapping DWS Section 
                    //var getStringStartEndTime = getStartEndTime.planned_working_time;
                    //string[] timeParts = getStringStartEndTime.Split(" - ");
                    //string startTime = timeParts[0];
                    //string endTime = timeParts[1];

                    //var startTimeParse = TimeOnly.Parse(startTime).ToString("HH:mm:ss").Replace(":", "");
                    //var endTimeParse = TimeOnly.Parse(endTime).ToString("HH:mm:ss").Replace(":", "");

                    //var itemSapDws = new SapDws()
                    //{
                    //    pernr = empId.PadLeft(8, '0'),
                    //    begda = DateTime.ParseExact(getStartEndDate, "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd"),
                    //    endda = DateTime.ParseExact(getStartEndDate, "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd"),
                    //    beguz = startTimeParse,
                    //    enduz = endTimeParse,
                    //};

                    //var sapDws_response = await _helper.PostToSapApi(itemSapDws, getUrlDws, "DWS");

                    //var lDwsLog = new List<SapDwsDTO>();
                    //lDwsLog.Add(new SapDwsDTO
                    //{
                    //    sapDws = itemSapDws,
                    //    responseMetadata = new ResponseMetadata
                    //    {
                    //        created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    //        created_by = "talenta-receiver",
                    //        status = sapDws_response?.Code == 200 ? "true" : "false",
                    //        response_message = sapDws_response?.Message!
                    //    }
                    //});

                    //await _logDb.db().PostDwsLog(lDwsLog);
                    #endregion
                }
                #endregion

                return new Protos.ResponseMessage
                {
                    Code = 200,
                    Message = "OK"
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<Protos.ResponseMessage> PostSingleDws(Protos.RootEventPwsDws request, ServerCallContext ctx)
        {
            //SDLogging.Log($"log daily work schedule, endpoint: /v1/talenta-receiver/single-dws, data = {request}", "INFO", SDLogging.INFO);
            if (request == null)
            {
                SDLogging.Log($"Bad request: request payload is null, endpoint: /v1/talenta-receiver/single-dws, data = {request}", "INFO", SDLogging.INFO);

                return new Protos.ResponseMessage
                {
                    Code = 400,
                    Message = $"bad request"
                };
            }

            try
            {
                var getUrl = _configUrl.GetEndpoint("/SapHcm/ZiHrPostDwsRfc");
                var rootData = await PwsDwsPayload(request);

                #region Whitelist Nip Validation
                //var lEmpString = request.Data.ListEmployees.SelectMany(e => e.ListEmployee2.Select(emp => emp.EmployeeId)).Distinct().ToList();
                //var existingWhitelistNips = await _logDb.db().CheckExistingWhitelistNips(lEmpString);
                //if (existingWhitelistNips.Count() == 0)
                //{
                //    _log.LogError($"400 Bad Request: Employee Id not found on the whitelist: {string.Join(", ", lEmpString)}, endpoints: /v1/talenta-receiver/single-dws");
                //    return new ResponseMessage
                //    {
                //        Code = 400,
                //        Message = $"The following NIPs are not found on the whitelist: {string.Join(", ", lEmpString)}"
                //    };
                //}
                #endregion

                await _repo.dbPwsDws().PostEventPwsDws(rootData);
                SDLogging.Log($"Successfully post single dws transaction, data = {request}", "INFO", SDLogging.INFO);

                #region Mapping to SAP
                foreach (var itemEvent in rootData.EmployeesPwsDwsDetails)
                {
                    #region Variables Declaration
                    var empId = itemEvent.employee_id;
                    var dwsCode = rootData.PwsDwsDetailDetails.FirstOrDefault(x => x.employee_id == empId && x.start_date == itemEvent.start_date)?.daily_work_schedule_code ?? "";
                    var getStartEndTime = await _repo.dbPwsDws().GetStartEndTime(empId, dwsCode);
                    
                    var getStringStartEndTime   = getStartEndTime.planned_working_time;
                    string[] timeParts          = getStringStartEndTime.Split(" - ");
                    string startTime            = timeParts[0];
                    string endTime              = timeParts[1];                    
                    var startTimeParse          = TimeOnly.Parse(startTime).ToString("HH:mm:ss").Replace(":", "");
                    var endTimeParse            = TimeOnly.Parse(endTime).ToString("HH:mm:ss").Replace(":", "");

                    #endregion

                    var itemSapDws = new SapDws()
                    {
                        pernr = empId.PadLeft(8, '0'),
                        begda = DateTime.ParseExact(itemEvent.start_date!, "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd"),
                        endda = DateTime.ParseExact(itemEvent.start_date!, "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd"),
                        beguz = startTimeParse,
                        enduz = endTimeParse,
                    };

                    var sapDws_response = await _helper.PostToSapApi(itemSapDws, getUrl, "DWS");

                    var lDwsLog = new List<SapDwsDTO>();

                    lDwsLog.Add(new SapDwsDTO
                    {
                        sapDws = itemSapDws,
                        responseMetadata = new ResponseMetadata
                        {
                            created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            created_by = "talenta-receiver",
                            status = sapDws_response?.Code == 200 ? "true" : "false",
                            response_message = sapDws_response?.Message!
                        }
                    });

                    await _logDb.db().PostDwsLog(lDwsLog);
                }

                #endregion

                return new Protos.ResponseMessage
                {
                    Code = 200,
                    Message = "OK"
                };
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<DataEmpPwsDws> PwsDwsPayload(Protos.RootEventPwsDws request)
        {
            var lemployee = request.Data.ListEmployees;
            List<Models.EmployeesPwsDwsDetail2> lempDetail = new List<Models.EmployeesPwsDwsDetail2>();
            List<Models.PwsDwsDetailDetail> lPwsDwsDetail2 = new List<Models.PwsDwsDetailDetail>();

            foreach (var lemp in lemployee)
            {
                var lEmp2 = lemp.ListEmployee2;

                foreach (var emp in lEmp2)
                {
                    var lEmpPwsDws2 = new Models.EmployeesPwsDwsDetail2
                    {
                        employee_id = emp.EmployeeId,
                        start_date = emp.StartDate,
                        month_hours = (int)(emp.MonthHours != 0 ? emp.MonthHours : 0)!,
                        week_hours = Convert.ToDouble(emp.WeekHours),
                        daily_hours = (int)(emp.DailyHours != 0 ? emp.DailyHours : 0)!,
                        week_days = (int)(emp.WeekDays != 0 ? emp.WeekDays : 9)!,
                        work_schedule_rule = emp.WorkScheduleRule,
                        work_break_schedule = emp.WorkBreakSchedule,
                    };
                    lempDetail.Add(lEmpPwsDws2);

                    for (int i = 0; i < emp.DailyWorkScheduleCode.Count; i++)
                    {
                        var arrayPwsDwsDetail2 = new Models.PwsDwsDetailDetail
                        {
                            employee_id = emp.EmployeeId,
                            daily_work_schedule_code = emp.DailyWorkScheduleCode[i],
                            planned_working_hours = emp.PlannedWorkingHours[i].ToString(),
                            planned_working_time = emp.PlannedWorkingTime[i],
                            start_date = emp.StartDate,
                        };
                        lPwsDwsDetail2.Add(arrayPwsDwsDetail2);
                    }
                }
            }

            var dataEmpPwsDwsDetail = new DataEmpPwsDws
            {
                EmployeesPwsDwsDetails = lempDetail,
                PwsDwsDetailDetails = lPwsDwsDetail2
            };

            return dataEmpPwsDwsDetail;
        }

        public async Task<Protos.ResponseMessage> PostPwsMaster(Protos.RootMasterPws request, ServerCallContext ctx)
        {
            try
            {
                List<Models.PeriodWorkSchedule> lPws = new List<Models.PeriodWorkSchedule>();
                foreach (var pws in request.Data.PeriodWorkSchedules)
                {
                    var listPDws = new Models.PeriodWorkSchedule();
                    Models.PeriodWorkSchedule modelPws = new Models.PeriodWorkSchedule
                    {
                        period_work_schedule_code = pws.PeriodWorkSchedule.PeriodWorkScheduleCode,
                        work_days = (int)pws.PeriodWorkSchedule.WorkDays,
                        daily_work_schedule_code = pws.PeriodWorkSchedule.DailyWorkScheduleCode,
                        period_work_schedule_text = pws.PeriodWorkSchedule.PeriodWorkScheduleText,
                        daily_work_schedule = pws.PeriodWorkSchedule.DailyWorkSchedule.ToList()
                    };
                    lPws.Add(modelPws);
                }

                await _repo.dbPwsDws().AddPws(lPws);

                SDLogging.Log($"Successfully post master pws, data = {request}", "INFO", SDLogging.INFO);

                return new Protos.ResponseMessage
                {
                    Code = 200,
                    Message = "OK"
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Protos.ResponseMessage> PostDwsMaster(Protos.RootDwsMaster request, ServerCallContext ctx)
        {
            try
            {
                var empId = await _repo.dbPwsDws().GetEmployeeId();

                List<Models.DailyWorkSchedule> lDws = new List<Models.DailyWorkSchedule>();
                foreach (var dws in request.Data.DailyWorkSchedule)
                {
                    Models.DailyWorkSchedule modelsDws = new Models.DailyWorkSchedule
                    {
                        daily_work_schedule_group = (int)dws.DailyWorkScheduleGroup!,
                        daily_work_schedule_code = dws.DailyWorkScheduleCode,
                        end_date = dws.EndDate,
                        start_date = dws.StartDate,
                        daily_work_schedule_text = dws.DailyWorkScheduleText,
                        planed_working_hours = (int)dws.PlanedWorkingHours!,
                        work_break_schedule_code = dws.WorkBreakScheduleCode,
                        employee_id = empId,
                    };
                    lDws.Add(modelsDws);
                };

                await _repo.dbPwsDws().AddDws(lDws);

                SDLogging.Log($"Successfully post dws master, data = {request}", "INFO", SDLogging.INFO);

                return new Protos.ResponseMessage
                {
                    Code = 200,
                    Message = "OK"
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
