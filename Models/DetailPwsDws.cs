namespace TalentaReceiver.Models
{
    public class RootEventPwsDws
    {
        public DataEventPwsDws data { get; set; }
    }

    public class DataEventPwsDws
    {
        public List<EmployeePwsDwsDetail> employees { get; set; }
    }

    public class EmployeePwsDwsDetail
    {
        public List<EmployeesPwsDwsDetail2> employee { get; set; }
    }

    public class EmployeesPwsDwsDetail2
    {
        public string employee_id { get; set; }
        public string start_date { get; set; }
        public int month_hours { get; set; }
        public double week_hours { get; set; }
        public int daily_hours { get; set; }
        public int week_days { get; set; }
        public string work_schedule_rule { get; set; }
        public string work_break_schedule { get; set; }
    }

    public class PwsDwsDetailDetail
    {
        public string employee_id { get; set; }
        public string daily_work_schedule_code { get; set; }
        public string planned_working_hours { get; set; }
        public string planned_working_time { get; set; }
        public string start_date { get; set; }
    }

    public class DataEmpPwsDws
    {
        public List<EmployeesPwsDwsDetail2> EmployeesPwsDwsDetails { get; set; }
        public List<PwsDwsDetailDetail> PwsDwsDetailDetails { get; set; }
    }
}
