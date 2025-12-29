namespace TalentaReceiver.Models
{
    public class RootMasterDws
    {
        public DataDailyWorkSchedule data { get; set; }
    }

    public class DataDailyWorkSchedule
    {
        public List<DailyWorkSchedule> DailyWorkSchedules { get; set; }
    }

    public class DailyWorkSchedule
    {
        public Int32 daily_work_schedule_group { get; set; }
        public string daily_work_schedule_code { get; set; }
        public string end_date { get; set; }
        public string start_date { get; set; }
        public string daily_work_schedule_text { get; set; }
        public Int32 planed_working_hours { get; set; }
        public string work_break_schedule_code { get; set; }
        public string employee_id { get; set; }
    }
}
