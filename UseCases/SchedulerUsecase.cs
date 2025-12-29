using FluentValidation.Results;
using Grpc.Core;
using Microsoft.VisualBasic;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using Polly;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Web;
using TalentaReceiver.Models;
using TalentaReceiver.Protos;
using TalentaReceiver.Repositories.MsSql;
using TalentaReceiver.Utils;
using static NUnit.Framework.Constraints.NUnitEqualityComparer;
using static TalentaReceiver.Models.OvertimeApprovalModels;

namespace TalentaReceiver.UseCases
{
    public interface ISchedulerUsecase
    {
        Task<Protos.ResponseMessage> RunOvertimJob(Protos.JobRequest request);
        Task<Protos.ResponseMessage> TimeoffJob(Protos.JobRequest request, ServerCallContext ctx);
        Task<Protos.ResponseMessage> DailyWorkScheduleJob(JobRequest request, ServerCallContext context);
        Task<Protos.ResponseMessage> PeriodWorkScheduleJob(JobRequest request, ServerCallContext context);
    }

    public class SchedulerUsecase : ISchedulerUsecase
    {
        private readonly ILogger<SchedulerUsecase> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHelperService _helper;
        private readonly ISchedularHelperService _schHelper;
        private readonly IMTJobLogSAP _logDb;
        private readonly IUrlSettingProvider _configUrl;

        public SchedulerUsecase(ILogger<SchedulerUsecase> logger, IServiceProvider serviceProvider, IHelperService helper, ISchedularHelperService schHelper, IMTJobLogSAP logDb, IUrlSettingProvider configUrl)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _helper = helper;
            _schHelper = schHelper;
            _logDb = logDb;
            _configUrl = configUrl;
        }

        #region Overtime Job Section
        public StartEndDateDTO GetStartEndDate()
        {
            var currDate = DateTime.Today;
            //var currDate = new DateTime(2025, 11, 10);

            var validDays = new[] { 1, 5, 10 };
            var isValidDay = validDays.Contains(currDate.Day) || (currDate.Day >= 14 && currDate.Day <= 21);

            if (!isValidDay)
            {
                throw new InvalidOperationException($"Invalid schedule execution date: {currDate:yyyy-MM-dd}. Not within a valid schedule window.");
            }

            var previousMonth = currDate.AddMonths(-1);
            var startDate = new DateTime(previousMonth.Year, previousMonth.Month, 16);
            var endDate = new DateTime(currDate.Year, currDate.Month, 15);

            return new StartEndDateDTO
            {
                StartDate = startDate,
                EndDate = endDate,
            };
        }

        public static List<OvertimeGradeRule> GetOvertimeGradeRules()
        {
            return new List<OvertimeGradeRule>
            {
                new OvertimeGradeRule
                {
                    Class = "A",
                    Grades = new List<string> { "5", "6", "7", "8", "9", "10A", "10B1" },
                    MinHoursPerDay = 3,
                    MaxHoursPerWeek = 18
                },
                new OvertimeGradeRule
                {
                    Class = "B",
                    Grades = new List<string> { "10B2", "11", "11A1", "11A2", "11B1", "11B2", "12", "12A1", "12A2", "12B1", "12B2" },
                    MinHoursPerDay = 5,
                    MaxHoursPerWeek = 18
                }
            };
        }

