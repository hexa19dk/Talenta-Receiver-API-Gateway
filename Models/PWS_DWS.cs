namespace TalentaReceiver.Models
{
    public class RootPwsDws
    {
        public DataPwsDws data { get; set; }
    }

    public class DataPwsDws
    {
        public List<DailyWorkSchedules> daily_work_schedules { get; set; }
        public List<PeriodWorkSchedules> period_work_schedules { get; set; }
    }

    public class DailyWorkSchedules
    {
        public DailyWorkSchedule daily_work_schedule { get; set; }
    }

    //public class DailyWorkSchedule
    //{
    //    public string daily_work_schedule_group { get; set; }
    //    public string daily_work_schedule_code { get; set; }
    //    public string end_date { get; set; }
    //    public string start_date { get; set; }
    //    public string daily_work_schedule_text { get; set; }
    //    public string planed_working_hours { get; set; }
    //    public string work_break_schedule_code { get; set; }
    //    public string employee_id { get; set; }
    //}
    
    public class PeriodWorkSchedules
    {
        public PeriodWorkSchedule period_work_schedule { get; set; }
    }

    public class PeriodWorkSchedule
    {
        public string period_work_schedule_code { get; set; }
        public string work_days { get; set; }
        public string daily_work_schedule_code { get; set; }
        public string period_work_schedule_text { get; set; }
        public List<string> daily_work_schedule { get; set; }
    }

    public class DataPwsDwsList
    {
        //public List<DailyWorkSchedule> DailyWorkScheduleList { get; set; }
        public List<PeriodWorkSchedule> PeriodWorkScheduleList { get; set;}
    }
}
