namespace TalentaReceiver.Models
{
    public class HiringDTO
    {
        public string employeenumber { get; set; } // Personal Number 
        public string hiringdate { get; set; } // Start Date
        public string actiontype { get; set; } // Action Type
        public string reasonforaction { get; set; } // Reason for Action
        public string werks { get; set; } // Personal Area
        public string btrtl { get; set; } // Personal Subarea
        public string persg { get; set; } // Employee Group 
        public string persk { get; set; } // Employee Subgroup
        public string abkrs { get; set; } // Payroll Area
        public string ansvh { get; set; } // Work Contract
        public string plans { get; set; } // Position
        public string stell { get; set; } // Jobs
        public string sachz { get; set; } // Administrator for Time Recording
        public string created_at { get; set; }
        public string created_by { get; set; }
        public string status { get; set; }
        public string response_message { get; set; }
    }

    public class PersonalDataDTO
    {
        public string pernr { get; set; } // Personal Number
        public string begda { get; set; } // date "yyyymmdd"
        public string endda { get; set; } // date "yyyymmdd"
        public string cname { get; set; } // Complete Name
        public string anred { get; set; } // Form-of-Address Key
        public string sprsl { get; set; } // Communication Language
        public string gbpas { get; set; } // date "yyyymmdd"
        public string gbort { get; set; } // Birthplace
        public string gblnd { get; set; } // Country of Birth
        public string natio { get; set; } // Nationality
        public string famst { get; set; } // Marital Status Key
        public string famdt { get; set; } // Valid From Date of Current Marital Status
        public string konfe { get; set; } // Religious Denomination Key
        public string gesch { get; set; } // Gender Key
        public string created_at { get; set; }
        public string created_by { get; set; }
        public string status { get; set; }
        public string response_message { get; set; }
    }
}
