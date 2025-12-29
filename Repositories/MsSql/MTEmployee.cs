using Dapper;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using TalentaReceiver.Config;
using TalentaReceiver.Models;

namespace TalentaReceiver.Repositories.MsSql
{
    public interface IMTEmployee
    {
        Task<bool> AddEmployees(Models.Employee e);
        Task<bool> UpdateEmployees(Models.Employee e);
        Task<List<string>> GetEmployeeId();
        Task<string> CheckEmploymentId(string empId);
        Task<EmployeePersonalInfo> GetEmploymentTalenta(string empId);
        Task<bool> UpdateEmployeeResign(string empId, string status, string resignDate);

    }

    public class MTEmployee: IMTEmployee
    {
        #region SqlCommand 

        #region Employment
        string table_employment = "employment";

        string field_employment = "employee_id, company_id, organization_id, organization_name, job_position_id, job_level_id, job_level, employment_status_id, employment_status, branch_id, branch, join_date, length_of_service, grade, approval_line, approval_line_employee_id, status, sign_date, resign_date, date_of_issue, country_of_issue, class";

        string field_insert_employment = "@employee_id, @company_id, @organization_id, @organization_name, @job_position_id, @job_level_id, @job_level, @employment_status_id, @employment_status, @branch_id, @branch, @join_date, @length_of_service, @grade, @approval_line,  @approval_line_employee_id, @status, @sign_date, @resign_date, @date_of_issue, @country_of_issue, @class";

        string field_update_employment = "company_id = @company_id, organization_id = @organization_id, organization_name = @organization_name, job_position_id = @job_position_id, job_level_id = @job_level_id, job_level = @job_level, employment_status_id = @employment_status_id, employment_status = @employment_status, branch_id = @branch_id, branch = @branch, join_date = @join_date, length_of_service = @length_of_service, grade = @grade, approval_line = @approval_line, approval_line_employee_id = @approval_line_employee_id, status = @status, sign_date = @sign_date, resign_date = @resign_date, date_of_issue = @date_of_issue, country_of_issue = @country_of_issue, class = @class";
        #endregion

        #region Sbu
        string table_sbu = "sbu";
        string field_sbu = "field_id, field_name, value_id, value_name, value_code, employee_id";
        string field_insert_sbu = "@field_id, @field_name, @value_id, @value_name, @value_code, @employee_id";
        string field_update_sbu = "field_name = @field_name, value_id = @value_id, value_name = @value_name, value_code = @value_code";
        #endregion

        #region Personal
        string table_personal = "personal";
        
        string field_personal = "first_name, last_name, barcode, email, identity_type,  identity_number, expired_date_identity_id, postal_code, address, current_address, birth_place, birth_date, phone, mobile_phone, gender, marital_status, blood_type, religion, avatar, employee_id";
        
        string field_insert_personal = "@first_name, @last_name, @barcode, @email, @identity_type, @identity_number, @expired_date_identity_id, @postal_code, @address, @current_address, @birth_place, @birth_date, @phone, @mobile_phone, @gender, @marital_status, @blood_type, @religion, @avatar, @employee_id";

        string field_update_personal = "first_name = @first_name, last_name = @last_name, barcode = @barcode, email = @email, identity_type = @identity_type, identity_number = @identity_number, expired_date_identity_id = @expired_date_identity_id, postal_code = @postal_code, address = @address, current_address = @current_address, birth_place = @birth_place, birth_date = @birth_date, phone = @phone, mobile_phone = @mobile_phone, gender = @gender, marital_status = @marital_status, blood_type = @blood_type, religion = @religion, avatar = @avatar";
        #endregion

        #region Family Member 
        string table_family_member = "family_member";

        string field_family_member = "full_name, relationship, relationship_id, birth_date, no_ktp, marital_status, gender, job, religion, is_deleted, created_at, updated_at, employee_id";

        string field_insert_family_member = "@full_name, @relationship, @relationship_id, @birth_date, @no_ktp, @marital_status, @gender, @job, @religion, @is_deleted, @created_at, @updated_at, @employee_id";

        string field_update_family_member = "full_name = @full_name, relationship = @relationship, relationship_id = @relationship_id, birth_date = @birth_date, marital_status = @marital_status, gender = @gender, job = @job, religion = @religion, is_deleted = @is_deleted, updated_at = @updated_at, no_ktp = @no_ktp";
        #endregion

