using System.Security.Cryptography;

namespace TalentaReceiver.Models
{
    public class RootTimeoff
    {
        public DataTimeoff data { get; set; }
    }

    public class DataTimeoff
    {
        public List<TimeOff> time_off { get; set; }
        public string total { get; set; }
    }
   
    public class ApprovalList
    {
        public int id { get; set; }
        public string pic { get; set; }
        public int approval_level { get; set; }
        public string reason { get; set; }
        public string updated_date { get; set; }
        public string status { get; set; }
    }

    public class TimeOff
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public string employee_id { get; set; }
        public int policy_id { get; set; }
        public string policy_name { get; set; }
        public string policy_code { get; set; }
        public string reason { get; set; }
        public string filename { get; set; }
        public string request_type { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string requested_date { get; set; }
        public string status { get; set; }
        public string updated_date { get; set; }
        public int inboxId { get; set; }
        public List<ApprovalList> approval_list { get; set; }
        public int days { get; set; }
    }

    public class TimeoffRangeSummary
    {
        public int RangeIndex { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int TotalRecords { get; set; }
    }

}
