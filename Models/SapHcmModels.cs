using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Moq;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace TalentaReceiver.Models
{
    public class SapHcmModels
    {
    }

    public class SapPwsRfc // mapping sap master dws
    {
        public string? pernr { get; set; } // personel_number 
        public string? begda { get; set; } // start_date 
        public string? endda { get; set; }  // end_date 
        public string? schkz { get; set; } // work_schedule_rule 
        public string? zterf { get; set; } // employee_time_management_status
    }

    public class SapPwsRfcDTO
    {
        public SapPwsRfc sapPwsRfc { get; set; }
        public ResponseMetadata responseMetadata { get; set; }
    }

    public class SapDws // mapping sap event employee
    {
        public string? pernr { get; set; } //personel_number
        public string? begda { get; set; } //start_date
        public string? endda { get; set; } //end_date
        public string? beguz { get; set; } //start_time
        public string? enduz { get; set; } //end_time
    }

    public class SapDwsDTO
    {
        public SapDws sapDws { get; set; }
        public ResponseMetadata responseMetadata { get; set; }
    }

    public class ZihrPostInitHireRfc
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
    }

    public class ZihrPostInitHireRfcDTO
    {
        public ZihrPostInitHireRfc zihrPostInitHireRfc { get; set; }
        public ResponseMetadata responseMetadata { get; set; }
    }

    public class ZihrPostPrsnDataRfc
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
    }

    public class ZihrPostPrsnDataRfcDTO
    {
        public ZihrPostPrsnDataRfc zihrPostPrsnDataRfc { get; set; }
        public ResponseMetadata responseMetadata { get; set; }
    }

    public class ZiHrPostAbsenRfc
    {
        public string pernr { get; set; }
        public string begda { get; set; }
        public string endda { get; set; }
        public string subty { get; set; }
    }

    public class ZiHrPostAbsenRfcLogDTO
    {
        public ZiHrPostAbsenRfc ziHrPostAbsenRfc { get; set; }
        public ResponseMetadata responseMetadata { get; set; }
    }

    
    public class OvertimeSAP
    {
        public string pernr { get; set; }
        public string begda { get; set; }
        public string endda { get; set; }
        public string ktart { get; set; }
        public string beguz { get; set; }
        public string enduz { get; set; }
    }

    public class OvertimeSAPLogDTO
    {
        public OvertimeSAP overtimeSAP { get; set; }
        public ResponseMetadata responseMetadata { get; set; }
    }

    public class ResponseMetadata
    {
        public string created_at { get; set; }
        public string created_by { get; set; }
        public string status { get; set; }
        public string response_message { get; set; }
    }

    
    public class StartEndDateDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    

}