        #region Emergency Contact 
        string table_emergency_contact = "emergency_contact";
        string field_emergency_contact = "full_name, relationship, birth_date, no_ktp, marital_status, gender, job, religion, employee_id";
        string field_insert_emergency_contact = "@full_name, @relationship, @birth_date, @no_ktp, @marital_status, @gender, @job, @religion, @employee_id";
        string field_update_emergency_contact = "full_name = @full_name, relationship = @relationship, birth_date = @birth_date, no_ktp = @no_ktp, marital_status = @marital_status, gender = @gender, job = @job, religion = @religion";
        #endregion

        #region Payroll Info 
        string table_payroll_info = "payroll_info";

        string field_payroll_info = "bpjs_ketenagakerjaan, bpjs_kesehatan, npwp, bank_id, bank_name, bank_account, bank_account_holder, ptkp_status, bank_code, cost_center_id, cost_center_name, cost_center_category_id, cost_center_category_name, employee_tax_status, nationality_code, expatriatedn_date, created_at, updated_at, employee_id";

        string field_insert_payroll_info = "@bpjs_ketenagakerjaan, @bpjs_kesehatan, @npwp, @bank_id, @bank_name, @bank_account, @bank_account_holder, @ptkp_status, @bank_code, @cost_center_id, @cost_center_name, @cost_center_category_id, @cost_center_category_name, @employee_tax_status, @nationality_code, @expatriatedn_date, @created_at, @updated_at, @employee_id";

        string field_update_payroll_info = "bpjs_ketenagakerjaan = @bpjs_ketenagakerjaan, bpjs_kesehatan = @bpjs_kesehatan, npwp = @npwp, bank_id = @bank_id, bank_name = @bank_name, bank_account = @bank_account, bank_account_holder = @bank_account_holder, ptkp_status = @ptkp_status, bank_code = @bank_code, cost_center_id = @cost_center_id, cost_center_name = @cost_center_name, cost_center_category_id = @cost_center_category_id, cost_center_category_name = @cost_center_category_name, employee_tax_status = @employee_tax_status, nationality_code = @nationality_code, expatriatedn_date = @expatriatedn_date, updated_at = @updated_at";
        #endregion

        #region Custom Field
        string table_custom_field = "custom_field";
        string field_custom_field = "field_name, value, employee_id";
        string field_insert_custom_field = "@field_name, @value, @employee_id";
        #endregion

        #endregion

        private readonly IDbConnectionFactory _connFactory;        
        public MTEmployee(IDbConnectionFactory connFactory)
        {
            _connFactory = connFactory ?? throw new ArgumentNullException(nameof(connFactory));            
        }

