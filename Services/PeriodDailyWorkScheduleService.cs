using Common.Logging;
using Grpc.Core;
using TalentaReceiver.UseCases;

namespace TalentaReceiver.Services
{
    public class PeriodDailyWorkScheduleService : Protos.PwsDwsMasterServiceGrpc.PwsDwsMasterServiceGrpcBase
    {
        private readonly ILogger<PeriodDailyWorkScheduleService> _log;
        private readonly IPeriodDailyWorkScheduleUsecase _uc;

        public PeriodDailyWorkScheduleService(ILogger<PeriodDailyWorkScheduleService> log, IPeriodDailyWorkScheduleUsecase uc)
        {
            _log = log;
            _uc = uc;
        }

        public async override Task<Protos.ResponseMessage> EventPwsDws(Protos.RootEventPwsDws request, ServerCallContext ctx)
        {
            try
            {
                var result = await _uc.PostEventPwsDws(request, ctx);
                return result;
            }
            catch (Exception ex)
            {
                SDLogging.Log($"Error post event pws-dws {ex.Message}, {request.Data}", SDLogging.ERROR);
                ctx.Status = new Status(StatusCode.Aborted, "Event PwsDws Failed, error " + ex.Message);
                return new Protos.ResponseMessage
                {
                    Code = 500,
                    Message = ex.Message,
                };
            }
        }

        public async override Task<Protos.ResponseMessage> PostSingleDws(Protos.RootEventPwsDws request, ServerCallContext ctx)
        {
            try
            {
                var result = await _uc.PostSingleDws(request, ctx);
                return result;
            }
            catch (Exception ex)
            {
                SDLogging.Log($"Error post single dws {ex.Message}, {request.Data}", SDLogging.ERROR);
                ctx.Status = new Status(StatusCode.Aborted, "Post Single DWS, error " + ex.Message);
                return new Protos.ResponseMessage
                {
                    Code = 500,
                    Message = ex.Message,
                };
            }
        }

        public async override Task<Protos.ResponseMessage> PostPwsMaster(Protos.RootMasterPws request, ServerCallContext ctx)
        {
            try
            {
                var result = await _uc.PostPwsMaster(request, ctx);
                return result;
            }
            catch(Exception ex)
            {
                SDLogging.Log($"Error post Period Work Schedule master {ex.Message}, {request.Data}", SDLogging.ERROR);
                ctx.Status = new Status(StatusCode.Aborted, "Event PwsDws Failed, error " + ex.Message);
                return new Protos.ResponseMessage
                {
                    Code = 500,
                    Message = ex.Message,
                };
            }
        }

        public async override Task<Protos.ResponseMessage> PostDwsMaster(Protos.RootDwsMaster request, ServerCallContext ctx)
        {
            try
            {
                var result = await _uc.PostDwsMaster(request, ctx);
                return result;
            }
            catch (Exception ex)
            {
                SDLogging.Log($"Error post Daily Work Schedule master {ex.Message}, {request.Data}", SDLogging.ERROR);
                ctx.Status = new Status(StatusCode.Aborted, "Event PwsDws Failed, error " + ex.Message);
                return new Protos.ResponseMessage
                {
                    Code = 500,
                    Message = ex.Message,
                };
            }
        }
    }
}
