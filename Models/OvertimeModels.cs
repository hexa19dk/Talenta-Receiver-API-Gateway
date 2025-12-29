using System.Security.Cryptography;

namespace TalentaReceiver.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class RootOvertime
    {
        public string message { get; set; }
        public DataOvertime data { get; set; }
    }

    public class DataOvertime
    {
        public List<SummaryAttendanceReport> summary_attendance_report { get; set; }
        public Pagination pagination { get; set; }
    }

    public class Pagination
    {
        public int current_page { get; set; }
        public string first_page_url { get; set; }
        public int from { get; set; }
        public int last_page { get; set; }
        public string last_page_url { get; set; }
        public string next_page_url { get; set; }
        public string path { get; set; }
        public int per_page { get; set; }
        public string prev_page_url { get; set; }
        public int to { get; set; }
        public int total { get; set; }
    }
    
    public class SummaryAttendanceReport
    {
        public int user_id { get; set; }
        public int company_id { get; set; }
        public string employee_id { get; set; }
        public string full_name { get; set; }
        public string schedule_date { get; set; }
        public string shift_name { get; set; }
        public string attendance_code { get; set; }
        public bool holiday { get; set; }
        public string schedule_in { get; set; }
        public string schedule_out { get; set; }
        public string clock_in { get; set; }
        public string clock_out { get; set; }
        public bool overtime_status { get; set; }
        public string overtime_checkin { get; set; }
        public string overtime_checkout { get; set; }
        public string beforetime_duration { get; set; }
        public string overtime_duration { get; set; }
        public string beforetime_break { get; set; }
        public string overtime_break { get; set; }
        public string timeoff_code { get; set; }
        public object timeoff_id { get; set; }
        public string timeoff_name { get; set; }
        public string overtime { get; set; }
        public string timeoff_schedulein { get; set; }
        public string timeoff_scheduleout { get; set; }
        public string halfday_flag { get; set; }
        public string break_duration { get; set; }
        public bool use_grace_period { get; set; }
        public int clock_in_dispensation_duration { get; set; }
        public int clock_out_dispensation_duration { get; set; }
        public string schedule_break_start { get; set; }
        public string schedule_break_end { get; set; }
        public string break_start { get; set; }
        public string break_end { get; set; }
        public int late_in { get; set; }
        public int early_out { get; set; }
        public int scheduled_work_hour { get; set; }
        public int actual_work_hour { get; set; }
        public int real_work_hour { get; set; }
        public int overtime_before { get; set; }
        public int overtime_after { get; set; }
        public object approval_line_user_id { get; set; }
        public object approval_line_fullname { get; set; }
        public object approval_line_employee_id { get; set; }
        public object approval_line_email { get; set; }
        public object manager_line_user_id { get; set; }
        public object manager_line_fullname { get; set; }
        public object manager_line_employee_id { get; set; }
        public object manager_line_email { get; set; }
    }

    public class OvertimeGradeRule
    {
        public List<string> Grades { get; set; }
        public int MinHoursPerDay { get; set; }
        public int MaxHoursPerWeek { get; set; }
        public string Class { get; set; }
    }   

}


