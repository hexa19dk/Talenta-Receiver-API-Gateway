using Common.Logging;
using Grpc.Core;
using System.Configuration;
using System.Runtime.CompilerServices;
using TalentaReceiver.Protos;
using TalentaReceiver.UseCases;

namespace TalentaReceiver.Services
{
    public class SapLogDataService : Protos.SapLogDataServiceGrpc.SapLogDataServiceGrpcBase
    {
        private readonly ILogger<SapLogDataService> _log;
        private readonly ISapLogDataUsecase _sapLogUc;
        public SapLogDataService(ISapLogDataUsecase sapLogUc, ILogger<SapLogDataService> log)
        {
            _log = log;
            _sapLogUc = sapLogUc;
        }

        public async override Task<Protos.HiringEmployeeResponse> GetHiringEmployeeLog(Protos.ParamRequest request, ServerCallContext ctx)
        {
            try
            {
                var result = await _sapLogUc.GetHiringEmployee(request);
                return result;
            }
            catch (Exception ex)
            {
                SDLogging.Log($"Error get hiring employee data: {ex.Message}, {request}", SDLogging.ERROR);
                ctx.Status = new Status(StatusCode.Aborted, "error get hiring employee data" + ex.Message);
                throw;
            }
        }

        public async override Task<Protos.PersonalDataResponse> GetPersonalEmployeeLog(Protos.ParamRequest request, ServerCallContext ctx)
        {
            try
            {
                var result = await _sapLogUc.GetPersonalData(request);
                return result;
            }
            catch (Exception ex)
            {
                SDLogging.Log($"Error get personal data: {ex.Message}, {request}", SDLogging.ERROR);
                ctx.Status = new Status(StatusCode.Aborted, "error get personal data" + ex.Message);
                throw;
            }
        }

        public async override Task<Protos.ResponseMessage> PostWhitelistNipEmployee(WhitelistNipEmployeeRequest request, ServerCallContext ctx)
        {
            try 
            {                 
                var result = await _sapLogUc.PostWhitelistNipEmployee(request);
                return result;
            }
            catch (Exception ex)
            {
                SDLogging.Log($"Error post whitelist nip employee: {ex.Message}, {request}", SDLogging.ERROR);
                ctx.Status = new Status(StatusCode.Aborted, "error post whitelist nip employee" + ex.Message);
                throw;
            }
        }
    }
}
