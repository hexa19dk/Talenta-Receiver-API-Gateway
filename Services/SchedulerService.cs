using Common.Logging;
using Grpc.Core;
using System.Configuration;
using TalentaReceiver.Protos;
using TalentaReceiver.UseCases;

namespace TalentaReceiver.Services
{
    public class SchedulerService : Protos.SchedulerServiceGrpc.SchedulerServiceGrpcBase
    {
        private readonly ILogger<SchedulerService> _log;
        private readonly ISchedulerUsecase _schedulerUc;

        public SchedulerService(ILogger<SchedulerService> log, ISchedulerUsecase schedulerUc) 
        { 
            _log = log;
            _schedulerUc = schedulerUc;
        }

        public override async Task<Protos.ResponseMessage> RunOvertimeJob(JobRequest request, ServerCallContext context)
        {
            try
            {
                return await _schedulerUc.RunOvertimJob(request);
            }
            catch (Exception ex)
            {
                SDLogging.Log("Overtime job error: " + ex.Message, SDLogging.ERROR);
                return new Protos.ResponseMessage
                {
                    Code = 500,
                    Message = "overtime job error, message:" + ex.Message
                };
            }
        }

        public override async Task<Protos.ResponseMessage> TimeoffJob(JobRequest request, ServerCallContext context)
        {
            try
            {
                return await _schedulerUc.TimeoffJob(request, context);
            }
            catch (Exception ex)
            {
                SDLogging.Log("Timeoff job error: " + ex.Message, SDLogging.ERROR);
                return new Protos.ResponseMessage
                {
                    Code = 500,
                    Message = "timeoff job error, message:" + ex.Message
                };
            }
        }

        public override async Task<Protos.ResponseMessage> DailyWorkScheduleJob(JobRequest request, ServerCallContext context)
        {
            try
            {
                return await _schedulerUc.DailyWorkScheduleJob(request, context);
            }
            catch (Exception ex)
            {
                SDLogging.Log("Daily work schedule job error: " + ex.Message, SDLogging.ERROR);
                return new Protos.ResponseMessage
                {
                    Code = 500,
                    Message = "Daily work schedule job error, message:" + ex.Message
                };
            }
        }

        public override async Task<Protos.ResponseMessage> PeriodWorkScheduleJob(JobRequest request, ServerCallContext context)
        {
            try
            {
                return await _schedulerUc.PeriodWorkScheduleJob(request, context);
            }
            catch (Exception ex)
            {
                SDLogging.Log("Period work schedule job error: " + ex.Message, SDLogging.ERROR);
                return new Protos.ResponseMessage
                {
                    Code = 500,
                    Message = "Period work schedule job error, message:" + ex.Message
                };
            }
        }
    }
}
