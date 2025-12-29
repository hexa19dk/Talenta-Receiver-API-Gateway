using Google.Protobuf.WellKnownTypes;
using Serilog;
using TalentaReceiver.Protos;
using TalentaReceiver.Repositories.MsSql;

namespace TalentaReceiver.UseCases
{
    public interface ISapLogDataUsecase
    {
        Task<Protos.HiringEmployeeResponse> GetHiringEmployee(Protos.ParamRequest request);
        Task<Protos.PersonalDataResponse> GetPersonalData(Protos.ParamRequest request);
        Task<Protos.ResponseMessage> PostWhitelistNipEmployee(WhitelistNipEmployeeRequest request);
    }

    public class SapLogDataUsecase : ISapLogDataUsecase
    {
        private readonly ILogger<SapLogDataUsecase> _log;
        private readonly IMTJobLogSAP _jobLogSAP;

        public SapLogDataUsecase(ILogger<SapLogDataUsecase> log, IMTJobLogSAP jobLogSAP)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _jobLogSAP = jobLogSAP ?? throw new ArgumentNullException(nameof(jobLogSAP));
        }

        public async Task<Protos.HiringEmployeeResponse> GetHiringEmployee(Protos.ParamRequest request)
        {
            try
            {
                var employeeNumber = request.EmployeeId ?? string.Empty;
                var createdDate = request.CreatedAt ?? string.Empty;

                var dbResults = await _jobLogSAP.GetEmployeeHiringData(employeeNumber, createdDate);

                var response = new HiringEmployeeResponse();

                foreach (var row in dbResults)
                {
                    response.LHiringEmployee.Add(new HiringEmployeeModels
                    {
                        Employeenumber = row.employeenumber ?? string.Empty,
                        Hiringdate = row.hiringdate ?? string.Empty,
                        Actiontype = row.actiontype ?? string.Empty,
                        Reasonforaction = row.reasonforaction ?? string.Empty,
                        Werks = row.werks ?? string.Empty,
                        Btrtl = row.btrtl ?? string.Empty,
                        Persg = row.persg ?? string.Empty,
                        Persk = row.persk ?? string.Empty,
                        Abkrs = row.abkrs ?? string.Empty,
                        Ansvh = row.ansvh ?? string.Empty,
                        CreatedAt = row.created_at.ToString() ?? string.Empty,
                        ResponseMessage = row.response_message ?? string.Empty,
                        Status = row.status?.ToString() ?? string.Empty
                    });
                }

                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<PersonalDataResponse> GetPersonalData(ParamRequest request)
        {
            try
            {
                var employeeNumber = request.EmployeeId ?? string.Empty;
                var createdDate = request.CreatedAt ?? string.Empty;

                var dbResults = await _jobLogSAP.GetPersonalDataDTO(employeeNumber, createdDate);

                var response = new PersonalDataResponse();

                foreach (var row in dbResults)
                {
                    response.LPersonalData.Add(new PersonalDataModels
                    {
                        Pernr = row.pernr ?? string.Empty,
                        Begda = row.begda ?? string.Empty,
                        Endda = row.endda ?? string.Empty,
                        Cname = row.cname ?? string.Empty,
                        Anred = row.anred ?? string.Empty,
                        Sprsl = row.sprsl ?? string.Empty,
                        Gbpas = row.gbpas ?? string.Empty,
                        Gbort = row.gbort ?? string.Empty,
                        Gblnd = row.gblnd ?? string.Empty,
                        Natio = row.natio ?? string.Empty,
                        Famst = row.famst ?? string.Empty,
                        Famdt = row.famdt ?? string.Empty,
                        Konfe = row.konfe ?? string.Empty,
                        Gesch = row.gesch ?? string.Empty,
                        CreatedAt = row.created_at.ToString() ?? string.Empty,
                        ResponseMessage = row.response_message ?? string.Empty,
                        Status = row.status?.ToString() ?? string.Empty
                    });
                }

                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Protos.ResponseMessage> PostWhitelistNipEmployee(WhitelistNipEmployeeRequest request)
        {
            try
            {
                if (request.LNip == null || !request.LNip.Any())
                {
                    return new ResponseMessage
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Request cannot be empty. Please provide at least one NIP."
                    };
                }

                var nipList = request.LNip.Select(nip => nip.Trim()).Where(nip => !string.IsNullOrEmpty(nip)).Distinct().ToList();

                var existingNips = await _jobLogSAP.CheckExistingWhitelistNips(nipList);
                if (existingNips.Count() > 0)
                {
                    return new ResponseMessage
                    {
                        Code = StatusCodes.Status409Conflict,
                        Message = $"The following NIPs are already exist on Whitelist: {string.Join(", ", existingNips)}"
                    };
                }

                var postResult = await _jobLogSAP.PostWhitelistNipEmployee(nipList);

                return new ResponseMessage
                {
                    Code = StatusCodes.Status200OK,
                    Message = $"Successfully added {nipList.Count} NIP(s) to whitelist."
                };
            }
            catch
            {
                throw;
            }
        }
    }
}