        public async Task<ResponseMessage> RunOvertimJob(Protos.JobRequest request)
        {
            #region Variables 
            var cancellationToken   = new CancellationToken();
            var scope               = _serviceProvider.CreateScope();
            var helper              = scope.ServiceProvider.GetRequiredService<IHelperService>();
            var logDb               = scope.ServiceProvider.GetRequiredService<IMTJobLogSAP>();
            var lOvertimeLog        = new List<OvertimeSAPLogDTO>();
            var configUrl           = scope.ServiceProvider.GetRequiredService<IUrlSettingProvider>();
            var getUrl              = configUrl.GetEndpoint("/SapHcm/ZiHrPostOvrtmRfc");

            //var getDate             = GetStartEndDate();
            var getDate         = GetTimeoffStartEndDate();
            var paramStartDate  = $"date=" + getDate.StartDate.ToString("yyyy-MM-dd");
            var paramEndDate    = $"end_date=" + getDate.EndDate.ToString("yyyy-MM-dd");

            //var paramStartDate = $"date=" + getDate.StartDate.ToString("2025-12-01");
            //var paramEndDate = $"end_date=" + getDate.EndDate.ToString("2025-12-16");
            string nextPageUrl = $"https://api.mekari.com/v2/talenta/v3/attendance/summary-report?{paramStartDate}&{paramEndDate}&overtime=true";

            var uri          = new Uri(nextPageUrl);
            var query        = HttpUtility.ParseQueryString(uri.Query);
            int successCount = 0, failCount = 0;

            // Network configuration
            const int maxRetries = 3;
            const int baseDelayMs = 1000; // 1 second base delay
            const int maxDelayMs = 10000; // 10 seconds max delay
            const int requestTimeoutMs = 30000; // 30 seconds timeout
            #endregion

            while (!string.IsNullOrEmpty(nextPageUrl) && !cancellationToken.IsCancellationRequested)
            {
                var retryCount = 0;
                var pageProcessed = false;

                while (retryCount <= maxRetries && !pageProcessed && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Add timeout to the HTTP request
                        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(requestTimeoutMs));
                        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
                        _logger.LogInformation($"Fetching page: {nextPageUrl} (Attempt {retryCount + 1}/{maxRetries + 1})");

                        // Check if response indicates an error
                        var responseData = await helper.GetTalentaData(nextPageUrl);
                        if (string.IsNullOrEmpty(responseData))
                        {
                            throw new InvalidOperationException("Empty response received from API");
                        }

                        // Validate the response structure
                        RootOvertime root = JsonConvert.DeserializeObject<RootOvertime>(responseData)!;
                        if (root?.data == null)
                        {
                            _logger.LogInformation($"Received empty data response for URL: {nextPageUrl}");
                            continue;
                        }

                        #region Fetch Data from Talenta Result

                        var lRootOvertime = root?.data?.summary_attendance_report ?? new List<SummaryAttendanceReport>();
                        var currentPageNullable = await _helper.GetPageNumberFromUrl(nextPageUrl);
                        var currentPage = currentPageNullable ?? 1;
                        var lastPage = root?.data?.pagination?.last_page;
                        var totalData = root?.data?.pagination?.total;

                        _logger.LogInformation($"Processing page {currentPage} with {lRootOvertime.Count} records");

                        #endregion

                        #region Checking NIP on the Whitelist 

                        //var nipList = lRootOvertime.Select(o => o.employee_id).Distinct().ToList();
                        //var existingWhitelistNips = await _logDb.CheckExistingWhitelistNips(nipList);
                        //var filteredOvertimeList = lRootOvertime.Where(o => existingWhitelistNips.Contains(o.employee_id)).ToList();

                        //if (filteredOvertimeList.Count == 0)
                        //{
                        //    _logger.LogInformation($"No whitelisted NIPs found on page {currentPage}. Skipping processing.");

                        //    if (string.IsNullOrEmpty(root.data.pagination?.next_page_url))
                        //    {
                        //        _logger.LogInformation("No more pages to process. Stopping.");
                        //        nextPageUrl = null;
                        //        break;
                        //    }

                        //    nextPageUrl = root.data.pagination?.next_page_url;
                        //    pageProcessed = true;
                        //    continue;
                        //}

                        #endregion

                        // Process each overtime item to SAP
                        int pageSuccess = 0, pageFail = 0;
                        //await ProcessOvertimeItems(filteredOvertimeList, lOvertimeLog, helper, getUrl, currentPage,  pageSuccess, pageFail);
                        await ProcessOvertimeItems(lRootOvertime, lOvertimeLog, helper, getUrl, currentPage,  pageSuccess, pageFail);

                        successCount += pageSuccess;
                        failCount += pageFail;

                        // Batch log the results
                        if (lOvertimeLog.Count > 0)
                        {
                            await logDb.PostOvertimeLog(lOvertimeLog);
                            lOvertimeLog.Clear(); // Clear after logging to free memory
                        }

                        nextPageUrl = root.data.pagination?.next_page_url;

                        if (string.IsNullOrEmpty(nextPageUrl))
                        {
                            _logger.LogInformation($"No more pages to fetch. Overtime job finished with TotalPages: {lastPage}, TotalData: {totalData}");
                            pageProcessed = true;
                            break;
                        }

                        //_logger.LogInformation($"Overtime page {currentPage} completed. FilteredRecord: {filteredOvertimeList.Count}, Success: {pageSuccess}, Failures: {pageFail}, TotalPages: {lastPage}");
                        _logger.LogInformation($"Overtime page {currentPage} completed. Success: {pageSuccess}, Failures: {pageFail}, TotalPages: {lastPage}");
                        pageProcessed = true;

                        // Add small delay between requests to avoid rate limiting
                        await Task.Delay(500, cancellationToken);
                    }
                    #region Catch Exceptions
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("Overtime job was cancelled");
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        // Timeout occurred
                        _logger.LogWarning($"Request timeout occurred for URL: {nextPageUrl} (Attempt {retryCount + 1})");
                        retryCount++;

                        if (retryCount <= maxRetries)
                        {
                            var delay = Math.Min(baseDelayMs * (int)Math.Pow(2, retryCount - 1), maxDelayMs);
                            _logger.LogInformation($"Retrying after {delay}ms delay...");
                            await Task.Delay(delay, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Unexpected error processing URL: {nextPageUrl} (Attempt {retryCount + 1})");
                        retryCount++;

                        if (retryCount <= maxRetries)
                        {
                            var delay = Math.Min(baseDelayMs * (int)Math.Pow(2, retryCount - 1), maxDelayMs);
                            _logger.LogInformation($"Retrying after {delay}ms delay...");
                            await Task.Delay(delay, cancellationToken);
                        }
                    }
                    #endregion
                }

                // If all retries failed, decide whether to continue or break
                if (!pageProcessed && retryCount > maxRetries)
                {
                    _logger.LogError($"Failed to process page after {maxRetries + 1} attempts. URL: {nextPageUrl}");

                    if (lOvertimeLog.Count > 0)
                    {
                        await _logDb.PostOvertimeLog(lOvertimeLog);
                    }

                    break;
                }
            }

