using Google.Cloud.PubSub.V1;
using System.ComponentModel.DataAnnotations;
using TalentaReceiver.Protos;

namespace TalentaReceiver.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Root
    {
        public Data? data { get; set; }
    }

    public class Data
    {
        public List<Employees> employees { get; set; }
    }

    public class Employees
    {
        public Employee employee { get; set; }
    }

    public class Employee
    {
        public Personal personal { get; set; }
        public Family family { get; set; }
        public Education education { get; set; }
        public Employment employment { get; set; }
        public PayrollInfo payroll_info { get; set; }
        public List<CustomField> custom_field { get; set; }
        public AccessRole access_role { get; set; }
        public List<Sbu> sbu { get; set; }
    }

    public class Personal
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string barcode { get; set; }
        public string email { get; set; }
        public string identity_type { get; set; }
        public string identity_number { get; set; }
        public string expired_date_identity_id { get; set; }
        public string postal_code { get; set; }
        public string address { get; set; }
        public string current_address { get; set; }
        public string birth_place { get; set; }
        public string birth_date { get; set; }
        public string phone { get; set; }
        public string mobile_phone { get; set; }
        public string gender { get; set; }
        public string marital_status { get; set; }
        public string blood_type { get; set; }
        public string religion { get; set; }
        public string avatar { get; set; }
        public string employee_id { get; set; }
    }

    public class Family
    {
        public List<Member> members { get; set; }
        public List<EmergencyContact> emergency_contacts { get; set; }
    }

    public class Member
    {
        public int id { get; set; }
        public string full_name { get; set; }
        public string relationship { get; set; }
        public int relationship_id { get; set; }
        public string birth_date { get; set; }
        public string no_ktp { get; set; }
        public string marital_status { get; set; }
        public string gender { get; set; }
        public string job { get; set; }
        public string religion { get; set; }
        public int is_deleted { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string employee_id { get; set; }
    }

    public class EmergencyContact
    {
        public string? full_name { get; set; }
        public string? relationship { get; set; }
        public string? birth_date { get; set; }
        public string? no_ktp { get; set; }
        public string? marital_status { get; set; }
        public string? gender { get; set; }
        public string? job { get; set; }
        public string? religion { get; set; }
        public string employee_id { get; set; }
    }
    
    public class Employment
    {
        [Required]
        public string employee_id { get; set; }
        public int? company_id { get; set; }
        public int? organization_id { get; set; }
        public string? organization_name { get; set; }
        public int? job_position_id { get; set; }
        public string? job_position { get; set; }
        public int? job_level_id { get; set; }
        [Required]
        public string job_level { get; set; }
        public int? employment_status_id { get; set; }
        public string employment_status { get; set; }
        [Required]
        public int branch_id { get; set; }
        public string? branch { get; set; }
        public string? branch_code { get; set; }
        [Required]
        public string join_date { get; set; }
        public string? length_of_service { get; set; }
        public string? grade { get; set; }
        public string? @class { get; set; }
        public int? approval_line { get; set; }
        public string? approval_line_employee_id { get; set; }
        [Required]
        public string status { get; set; }
        public string? sign_date { get; set; }
        public string? resign_date { get; set; }
        public string? date_of_issue { get; set; }
        public string? country_of_issue { get; set; }
        public List<object>? working_experiences { get; set; }
        public List<Sbu>? sbu { get; set; }
    }

    public class Sbu
    {
        public int field_id { get; set; }
        public string? field_name { get; set; }
        public int? value_id { get; set; }
        public string? value_name { get; set; }
        public string? value_code { get; set; }
        public string employee_id { get; set; }
    }

    public class PayrollInfo
    {
        public string? bpjs_ketenagakerjaan { get; set; }
        public string? bpjs_kesehatan { get; set; }
        public string? npwp { get; set; }
        public int? bank_id { get; set; }
        public string? bank_name { get; set; }
        public string? bank_account { get; set; }
        public string? bank_account_holder { get; set; }
        public string? ptkp_status { get; set; }
        public string? bank_code { get; set; }
        public int? cost_center_id { get; set; }
        public string? cost_center_name { get; set; }
        public int? cost_center_category_id { get; set; }
        public string? cost_center_category_name { get; set; }
        public int? employee_tax_status { get; set; }
        public string? nationality_code { get; set; }
        public string? expatriatedn_date { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
        public string? employee_id { get; set; }
    }
    
    public class AccessRole
    {
        public int id { get; set; }
        public string name { get; set; }
        public int role_id { get; set; }
        public string role_name { get; set; }
        public string role_type { get; set; }
    }

    public class CustomField
    {
        public string field_name { get; set; }
        public string value { get; set; }
        public string employee_id { get; set; }
    }
   
    public class Education
    {
        public List<object> formal_education_history { get; set; }
        public List<object> informal_education_history { get; set; }
    }

    public class EmployeePersonalInfo
    {
        public string EmployeeId { get; set; }
        public string Address { get; set; }
        public string BirthPlace { get; set; }
        public string CountryOfIssue { get; set; }
        public string Religion { get; set; }
        public string JoinDate { get; set; }
    }

}
