using TalentaReceiver.UseCases;
using Grpc.Core;
using Common.Logging;
using Google.Cloud.PubSub.V1;
using TalentaReceiver.Protos;
using System.Configuration;

namespace TalentaReceiver.Services
{
    public class TalentaReceiverService : Protos.TalentaReceiverServiceGrpc.TalentaReceiverServiceGrpcBase
    {
        private readonly ILogger<TalentaReceiverService> _log;
        private readonly ITalentaReceiverUsecase _uc;
        
        public TalentaReceiverService(ILogger<TalentaReceiverService> log, ITalentaReceiverUsecase uc)
        {
            _log = log;
            _uc = uc;            
        }

        public async override Task<Protos.ResponseMessage> PostEmployee(Protos.Root request, ServerCallContext ctx)
        {
            try
            {
                var result = await _uc.PostEmployee(request, ctx);

                return result;
            }
            catch (Exception ex)
            {
                SDLogging.Log($"Error post employee {ex.Message}, UserId = {request.Data.Employees.First().Employee.Employment.EmployeeId} :  {request.Data}", SDLogging.ERROR);
                ctx.Status = new Status(StatusCode.Aborted, "Post Employee Failed, error " + ex.Message);
                return new Protos.ResponseMessage
                {
                    Code = 500,
                    Message = ex.Message,
                };
            }
        }

        public async override Task<Protos.ResponseMessage> UpdateEmployee(Protos.Root request, ServerCallContext ctx)
        {
            try
            {
                var result = await _uc.UpdateEmployee(request, ctx);

                return result;
            }
            catch (Exception ex)
            {
                SDLogging.Log($"Error update employee {ex.Message}, UserId = {request.Data.Employees.First().Employee.Employment.EmployeeId} :  {request.Data}", SDLogging.ERROR);
                ctx.Status = new Status(StatusCode.Aborted, "Update Employee Failed, error " + ex.Message);
                return new Protos.ResponseMessage
                {
                    Code = 500,
                    Message = ex.Message,
                };
            }
        }

        public async override Task<Protos.ResponseMessage> EmployeeTransfer(Protos.EmpTransferRoot request, ServerCallContext ctx)
        {
            try
            {
                var result = await _uc.EmployeeTransfer(request, ctx);

                return result;
            }
            catch (Exception ex)
            {
                SDLogging.Log($"Employee transfer error {ex.Message} :  {request.EmpTransferData}", SDLogging.ERROR);
                ctx.Status = new Status(StatusCode.Aborted, "Update Employee Failed, error " + ex.Message);
                return new Protos.ResponseMessage
                {
                    Code = 500,
                    Message = ex.Message,
                };
            }
        }

        public override async Task<Protos.ResponseMessage> EmployeeResign(Protos.ResignDataRequest request, ServerCallContext context)
        {
            try
            {
                SDLogging.Log($"Employee resign submitted, Resign Data: {request.Data}", "INFO", SDLogging.INFO);
                var result = await _uc.EmployeeResign(request, context);
                return result;
            }
            catch (Exception ex)
            {
                SDLogging.Log($"Employee resign error {ex.Message}, Resign Data : {request}", SDLogging.ERROR);
                context.Status = new Status(StatusCode.Aborted, "Employee resign failed, error " + ex.Message);
                return new Protos.ResponseMessage
                {
                    Code = 500,
                    Message = context.Status.ToString()
                };
            }
        }

    }
}
