using AutoMapper;
using Google.Apis.Auth.OAuth2;

namespace TalentaReceiver.Mappers
{
    public class EmployeeMapper : Profile
    {
        public EmployeeMapper() 
        { 
            CreateMap<Protos.Root, Models.Root>().ReverseMap();
            CreateMap<Protos.Data, Models.Data>().ReverseMap();
            CreateMap<Protos.Employees, Models.Employees>()
                .ForMember(d => d.employee, opt => opt.MapFrom(src => src.Employee));

            CreateMap<Protos.Employee, Models.Employee>()
                .ForMember(d => d.personal, opt => opt.MapFrom(src => src.Personal))
                .ForMember(d => d.family, opt => opt.MapFrom(src => src.Family))
                .ForMember(d => d.education, opt => opt.MapFrom(src => src.Education))
                .ForMember(d => d.employment, opt => opt.MapFrom(src => src.Employment))
                .ForMember(d => d.payroll_info, opt => opt.MapFrom(src => src.PayrollInfo))
                .ForMember(d => d.custom_field, opt => opt.MapFrom(src => src.CustomField))
                .ForMember(d => d.access_role, opt => opt.MapFrom(src => src.AccessRole));
                
            CreateMap<Protos.Personal, Models.Personal>()
                .ForMember(dest => dest.first_name, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.last_name, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.barcode, opt => opt.MapFrom(src => src.Barcode))
                .ForMember(dest => dest.email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.identity_type, opt => opt.MapFrom(src => src.IdentityType))
                .ForMember(dest => dest.identity_number, opt => opt.MapFrom(src => src.IdentityNumber))
                .ForMember(dest => dest.expired_date_identity_id, opt => opt.MapFrom(src => src.ExpiredDateIdentityId))
                .ForMember(dest => dest.postal_code, opt => opt.MapFrom(src => src.PostalCode))
                .ForMember(dest => dest.address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.current_address, opt => opt.MapFrom(src => src.CurrentAddress))
                .ForMember(dest => dest.birth_place, opt => opt.MapFrom(src => src.BirthPlace))
                .ForMember(dest => dest.birth_date, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.mobile_phone, opt => opt.MapFrom(src => src.MobilePhone))
                .ForMember(dest => dest.gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.marital_status, opt => opt.MapFrom(src => src.MaritalStatus))
                .ForMember(dest => dest.blood_type, opt => opt.MapFrom(src => src.BloodType))
                .ForMember(dest => dest.religion, opt => opt.MapFrom(src => src.Religion))
                .ForMember(dest => dest.avatar, opt => opt.MapFrom(src => src.Avatar));

            CreateMap<Protos.Family, Models.Family>()
                .ForMember(d => d.members, opt => opt.MapFrom(src => src.Members))
                .ForMember(d => d.emergency_contacts, opt => opt.MapFrom(src => src.EmergencyContact));

            CreateMap<Protos.EmergencyContacts, Models.EmergencyContact>()
                .ForMember(d => d.full_name, opt => opt.MapFrom(src => src.FullName))
                .ForMember(d => d.relationship, opt => opt.MapFrom(src => src.Relationship))
                .ForMember(d => d.birth_date, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(d => d.no_ktp, opt => opt.MapFrom(src => src.NoKtp))
                .ForMember(d => d.marital_status, opt => opt.MapFrom(src => src.MaritalStatus))
                .ForMember(d => d.gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(d => d.job, opt => opt.MapFrom(src => src.Job))
                .ForMember(d => d.religion, opt => opt.MapFrom(src => src.Religion));

            CreateMap<Protos.Members, Models.Member>()
                .ForMember(d => d.full_name, opt => opt.MapFrom(src => src.FullName))
                .ForMember(d => d.relationship, opt => opt.MapFrom(src => src.Relationship))
                .ForMember(d => d.relationship_id, opt => opt.MapFrom(src => src.RelationshipId))
                .ForMember(d => d.birth_date, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(d => d.marital_status, opt => opt.MapFrom(src => src.MaritalStatus))
                .ForMember(d => d.gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(d => d.job, opt => opt.MapFrom(src => src.Job))
                .ForMember(d => d.religion, opt => opt.MapFrom(src => src.Religion))
                .ForMember(d => d.is_deleted, opt => opt.MapFrom(src => src.IsDeleted))
                .ForMember(d => d.created_at, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(d => d.updated_at, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(d => d.no_ktp, opt => opt.MapFrom(src => src.NoKtp));

            CreateMap<Protos.Education, Models.Education>()
                .ForMember(d => d.formal_education_history, opt => opt.MapFrom(src => src.FormalEducationHistory))
                .ForMember(d => d.informal_education_history, opt => opt.MapFrom(src => src.InformalEducationHistory));

            CreateMap<Protos.Employment, Models.Employment>()
                .ForMember(d => d.employee_id, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(d => d.company_id, opt => opt.MapFrom(src => src.CompanyId))
                .ForMember(d => d.organization_id, opt => opt.MapFrom(src => src.OrganizationId))
                .ForMember(d => d.organization_name, opt => opt.MapFrom(src => src.OrganizationName))
                .ForMember(d => d.job_position_id, opt => opt.MapFrom(src => src.JobPositionId))
                .ForMember(d => d.job_position, opt => opt.MapFrom(src => src.JobPosition))
                .ForMember(d => d.job_level, opt => opt.MapFrom(src => src.JobLevel))
                .ForMember(d => d.employment_status, opt => opt.MapFrom(src => src.EmploymentStatus))
                //.ForMember(d => d.end_date, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(d => d.branch_id, opt => opt.MapFrom(src => src.BranchId))
                .ForMember(d => d.branch, opt => opt.MapFrom(src => src.Branch))
                .ForMember(d => d.join_date, opt => opt.MapFrom(src => src.JoinDate))
                .ForMember(d => d.grade, opt => opt.MapFrom(src => src.Grade))
                .ForMember(d => d.@class, opt => opt.MapFrom(src => src.Class))
                .ForMember(d => d.approval_line, opt => opt.MapFrom(src => src.ApprovalLine))
                .ForMember(d => d.status, opt => opt.MapFrom(src => src.Status))
                .ForMember(d => d.resign_date, opt => opt.MapFrom(src => src.ResignDate))
                //.ForMember(d => d.sbu, opt => opt.MapFrom(src => src.Sbu))
                .ForMember(d => d.sign_date, opt => opt.MapFrom(src => src.SignDate))
                .ForMember(d => d.employment_status_id, opt => opt.MapFrom(src => src.EmploymentStatusId))
                .ForMember(d => d.approval_line_employee_id, opt => opt.MapFrom(src => src.ApprovalLineEmployeeId))
                .ForMember(d => d.date_of_issue, opt => opt.MapFrom(src => src.DateOfIssue))
                .ForMember(d => d.country_of_issue, opt => opt.MapFrom(src => src.CountryOfIssue))
                .ForMember(d => d.job_level_id, opt => opt.MapFrom(src => src.JobLevelId))
                .ForMember(d => d.working_experiences, opt => opt.MapFrom(src => src.WorkingExperiences));

            CreateMap<Protos.PayrollInfo, Models.PayrollInfo>()
                .ForMember(d => d.cost_center_id, opt => opt.MapFrom(src => src.CostCenterId))
                .ForMember(d => d.cost_center_name, opt => opt.MapFrom(src => src.CostCenterName))
                .ForMember(d => d.cost_center_category_id, opt => opt.MapFrom(src => src.CostCenterCategoryId))
                .ForMember(d => d.bpjs_ketenagakerjaan, opt => opt.MapFrom(src => src.BpjsKetenagakerjaan))
                .ForMember(d => d.bpjs_kesehatan, opt => opt.MapFrom(src => src.BpjsKesehatan))
                .ForMember(d => d.npwp, opt => opt.MapFrom(src => src.Npwp))
                .ForMember(d => d.bank_id, opt => opt.MapFrom(src => src.BankId))
                .ForMember(d => d.bank_name, opt => opt.MapFrom(src => src.BankName))
                .ForMember(d => d.bank_account, opt => opt.MapFrom(src => src.BankAccount))
                .ForMember(d => d.bank_account_holder, opt => opt.MapFrom(src => src.BankAccountHolder))
                .ForMember(d => d.ptkp_status, opt => opt.MapFrom(src => src.PtkpStatus))
                .ForMember(d => d.bank_code, opt => opt.MapFrom(src => src.BankCode))
                .ForMember(d => d.employee_tax_status, opt => opt.MapFrom(src => src.EmployeeTaxStatus))
                .ForMember(d => d.nationality_code, opt => opt.MapFrom(src => src.NationalityCode))
                .ForMember(d => d.expatriatedn_date, opt => opt.MapFrom(src => src.ExpatriatednDate))
                .ForMember(d => d.created_at, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(d => d.updated_at, opt => opt.MapFrom(src => src.UpdatedAt));

            CreateMap<Protos.CustomField, Models.CustomField>()
                .ForMember(d => d.field_name, opt => opt.MapFrom(src => src.FieldName))
                .ForMember(d => d.value, opt => opt.MapFrom(src => src.Value));

            CreateMap<Protos.AccessRole, Models.AccessRole>()
                .ForMember(d => d.id, opt => opt.MapFrom(src => src.Id))
                .ForMember(d => d.name, opt => opt.MapFrom(src => src.Name))
                .ForMember(d => d.role_id, opt => opt.MapFrom(src => src.RoleId))
                .ForMember(d => d.role_name, opt => opt.MapFrom(src => src.RoleName))
                .ForMember(d => d.role_type, opt => opt.MapFrom(src => src.RoleType));

            CreateMap<Protos.Sbu, Models.Sbu>()
                .ForMember(d => d.field_id, opt => opt.MapFrom(src => src.FieldId))
                .ForMember(d => d.field_name, opt => opt.MapFrom(src => src.FieldName))
                .ForMember(d => d.value_id, opt => opt.MapFrom(src => src.ValueId))
                .ForMember(d => d.value_name, opt => opt.MapFrom(src => src.ValueName))
                .ForMember(d => d.value_code, opt => opt.MapFrom(src => src.ValueCode));
        }
    }
}
