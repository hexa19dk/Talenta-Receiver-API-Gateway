namespace TalentaReceiver.Models
{
    public class RootMasterPws
    {
        public DataPeriodWorkSchedule data { get; set; }
    }

    public class DataPeriodWorkSchedule
    {
        public PeriodWorkSchedules PeriodWorkSchedules { get; set; }
    }

    public class PeriodWorkSchedules
    {
        public List<PeriodWorkSchedule> PeriodWorkSchedule { get; set; }
    }

    public class PeriodWorkSchedule
    {
        public string period_work_schedule_code { get; set; }
        public Int32 work_days { get; set; }
        public string daily_work_schedule_code { get; set; }
        public string period_work_schedule_text { get; set; }
        public List<string> daily_work_schedule { get; set; }
    }
}