        public async Task<List<string>> GetEmployeeId()
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            try
            {
                string query = "SELECT employee_id FROM employment ORDER BY id DESC";
                var result = await _mysqlConn.QueryAsync<string>(query);

                return result.ToList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<EmployeePersonalInfo> GetEmploymentTalenta(string empId)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            try
            {
                var query = @"SELECT p.employee_id, p.address, p.birth_place, e.country_of_issue, p.religion, e.join_date 
                    FROM personal p
                    LEFT JOIN employment e ON e.employee_id = p.employee_id
                    WHERE p.employee_id = @EmployeeId";

                return await _mysqlConn.QueryFirstOrDefaultAsync<EmployeePersonalInfo>(query, new { EmployeeId = empId });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> CheckSbuData(string empId, int fieldId)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            try
            {
                var query = @"SELECT employee_id, field_id FROM sbu WHERE employee_id = @employeeId AND field_id = @fieldId";
                var res =  await _mysqlConn.QueryFirstOrDefaultAsync<string>(query, new
                {
                    employeeId = empId,
                    fieldId = fieldId
                });

                return res != null;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> CheckEmploymentId(string empId)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            try
            {
                var query = @"SELECT employee_id FROM employment WHERE employee_id = @employeeId";
                return  await _mysqlConn.QueryFirstOrDefaultAsync<string>(query, new { employeeId = empId });
            }
            catch
            {
                throw;
            }
        }

        #region Create Employee Section
        public async Task<bool> AddEmployees(Models.Employee e)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            var transaction = _mysqlConn.BeginTransaction();

            try
            {
                await AddEmployment(e.employment, transaction);
                await AddPersonal(e.personal, transaction);
                await AddSbu(e.sbu!, transaction);
                await AddFamilyMember(e.family, transaction);
                await AddEmergencyContact(e.family, transaction);
                await AddPayrollInfo(e.payroll_info, transaction);
                await AddCustomField(e.custom_field, transaction);

                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task AddEmployment(Models.Employment emp, IDbTransaction transaction)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            try
            {
                string query = $"insert into {table_employment} ({field_employment}) values ({field_insert_employment})";

                await _mysqlConn.ExecuteAsync(query, new
                {
                     employee_id                = emp.employee_id,     
                     company_id                 = emp.company_id,                     
                     organization_id            = emp.organization_id,
                     organization_name          = emp.organization_name, 
                     job_position_id            = emp.job_position_id,
                     job_level_id               = emp.job_level_id,                      
                     job_level                  = emp.job_level,                      
                     employment_status_id       = emp.employment_status_id,
                     employment_status          = emp.employment_status,
                     branch_id                  = emp.branch_id,                      
                     branch                     = emp.branch,                  
                     join_date                  = emp.join_date,                     
                     length_of_service          = emp.length_of_service,          
                     grade                      = emp.grade,                     
                     approval_line              = emp.approval_line,                      
                     approval_line_employee_id  = emp.approval_line_employee_id,
                     status                     = emp.status,                     
                     sign_date                  = emp.sign_date,                      
                     resign_date                = emp.resign_date,                        
                     date_of_issue              = emp.date_of_issue,                      
                     country_of_issue           = emp.country_of_issue,
                     @class                     = emp.@class                  
                }, transaction);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task AddSbu(List<Models.Sbu> sbu, IDbTransaction transaction)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            try
            {
                string query = $"insert into {table_sbu} ({field_sbu}) values ({field_insert_sbu})";
                foreach (var lsbu in sbu)
                {
                    await _mysqlConn.ExecuteAsync(query, new
                    {
                        field_id = lsbu.field_id,
                        field_name = lsbu.field_name,
                        value_id = lsbu.value_id,
                        value_name = lsbu.value_name,
                        value_code = lsbu.value_code,
                        employee_id = lsbu.employee_id,
                    }, transaction);
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task AddPersonal(Models.Personal p, IDbTransaction transaction)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            try
            {
                string query = $"insert into {table_personal} ({field_personal}) values ({field_insert_personal})";                
                await _mysqlConn.ExecuteAsync(query, new
                {
                    first_name                  = p.first_name,
                    last_name                   = p.last_name,
                    barcode                     = p.barcode,
                    email                       = p.email,
                    identity_type               = p.identity_type,
                    identity_number             = p.identity_number,
                    expired_date_identity_id    = p.expired_date_identity_id,
                    postal_code                 = p.postal_code,
                    address                     = p.address,
                    current_address             = p.current_address,
                    birth_place                 = p.birth_place,
                    birth_date                  = p.birth_date,
                    phone                       = p.phone,
                    mobile_phone                = p.mobile_phone,
                    gender                      = p.gender,
                    marital_status              = p.marital_status, 
                    blood_type                  = p.blood_type,
                    religion                    = p.religion,
                    avatar                      = p.avatar,
                    employee_id                 = p.employee_id,
                }, transaction);
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task AddFamilyMember(Models.Family f, IDbTransaction transaction)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            try
            {
                string query = $"insert into {table_family_member} ({field_family_member}) values ({field_insert_family_member})";
                foreach (var lMembers in f.members)
                {
                    await _mysqlConn.ExecuteAsync(query, new {
                        full_name           = lMembers.full_name,
                        relationship        = lMembers.relationship,
                        relationship_id     = lMembers.relationship_id,
                        birth_date          = lMembers.birth_date,
                        marital_status      = lMembers.marital_status,
                        gender              = lMembers.gender,
                        job                 = lMembers.job,
                        religion            = lMembers.religion,
                        is_deleted          = lMembers.is_deleted,
                        created_at          = lMembers.created_at,
                        updated_at          = lMembers.updated_at,
                        no_ktp              = lMembers.no_ktp,
                        employee_id         = lMembers.employee_id,
                    }, transaction);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task AddEmergencyContact(Models.Family f, IDbTransaction transaction)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            try
            {
                string query = $"insert into {table_emergency_contact} ({field_emergency_contact}) values ({field_insert_emergency_contact})";

                foreach (var lEmContact in f.emergency_contacts)
                {
                    await _mysqlConn.ExecuteAsync(query, new
                    {
                        full_name = lEmContact.full_name,
                        relationship = lEmContact.relationship,
                        birth_date = lEmContact.birth_date,
                        no_ktp = lEmContact.no_ktp,
                        marital_status = lEmContact.marital_status,
                        gender = lEmContact.gender,
                        job = lEmContact.job,
                        religion = lEmContact.religion,
                        employee_id = lEmContact.employee_id
                    }, transaction);
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task AddPayrollInfo(Models.PayrollInfo pi, IDbTransaction transaction)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            try
            {
                string query = $"insert into {table_payroll_info} ({field_payroll_info}) values ({field_insert_payroll_info})";
                await _mysqlConn.ExecuteAsync(query, new
                {
                    bpjs_ketenagakerjaan = pi.bpjs_ketenagakerjaan,
                    bpjs_kesehatan = pi.bpjs_kesehatan,
                    npwp = pi.npwp,
                    bank_id = pi.bank_id,
                    bank_name = pi.bank_name,
                    bank_account = pi.bank_account,
                    bank_account_holder = pi.bank_account_holder,
                    ptkp_status = pi.ptkp_status,
                    bank_code = pi.bank_code,
                    cost_center_id = pi.cost_center_id,
                    cost_center_name = pi.cost_center_name,
                    cost_center_category_id = pi.cost_center_category_id,
                    cost_center_category_name = pi.cost_center_category_name,
                    employee_tax_status = pi.employee_tax_status,
                    nationality_code = pi.nationality_code,
                    expatriatedn_date = pi.expatriatedn_date,
                    created_at = pi.created_at,
                    updated_at = pi.updated_at,
                    employee_id = pi.employee_id
                }, transaction);
            }
            catch
            {
                throw;
            }
        }

        public async Task AddCustomField(List<Models.CustomField> cf, IDbTransaction transaction)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            try
            {
                string query = $"insert into {table_custom_field} ({field_custom_field}) values ({field_insert_custom_field})";                
                foreach (var lcf in cf)
                {
                    await _mysqlConn.ExecuteAsync(query, new
                    {
                        field_name = lcf.field_name,
                        value = lcf.value,
                        employee_id = lcf.employee_id,
                    }, transaction);
                }
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Update Employee Section

        public async Task<bool> UpdateEmployees(Models.Employee e)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            var transaction = _mysqlConn.BeginTransaction();

            try
            {
                await UpdateEmployment(e.employment, transaction);
                await UpdatePersonal(e.personal, transaction);
                await UpdateSbu(e.sbu!, transaction);
                await UpdateFamilyMember(e.family, transaction);
                await UpdateEmergencyContact(e.family, transaction);
                await UpdatePayrollInfo(e.payroll_info, transaction);
                await UpdateCustomField(e.custom_field, transaction);

                transaction.Commit();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateEmployment(Models.Employment emp, IDbTransaction transaction)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            
            try
            {
                string queryUpdate = $"update {table_employment} set {field_update_employment} where employee_id = @employee_id";
                await _mysqlConn.ExecuteAsync(queryUpdate, new
                {
                    emp.company_id,
                    emp.organization_id,
                    emp.organization_name,
                    emp.job_position_id,
                    emp.job_level_id,
                    emp.job_level,
                    emp.employment_status_id,
                    emp.employment_status,
                    emp.branch_id,
                    emp.branch,
                    emp.join_date,
                    emp.length_of_service,
                    emp.grade,
                    emp.approval_line,
                    emp.approval_line_employee_id,
                    emp.status,
                    emp.sign_date,
                    emp.resign_date,
                    emp.date_of_issue,
                    emp.country_of_issue,
                    emp.@class,
                    emp.employee_id,
                }, transaction);
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdatePersonal(Models.Personal emp, IDbTransaction transaction)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

            try
            {
                string queryUpdate = $"update {table_personal} set {field_update_personal} where employee_id = @employee_id";
                await _mysqlConn.ExecuteAsync(queryUpdate, new
                {
                    emp.first_name,
                    emp.last_name,
                    emp.barcode,
                    emp.email,
                    emp.identity_type,
                    emp.identity_number,
                    emp.expired_date_identity_id,
                    emp.postal_code,
                    emp.address,
                    emp.current_address,
                    emp.birth_place,
                    emp.birth_date,
                    emp.phone,
                    emp.mobile_phone,
                    emp.gender,
                    emp.marital_status,
                    emp.blood_type,
                    emp.religion,
                    emp.avatar,
                    emp.employee_id
                }, transaction);
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateSbu(List<Models.Sbu> sbuList, IDbTransaction transaction)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

            try
            {
                string updateQuery = $@"UPDATE {table_sbu} SET {field_update_sbu} WHERE employee_id = @employee_id AND field_id = @field_id";
                string insertQuery = $"insert into {table_sbu} ({field_sbu}) values ({field_insert_sbu})";

                foreach (var lsbu in sbuList)
                {
                    var checkSbuExists = await CheckSbuData(lsbu.employee_id, lsbu.field_id);
                    
                    if (!checkSbuExists)
                    {
                        await _mysqlConn.ExecuteAsync(insertQuery, new
                        {
                            lsbu.field_name,
                            lsbu.value_id,
                            lsbu.value_name,
                            lsbu.value_code,
                            lsbu.field_id,
                            lsbu.employee_id
                        }, transaction);
                    }
                    else
                    {
                        await _mysqlConn.ExecuteAsync(updateQuery, new
                        {
                            lsbu.field_name,
                            lsbu.value_id,
                            lsbu.value_name,
                            lsbu.value_code,
                            lsbu.field_id,
                            lsbu.employee_id
                        }, transaction);
                    }                        
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateFamilyMember(Models.Family f, IDbTransaction transaction)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

            try
            {
                string query = $"update {table_family_member} set {field_update_family_member} where employee_id=@employee_id";
                
                foreach (var lMembers in f.members)
                {
                    await _mysqlConn.ExecuteAsync(query, new
                    {
                        full_name = lMembers.full_name,
                        relationship = lMembers.relationship,
                        relationship_id = lMembers.relationship_id,
                        birth_date = lMembers.birth_date,
                        marital_status = lMembers.marital_status,
                        gender = lMembers.gender,
                        job = lMembers.job,
                        religion = lMembers.religion,
                        is_deleted = lMembers.is_deleted,
                        created_at = lMembers.created_at,
                        updated_at = lMembers.updated_at,
                        no_ktp = lMembers.no_ktp,
                        employee_id = lMembers.employee_id,
                    }, transaction);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateEmergencyContact(Models.Family f, IDbTransaction transaction)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

            try
            {
                string query = $"insert into {table_emergency_contact} set {field_update_emergency_contact} where employee_id = @employee_id";

                foreach (var lEmContact in f.emergency_contacts)
                {
                    await _mysqlConn.ExecuteAsync(query, new
                    {
                        full_name = lEmContact.full_name,
                        relationship = lEmContact.relationship,
                        birth_date = lEmContact.birth_date,
                        no_ktp = lEmContact.no_ktp,
                        marital_status = lEmContact.marital_status,
                        gender = lEmContact.gender,
                        job = lEmContact.job,
                        religion = lEmContact.religion,
                        employee_id = lEmContact.employee_id
                    }, transaction);
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdatePayrollInfo(Models.PayrollInfo pi, IDbTransaction transaction)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

            try
            {
                string query = $"update {table_payroll_info} set {field_update_payroll_info} where employee_id = @employee_id";

                await _mysqlConn.ExecuteAsync(query, new
                {
                    bpjs_ketenagakerjaan = pi.bpjs_ketenagakerjaan,
                    bpjs_kesehatan = pi.bpjs_kesehatan,
                    npwp = pi.npwp,
                    bank_id = pi.bank_id,
                    bank_name = pi.bank_name,
                    bank_account = pi.bank_account,
                    bank_account_holder = pi.bank_account_holder,
                    ptkp_status = pi.ptkp_status,
                    bank_code = pi.bank_code,
                    cost_center_id = pi.cost_center_id,
                    cost_center_name = pi.cost_center_name,
                    cost_center_category_id = pi.cost_center_category_id,
                    cost_center_category_name = pi.cost_center_category_name,
                    employee_tax_status = pi.employee_tax_status,
                    nationality_code = pi.nationality_code,
                    expatriatedn_date = pi.expatriatedn_date,
                    created_at = pi.created_at,
                    updated_at = pi.updated_at,
                    employee_id = pi.employee_id
                }, transaction);
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateCustomField(List<Models.CustomField> cf, IDbTransaction transaction)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

            try
            {
                string query = $@"UPDATE {table_custom_field} SET value = @value WHERE employee_id = @employee_id AND field_name = @field_name";

                foreach (var lcf in cf)
                {
                    await _mysqlConn.ExecuteAsync(query, new
                    {
                        field_name = lcf.field_name,
                        value = lcf.value,
                        employee_id = lcf.employee_id
                    }, transaction);
                }
            }
            catch
            {
                throw;
            }
        }

        #endregion

        public async Task<bool> UpdateEmployeeResign(string empId, string status, string resignDate)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            var trx = _mysqlConn.BeginTransaction();

            try
            {
                string query = $"update employment set status=@status, sign_date=@signDate where employee_id=@empId";

                await _mysqlConn.ExecuteAsync(query, new
                {
                    status = status,
                    resign_date = resignDate,
                    employee_id = empId
                }, trx);

                trx.Commit();

                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                throw;
            }
        }
    }
}
