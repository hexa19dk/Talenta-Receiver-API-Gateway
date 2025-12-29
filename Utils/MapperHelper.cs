using TalentaReceiver.Models;

namespace TalentaReceiver.Utils
{
    public class MapperHelper
    {
        public static string GetSalutation(string gender, string maritalStatus)
        {
            if (!Enum.TryParse<MaritalStatusEnum>(maritalStatus.Replace(" ", "").Replace(".", ""), true, out var maritalStatusCode))
                return string.Empty;

            if (gender == "Female")
            {
                return maritalStatusCode == MaritalStatusEnum.Single ? "2" :
                       maritalStatusCode == MaritalStatusEnum.Married ? "3" : "";
            }
            else if (gender == "Male")
            {
                return "1";
            }

            return string.Empty;
        }

        public static Models.PayrollInfo MapPayrollInfoFromProto(Protos.PayrollInfo protoPayroll)
        {
            return new Models.PayrollInfo
            {
                nationality_code = protoPayroll.NationalityCode,
                employee_tax_status = protoPayroll.EmployeeTaxStatus
            };
        }

        public static string GetNationalityCode(PayrollInfo payrollInfo)
        {
            string? codeValue = typeof(NationalityCodeEnum)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .FirstOrDefault(f => f.Name == payrollInfo.nationality_code)?.GetValue(null)?.ToString();

            return (payrollInfo.employee_tax_status == 4 || payrollInfo.employee_tax_status == 5)
                ? codeValue ?? "ID"
                : "ID";
        }

        public static string GetMaritalStatusCode(string maritalStatus)
        {
            return Enum.TryParse<MaritalStatusEnum>(maritalStatus.Replace(" ", "").Replace(".", ""), true, out var statusCode)
                ? ((int)statusCode).ToString()
                : string.Empty;
        }

        public static string GetReligionCode(string religion)
        {
            return Enum.TryParse<ReligionEnum>(religion.Replace(" ", "").Replace(".", ""), true, out var religionCode)
                ? ((int)religionCode).ToString()
                : string.Empty;
        }

        public static List<Models.CustomField> MapCustomFields(IEnumerable<Protos.CustomField> reqCustomField, string employeeId)
        {
            return reqCustomField.Select(field => new Models.CustomField
            {
                field_name = field.FieldName,
                value = field.Value,
                employee_id = employeeId
            }).ToList();
        }

        public static List<Models.Sbu> MapSbuFields(IEnumerable<Protos.Sbu> sbuFields, string employeeId)
        {
            return sbuFields.Select(sbu => new Models.Sbu
            {
                field_id = (int)sbu.FieldId!,
                field_name = sbu.FieldName,
                value_id = sbu.ValueId,
                value_name = sbu.ValueName,
                value_code = sbu.ValueCode,
                employee_id = employeeId
            }).ToList();
        }

        public static string ExtractValueCode(IEnumerable<Models.Sbu> sbuList, string fieldName)
        {
            return sbuList.FirstOrDefault(x => x.field_name == fieldName)?.value_code ?? string.Empty;
        }

        public static string ExtractValueName(IEnumerable<Models.Sbu> sbuList, string fieldName)
        {
            return sbuList.FirstOrDefault(x => x.field_name == fieldName)?.value_name ?? string.Empty;
        }

        public static string ExtractReasonForAction(IEnumerable<Models.Sbu> sbuList)
        {
            var value = sbuList.FirstOrDefault(x => x.field_name!.StartsWith("Reason For Action"))?.value_code;
            return value?.Split('-')[0].Trim() ?? string.Empty;
        }
    }
}
