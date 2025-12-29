namespace TalentaReceiver.Models
{
    public class OvertimeApprovalModels
    {
        public class DataOvertimeApproval
        {
            public List<OvertimeRequest> overtime_request { get; set; }
            public Pagination pagination { get; set; }
        }

        public class OvertimeRequest
        {
            public int id { get; set; }
            public string description { get; set; }
            public int status_approval { get; set; }
            public string request_date { get; set; }
            public string create_date { get; set; }
            public string approved_by { get; set; }
            public string approval_date { get; set; }
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

        public class RootOvertimeApproval
        {
            public string message { get; set; }
            public DataOvertimeApproval data { get; set; }
        }
    }
}