            // Final log of any remaining items
            if (lOvertimeLog.Count > 0)
            {
                await logDb.PostOvertimeLog(lOvertimeLog);
            }

            return new ResponseMessage
            {
                Code = 200,
                Message = "Overtimejob completed"
            };
        }

        private async Task ProcessOvertimeItems(List<SummaryAttendanceReport> overtimeItems, List<OvertimeSAPLogDTO> lOvertimeLog, IHelperService helper, string getUrl, int currentPage, int pageSuccess, int pageFail)
        {
            var getEmpId = lOvertimeLog.Select(o => o.overtimeSAP.pernr).Distinct().ToList();
            var fristEmpId = getEmpId.FirstOrDefault();

            try
            {
                var getDate = GetStartEndDate();
                var scope = _serviceProvider.CreateScope();
                var logDb = scope.ServiceProvider.GetRequiredService<IMTJobLogSAP>();

                foreach (var overtimeItem in overtimeItems)
                {
                    #region Variables and Validation
                    var userId = overtimeItem.user_id;
                    var getDateConvert = DateTime.Parse(overtimeItem.schedule_date);
                    var getMonth = getDateConvert.Month.ToString();
                    var getYear = getDateConvert.Year.ToString();

                    // Get Overtime Approval Request List Talenta
                    var approvalStatusURL = $"https://api.mekari.com/v2/talenta/v2/overtime/{userId}/requests?year={getYear}&limit=100&month={getMonth}";
                    var overtimeApprovalResponse = await helper.GetTalentaData(approvalStatusURL);
                    var rootApproval = JsonConvert.DeserializeObject<RootOvertimeApproval>(overtimeApprovalResponse)!;

                    // Check if the overtime is approved
                    var checkApprovalStatus = rootApproval.data.overtime_request.FirstOrDefault(r => r.status_approval == 2 && r.request_date == overtimeItem.schedule_date);
                    if (checkApprovalStatus == null || checkApprovalStatus.status_approval != 2)
                    {
                        _logger.LogInformation($"Overtime data with User ID: {userId} is not approved yet");
                        pageFail++;
                        continue;
                    }

                    #endregion

                    try
                    {
                        #region SAP Data Mapping
                        var overtimeSAP = new OvertimeSAP()
                        {
                            pernr = overtimeItem.employee_id,
                            begda = overtimeItem.schedule_date.Replace("-", ""),
                            endda = overtimeItem.schedule_date.Replace("-", ""),
                            ktart = "01",
                        };

                        if (overtimeItem.holiday == false)
                        {
                            overtimeSAP.beguz = overtimeItem.schedule_out == "00:00:00" ? "240000" : overtimeItem.schedule_out.Replace(":", "");

                            TimeSpan scheduleOutTime = TimeSpan.Parse(overtimeItem.schedule_out);
                            TimeSpan overtimeDuration = TimeSpan.Parse(overtimeItem.overtime_duration);
                            TimeSpan sapScheduleOut = scheduleOutTime.Add(overtimeDuration);

                            overtimeSAP.enduz = sapScheduleOut.ToString(@"hhmmss");
                        }
                        else
                        {
                            overtimeSAP.beguz = overtimeItem.schedule_in.Replace(":", "");
                            overtimeSAP.enduz = overtimeItem.schedule_out == "00:00:00" ? "240000" : overtimeItem.schedule_out.Replace(":", "");
                        }
                        #endregion

                        #region Employee Grading Validation [Disable]

                        var dateParam = overtimeItem.schedule_date;
                        var empPersonnelURL = $"http://192.168.0.110:8184/LegacyApi/api/employee/GetPersonnel6?pernr={overtimeSAP.pernr}&begda={dateParam}&endda={dateParam}";
                        var getEmpPersonnel = await _helper.GetEmployeePersonnel6(overtimeSAP.pernr, empPersonnelURL);
                        EmployeePersonnel6 empPers6Item = JsonConvert.DeserializeObject<EmployeePersonnel6>(getEmpPersonnel)!;

                        var empGrade = empPers6Item.Trfgr + empPers6Item.Trfst;
                        var overtimeHours = TimeSpan.Parse(overtimeItem.overtime);
                        var totalOvertimeHours = (int)overtimeHours.TotalHours;
                        var overtimeRules = GetOvertimeGradeRules();
                        var matchedRule = overtimeRules.FirstOrDefault(r => r.Grades.Contains(empGrade));

                        if (empPers6Item == null)
                        {
                            _logger.LogInformation($"Employee Personnel data with NIP: {overtimeSAP.pernr} not found in SAP");
                            pageFail++;
                            continue;
                        }

                        if (totalOvertimeHours < matchedRule?.MinHoursPerDay)
                        {
                            _logger.LogInformation($"Overtime {overtimeHours}h for Employee id: {overtimeSAP.pernr} below min {matchedRule.MinHoursPerDay}h");
                            pageFail++;
                            continue;
                        }

                        if (matchedRule == null)
                        {
                            _logger.LogInformation($"Employee nip: {overtimeSAP.pernr} is not included in overtime rules range");
                            pageFail++;
                            continue;
                        }

                        if (matchedRule.Class == "B" && totalOvertimeHours < matchedRule.MinHoursPerDay)
                        {
                            _logger.LogInformation($"Overtime {overtimeHours}h for Employee id: {overtimeSAP.pernr} below min {matchedRule.MinHoursPerDay}h");
                            pageFail++;
                            continue;
                        }

                        _logger.LogInformation($"Overtime {overtimeHours}h for {empGrade} meets min requirement {matchedRule.MinHoursPerDay}h");

                        #endregion

                        // Check if NIP and Date already exists in SAP log database
                        var checkNipOvertimeExists = _logDb.CheckNipOnOvertime(overtimeSAP.pernr, overtimeSAP.begda);
                        if (checkNipOvertimeExists.Result != null)
                        {
                            _logger.LogInformation($"Overtime data with NIP: {overtimeSAP.pernr} and Date: {overtimeSAP.begda} already exists in SAP. Skipping.");
                            continue;
                        }

                        var resSAP = await _helper.PostToSapApi(overtimeSAP, getUrl, "Overtime");
                        lOvertimeLog.Add(new OvertimeSAPLogDTO
                        {
                            overtimeSAP = overtimeSAP,
                            responseMetadata = new ResponseMetadata
                            {
                                created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                created_by = "talenta-receiver",
                                status = resSAP?.Message == "OK" ? "SUCCESS" : "FAILED",
                                response_message = resSAP?.Message! + $" | TalentaPage: {currentPage}"
                            }
                        });

                        if (resSAP?.Message == "OK")
                            pageSuccess++;
                        else
                            pageFail++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process overtime for employee {EmployeeId}: {Message}",
                            overtimeItem.employee_id, ex.Message);
                        lOvertimeLog.Add(new OvertimeSAPLogDTO
                        {
                            overtimeSAP = new OvertimeSAP { pernr = overtimeItem.employee_id },
                            responseMetadata = new ResponseMetadata
                            {
                                created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                created_by = "talenta-receiver",
                                status = "ERROR",
                                response_message = $"Processing failed: {ex.Message} | TalentaPage: {currentPage}"
                            }
                        });

                        pageFail++;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process overtime for employee {EmployeeId}: {Message}", fristEmpId, ex.Message);
                throw;
            }          
        }

        #endregion


        #region Timeoff Job Section
        public StartEndDateDTO GetTimeoffStartEndDate()
        {
            var currDate = DateTime.Today;
            //var currDate = new DateTime(2025, 11, 5);
            DateTime startDate = default;
            DateTime endDate = default;
            var previousMonth = currDate.AddMonths(-1);

            if (currDate.Day >= 16 && currDate.Day <= 21)
            {
                startDate = new DateTime(previousMonth.Year, previousMonth.Month, 15);
                endDate = new DateTime(currDate.Year, currDate.Month, currDate.Day);
            }
            else if (currDate.Day >= 22 && currDate.Day <= 31)
            {
                startDate = new DateTime(currDate.Year, currDate.Month, 16);
                endDate = new DateTime(currDate.Year, currDate.Month, currDate.Day);
            }
            else if (currDate.Day >= 1 && currDate.Day <= 15)
            {
                startDate = new DateTime(previousMonth.Year, previousMonth.Month, 16);
                endDate = new DateTime(currDate.Year, currDate.Month, currDate.Day);
            }
            else
            {
                throw new InvalidOperationException($"Invalid schedule execution date: {currDate:yyyy-MM-dd}. Not within a valid schedule window.");
            }

            return new StartEndDateDTO
            {
                StartDate = startDate,
                EndDate = endDate,
            };
        }

        public List<StartEndDateDTO> GetTimeoffDateRange()
        {
            var startEndDateRange = new List<StartEndDateDTO>();
            var master = GetTimeoffStartEndDate();
            var start = master.StartDate;
            var end = master.EndDate;

            // ===== RANGE 1 : 15 → end of start month =====
            if (start.Day <= 15)
            {
                var r1Start = start;
                var r1End = new DateTime(start.Year, start.Month, DateTime.DaysInMonth(start.Year, start.Month));

                if (r1Start <= end)
                {
                    startEndDateRange.Add(new StartEndDateDTO
                    {
                        StartDate = r1Start,
                        EndDate = r1End > end ? end : r1End
                    });
                }
            }

            // ===== RANGE 2 : 01 → 15 of next month =====
            var nextMonth = new DateTime(start.Year, start.Month, 1).AddMonths(1);
            var r2Start = new DateTime(nextMonth.Year, nextMonth.Month, 1);
            var r2End = new DateTime(nextMonth.Year, nextMonth.Month, 15);
            if (r2Start <= end && r2End >= start)
            {
                startEndDateRange.Add(new StartEndDateDTO
                {
                    StartDate = r2Start < start ? start : r2Start,
                    EndDate = r2End > end ? end : r2End
                });
            }

            // ===== RANGE 3 : 16 → execution date =====
            var r3Start = new DateTime(end.Year, end.Month, 16);
            if (r3Start <= end)
            {
                startEndDateRange.Add(new StartEndDateDTO
                {
                    StartDate = r3Start < start ? start : r3Start,
                    EndDate = end
                });
            }

            return startEndDateRange;
        }

        public async Task<ResponseMessage> TimeoffJob(Protos.JobRequest request, ServerCallContext ctx)
        {
            #region Variables

            string url = "";
            int successCount = 0;
            int failCount = 0;
            string runId = Guid.NewGuid().ToString();
            var currentDate = DateTime.Today;
            int totalFilteredRecords = 0;

            using var scope         = _serviceProvider.CreateScope();
            var configUrl           = scope.ServiceProvider.GetRequiredService<IUrlSettingProvider>();
            var getUrl              = configUrl.GetEndpoint("/SapHcm/ZiHrPostAbsenRfc");
            var helper              = scope.ServiceProvider.GetRequiredService<IHelperService>();
            var JobLogDB            = scope.ServiceProvider.GetRequiredService<IMTJobLogSAP>();
            var lZiHrPostAbsenRfc   = new List<ZiHrPostAbsenRfcLogDTO>();
            var rangeSummaries      = new List<TimeoffRangeSummary>();

            // Retry Loop Variables
            const int maxRetries = 3;
            const int retryDelayMs = 25000;
            const int requestTimeoutSeconds = 60;           
            int rangeIndex = 1;

            #endregion

            var dateGroups = GetTimeoffDateRange();

            foreach (var dateGroup in dateGroups)
            {
                _logger.LogInformation($"Timeoff Job Date Range: {dateGroup.StartDate:yyyy-MM-dd} to {dateGroup.EndDate:yyyy-MM-dd}");
                //var paramStartDate = getDate.StartDate.ToString("2025-12-01");
                //var paramEndDate = getDate.EndDate.ToString("2025-12-16");
                var paramStartDate = dateGroup.StartDate.ToString("yyyy-MM-dd");
                var paramEndDate = dateGroup.EndDate.ToString("yyyy-MM-dd");
                url = $"https://api.mekari.com/v2/talenta/v2/time-off?start_date={paramStartDate}&end_date={paramEndDate}&status=approved";

                #region Talenta API Retry Loop 
                string responseData = null!;
                bool apiSuccess = false;
                int attempt = 0;

                while (!apiSuccess && attempt <= maxRetries)
                {
                    try
                    {
                        attempt++;
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(requestTimeoutSeconds));

                        _logger.LogInformation($"[Timeoff] Fetching Talenta API (Attempt {attempt}/{maxRetries})");

                        responseData = await helper.GetTalentaData(url);

                        if (string.IsNullOrWhiteSpace(responseData))
                            throw new Exception("Empty response from Talenta API");

                        apiSuccess = true;
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning($"[Timeoff] API timeout (Attempt {attempt})");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex.Message, $"[Timeoff] API error (Attempt {attempt})");
                    }

                    if (!apiSuccess && attempt <= maxRetries)
                    {
                        _logger.LogInformation($"[Timeoff] Retrying after {retryDelayMs / 1000} seconds...");
                        await Task.Delay(retryDelayMs, ctx.CancellationToken);
                    }
                }

                if (!apiSuccess)
                {
                    _logger.LogError("[Timeoff] Talenta API failed after max retries");
                    throw new Exception("Timeoff API unreachable");
                }
                #endregion
                
                try
                {
                    RootTimeoff root        = JsonConvert.DeserializeObject<RootTimeoff>(responseData)!;
                    var lTimeoff            = root.data.time_off ?? new List<TimeOff>();
                    var sumTimeoffOfRange   = lTimeoff.Count();
                    totalFilteredRecords += sumTimeoffOfRange;
                    var resultLog           = new ConcurrentBag<ZiHrPostAbsenRfcLogDTO>();

                    rangeSummaries.Add(new TimeoffRangeSummary
                    {
                        RangeIndex = rangeIndex,
                        Start = dateGroup.StartDate,
                        End = dateGroup.EndDate,
                        TotalRecords = sumTimeoffOfRange
                    });


                    if (lTimeoff.Count() != 0)
                    {
                        try
                        {
                            foreach (var timeoffItem in lTimeoff)
                            {
                                _logger.LogInformation($"Processing total timeoff data: {totalFilteredRecords}, timeoff records for employee id: {timeoffItem.employee_id}, URL to Talenta: {url}");
                                var ZiHrPostAbsenRfc = new ZiHrPostAbsenRfc()
                                {
                                    pernr = timeoffItem.employee_id,
                                    begda = timeoffItem.start_date,
                                    endda = timeoffItem.end_date,
                                    subty = timeoffItem.policy_code == "P011" ? "P010" : timeoffItem.policy_code
                                };

                                #region Core Process 
                                try
                                {
                                    var resToSAP = await helper.PostToSapApi(ZiHrPostAbsenRfc, getUrl, "Timeoff");
                                    resultLog.Add(new ZiHrPostAbsenRfcLogDTO
                                    {
                                        ziHrPostAbsenRfc = ZiHrPostAbsenRfc,
                                        responseMetadata = new ResponseMetadata
                                        {
                                            created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                            created_by = "talenta-receiver",
                                            response_message = resToSAP?.Message!
                                        }
                                    });

                                    if (resToSAP?.Message == "OK")
                                    {
                                        _logger.LogInformation($"Successfully processed timeoff for employee id: {timeoffItem.employee_id}, dates: {timeoffItem.start_date}");

                                        Interlocked.Increment(ref successCount);
                                    }

                                    if (resToSAP?.Message == "ERROR")
                                    {
                                        _logger.LogError($"Failed to process timeoff for employee id: {timeoffItem.employee_id}, dates: {timeoffItem.start_date}, reason: {resToSAP?.Message}");

                                        Interlocked.Increment(ref failCount);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"Failed to process timeoff for employee {timeoffItem.employee_id}: {ex.Message}", timeoffItem.employee_id, ex.Message);

                                    Interlocked.Increment(ref failCount);
                                }

                                #endregion
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Timeoff scheduler failed: {Message}", ex.Message);
                            await JobLogDB.PostZiHrPostAbsenRfcLog(lZiHrPostAbsenRfc);
                            Interlocked.Increment(ref failCount);
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to get timeoff data, Talenta response: " + responseData);
                    }

                    // Post to Timeoff DB Log
                    if (resultLog.Count > 0)
                    {
                        await JobLogDB.PostZiHrPostAbsenRfcLog(resultLog.ToList());
                    }

                    rangeIndex++;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Timeoff Scheduler Error: {ex.Message}, url: {url}");
                    throw;
                }                
            }

            #region Job Summary 

            _logger.LogInformation("========== TIMEOFF JOB SUMMARY ==========");
            foreach (var summary in rangeSummaries)
            {
                _logger.LogInformation(
                    "Range {Range} {Start:yyyy-MM-dd} → {End:yyyy-MM-dd} : {Total}",
                    summary.RangeIndex,
                    summary.Start,
                    summary.End,
                    summary.TotalRecords
                );
            }

            _logger.LogInformation("TOTAL PROCESSED : {Total}", totalFilteredRecords);
            //_logger.LogInformation($"SUCCESS : {successCount}, FAILED : {failCount}");
            _logger.LogInformation("========================================");

            #endregion

            return new ResponseMessage
            {
                Code = 200,
                Message = "Timeoffjob completed"
            };
        }

        #endregion


        #region PWS-DWS Job Section
        public async Task<Protos.ResponseMessage> DailyWorkScheduleJob(JobRequest request, ServerCallContext context)
        {
            string empId = "";
            int successCount = 0;
            int failureCount = 0;

            try
            {
                // Define Start-End Date
                var getUrl          = _configUrl.GetEndpoint("/SapHcm/ZiHrPostDwsRfc");
                var getDate         = GetStartEndDate();
                //var paramStartDate = getDate.StartDate.ToString("yyyy-MM-dd");
                //var paramEndDate = getDate.EndDate.ToString("yyyy-MM-dd");
                var paramStartDate = getDate.StartDate.ToString("2025-07-11");
                var paramEndDate = getDate.EndDate.ToString("2025-12-15 ");

                var dwsLogsData = await _logDb.GetDwsLogs(paramStartDate, paramEndDate);

                if (dwsLogsData == null || dwsLogsData.Count() == 0)
                {
                    _logger.LogInformation("No DWS logs found for the specified date range.");

                    return new Protos.ResponseMessage
                    {
                        Code = 200,
                        Message = "No DWS logs to process."
                    };
                }

                foreach (var dwsItem in dwsLogsData)
                {
                    try
                    {
                        var itemSapDws = new SapDws()
                        {
                            pernr = dwsItem.pernr,
                            begda = dwsItem.begda,
                            endda = dwsItem.endda,
                            beguz = dwsItem.beguz,
                            enduz = dwsItem.enduz,
                        };

                        empId = itemSapDws.pernr!;
                        var sapResponse = await _helper.PostToSapApi(itemSapDws, getUrl, "DWS");

                        if (sapResponse?.Message == "OK")
                        {
                            successCount++;
                        }
                        else
                        {
                            failureCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError($"[ERROR] Failed processing DWS with message: {ex.Message} | Employee: {empId} | Date: {dwsItem.begda} | SAP URL: {getUrl}");
                    }                    
                }

                _logger.LogInformation($"Finishing job daily work schedule with total data : {dwsLogsData.Count()}");
                _logger.LogInformation("SUCCESS : {Success}", successCount);
                _logger.LogInformation("FAILED : {Failed}", failureCount);

                return new Protos.ResponseMessage
                {
                    Code = 200,
                    Message = "OK"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to process dws scheduler for employee {empId}", ex.Message);
                throw;
            }
        }

        public async Task<Protos.ResponseMessage> PeriodWorkScheduleJob(JobRequest request, ServerCallContext context)
        {
            string empId = "";
            int successCount = 0;
            int failureCount = 0;

            try
            {
                // Define Start-End Date
                var getUrl = _configUrl.GetEndpoint("/SapHcm/ZiHrPostPwsRfc");
                var getDate = GetStartEndDate();
                //var paramStartDate = getDate.StartDate.ToString("yyyy-MM-dd");
                //var paramEndDate = getDate.EndDate.ToString("yyyy-MM-dd");
                var paramStartDate = getDate.StartDate.ToString("2025-07-11");
                var paramEndDate = getDate.EndDate.ToString("2025-12-15");
                var pwsLogData = await _logDb.GetPwsRfcs(paramStartDate, paramEndDate);

                if (pwsLogData == null || pwsLogData.Count() == 0)
                {
                    _logger.LogInformation("No PWS logs found for the specified date range.");

                    return new Protos.ResponseMessage
                    {
                        Code = 200,
                        Message = "No PWS logs to process."
                    };
                }

                foreach (var pwsItem in pwsLogData)
                {
                    try
                    {
                        var itemSapPws = new SapPwsRfc()
                        {
                            pernr = pwsItem.pernr,
                            begda = pwsItem.begda,
                            endda = pwsItem.endda,
                            schkz = pwsItem.schkz,
                            zterf = pwsItem.zterf,
                        };

                        empId = itemSapPws.pernr!;
                        var sapResponse = await _helper.PostToSapApi(itemSapPws, getUrl, "PWS");

                        if (sapResponse?.Message == "OK")
                        {
                            successCount++;
                        }
                        else
                        {
                            failureCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError($"[ERROR] Failed processing PWS with message: {ex.Message} | Employee: {empId} | Date: {pwsItem.begda} | SAP URL: {getUrl}");
                    }
                }

                _logger.LogInformation($"Finishing job period work schedule with total data : {pwsLogData.Count()}");
                _logger.LogInformation("SUCCESS : {Success}", successCount);
                _logger.LogInformation("FAILED : {Failed}", failureCount);

                return new Protos.ResponseMessage
                {
                    Code = 200,
                    Message = "OK"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to process pws scheduler for employee {empId}", ex.Message);
                throw;
            }
        }

        #endregion

    }
}
