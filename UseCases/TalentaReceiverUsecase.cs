using AutoMapper;
using Common.Logging;
using FluentValidation;
using Grpc.Core;
using jpk3service.Repositories;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Mozilla;
using Org.BouncyCastle.Ocsp;
using TalentaReceiver.Models;
using TalentaReceiver.Protos;
using TalentaReceiver.Repositories;
using TalentaReceiver.Repositories.MsSql;
using TalentaReceiver.Utils;

namespace TalentaReceiver.UseCases
{
    public interface ITalentaReceiverUsecase
    {
        Task<Protos.ResponseMessage> PostEmployee(Protos.Root request, ServerCallContext ctx);
        Task<Protos.ResponseMessage> UpdateEmployee(Protos.Root request, ServerCallContext ctx);
        Task<Protos.ResponseMessage> EmployeeResign(Protos.ResignDataRequest resignData, ServerCallContext ctx);
        Task<Protos.ResponseMessage> EmployeeTransfer(Protos.EmpTransferRoot request, ServerCallContext ctx);
    }

    public class TalentaReceiverUsecase : ITalentaReceiverUsecase
    {
        private readonly ILogger<TalentaReceiverUsecase> _log;
        private readonly IMTEmployeeRepositories _repo;
        private readonly IMTPesertaTempRepository _repoMtPeserta;
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly IValidator<MT_Peserta_Temp> _validator;
        private readonly IMTBBGJobLevelRepository _jobLevelRepository;
        private readonly IMTBBGCompanyRepository _companyRepository;
        private readonly IHelperService _helper;
        private readonly IMTJobLogSAP _jobLogSAP;
        private readonly IUrlSettingProvider _configUrl;

        public TalentaReceiverUsecase(ILogger<TalentaReceiverUsecase> log, IMTEmployeeRepositories repo, HttpClient httpClient, IMapper mapper, IValidator<MT_Peserta_Temp> validator, IMTBBGJobLevelRepository jobLevelRepository, IMTBBGCompanyRepository companyRepository, IMTPesertaTempRepository repoMtPeserta, IHelperService helper, IMTJobLogSAP jobLogSAP, IUrlSettingProvider configUrl)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _repo = repo;
            _httpClient = httpClient;
            _mapper = mapper;
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _jobLevelRepository = jobLevelRepository ?? throw new ArgumentNullException(nameof(jobLevelRepository));
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            _repoMtPeserta = repoMtPeserta ?? throw new ArgumentNullException(nameof(repoMtPeserta));
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _jobLogSAP = jobLogSAP ?? throw new ArgumentNullException(nameof(jobLogSAP));
            _configUrl = configUrl ?? throw new ArgumentNullException(nameof(configUrl));
        }


        public static Models.Employee EmploymentMaps(Protos.Employee req)
        {
            #region Employment
            var empProtos = req.Employment;
            var sbuData = req.Employment.Sbu;
            var listSbu = new List<Models.Sbu>();

            if (sbuData != null)
            {
                foreach (var lsbu in sbuData)
                {
                    Models.Sbu modSbu = new()
                    {
                        field_id = (int)lsbu.FieldId!,
                        field_name = lsbu.FieldName!,
                        value_id = (int)lsbu.ValueId!,
                        value_name = lsbu.ValueName!,
                        value_code = lsbu.ValueCode!,
                        employee_id = empProtos.EmployeeId!,
                    };
                    listSbu.Add(modSbu);
                }
            }
            else
            {
                listSbu = [];
            }

            var mapEmployment = new Models.Employment()
            {
                employee_id                 = empProtos.EmployeeId,
                company_id                  = (int)(empProtos.CompanyId! != 0 ? empProtos.CompanyId : 0)!,
                organization_id             = (int)(empProtos.OrganizationId! != 0 ? empProtos.OrganizationId : 0)!,
                organization_name           = empProtos.OrganizationName != null ? empProtos.OrganizationName : "",
                job_position_id             = (int)(empProtos.JobPositionId! != 0 ? empProtos.JobPositionId : 0)!,
                job_position                = empProtos.JobPosition != null ? empProtos.JobPosition : "",
                job_level_id                = (int)(empProtos.JobLevelId! != 0 ? empProtos.JobLevelId : 0)!,
                job_level                   = empProtos.JobLevel != null ? empProtos.JobLevel : "",
                employment_status_id        = (int)(empProtos.EmploymentStatusId! != 0 ? empProtos.EmploymentStatusId : 0)!,
                employment_status           = empProtos.EmploymentStatus != null ? empProtos.EmploymentStatus : "",
                branch_id                   = (int)(empProtos.BranchId! != 0 ? empProtos.BranchId : 0)!,
                branch                      = empProtos.Branch != null ? empProtos.Branch : "",
                join_date                   = empProtos.JoinDate != null ? empProtos.JoinDate : "",
                length_of_service           = empProtos.LengthOfService != null ? empProtos.LengthOfService : "",
                grade                       = empProtos.Grade != null ? empProtos.Grade : "",
                approval_line               = (int)(empProtos.ApprovalLine! != 0 ? empProtos.ApprovalLine : 0)!,
                approval_line_employee_id   = empProtos.ApprovalLineEmployeeId != null ? empProtos.ApprovalLineEmployeeId : "",
                status                      = empProtos.Status != null ? empProtos.Status : "",
                sign_date                   = empProtos.SignDate != null ? empProtos.SignDate : "",
                resign_date                 = empProtos.ResignDate != null ? empProtos.ResignDate : "",
                date_of_issue               = empProtos.DateOfIssue != null ? empProtos.DateOfIssue : "",
                country_of_issue            = empProtos.CountryOfIssue != null ? empProtos.DateOfIssue : "",
                @class                      = empProtos.Class != null ? empProtos.Class : "",
            };
            #endregion

            #region Personal
            var personalProtos = req.Personal;
            var mapPersonal = new Models.Personal()
            {
                first_name                  = personalProtos.FirstName,
                last_name                   = personalProtos.LastName,
                barcode                     = personalProtos.Barcode,
                email                       = personalProtos.Email,
                identity_type               = personalProtos.IdentityType,
                identity_number             = personalProtos.IdentityNumber,
                expired_date_identity_id    = personalProtos.ExpiredDateIdentityId,
                postal_code                 = personalProtos.PostalCode,
                address                     = personalProtos.Address,
                current_address             = personalProtos.CurrentAddress,
                birth_place                 = personalProtos.BirthPlace,
                birth_date                  = personalProtos.BirthDate,
                phone                       = personalProtos.Phone,
                mobile_phone                = personalProtos.MobilePhone,
                gender                      = personalProtos.Gender,
                marital_status              = personalProtos.MaritalStatus,
                blood_type                  = personalProtos.BloodType,
                religion                    = personalProtos.Religion,
                avatar                      = personalProtos.Avatar,
                employee_id                 = mapEmployment.employee_id
            };
            #endregion

            #region Family [Members & Emergency Contact]
            var famMemProtos    = req.Family.Members;
            var famECProtos     = req.Family.EmergencyContact;
            var lMember         = new List<Models.Member>();
            var lEmgContact     = new List<Models.EmergencyContact>();

            foreach (var member in famMemProtos)
            {
                Models.Member memDt = new()
                {
                    full_name = member.FullName,
                    relationship = member.Relationship,
                    relationship_id = (int)member.RelationshipId!,
                    birth_date = member.BirthDate,
                    no_ktp = member.NoKtp,
                    marital_status = member.MaritalStatus,
                    gender = member.Gender,
                    job = member.Job,
                    religion = member.Religion,
                    is_deleted = (int)member.IsDeleted!,
                    created_at = member.CreatedAt,
                    updated_at = member.UpdatedAt,
                    employee_id = mapEmployment.employee_id,
                };
                lMember.Add(memDt);
            }

            foreach (var member in famECProtos)
            {
                Models.EmergencyContact emgContactDt = new()
                {
                    full_name = member.FullName,
                    relationship = member.Relationship,
                    birth_date = member.BirthDate,
                    no_ktp = member.NoKtp,
                    marital_status = member.MaritalStatus,
                    gender = member.Gender,
                    job = member.Job,
                    religion = member.Religion,
                    employee_id = mapEmployment.employee_id
                };
                lEmgContact.Add(emgContactDt);
            }
            #endregion

            #region Payroll Info
            var reqPayInfo = req.PayrollInfo;
            var mapPayrollInfo = new Models.PayrollInfo()
            {
                bpjs_ketenagakerjaan = reqPayInfo.BpjsKetenagakerjaan,
                bpjs_kesehatan = reqPayInfo.BpjsKesehatan,
                npwp = reqPayInfo.Npwp,
                bank_id = (int)(reqPayInfo.BankId ?? 0),
                bank_name = reqPayInfo.BankName,
                bank_account = reqPayInfo.BankAccount,
                bank_account_holder = reqPayInfo.BankAccountHolder,
                ptkp_status = reqPayInfo.PtkpStatus,
                bank_code = reqPayInfo.BankCode,
                cost_center_id = (int)(reqPayInfo.CostCenterId ?? 0),
                cost_center_name = reqPayInfo.CostCenterName,
                cost_center_category_id = (int)(reqPayInfo.CostCenterCategoryId ?? 0),
                cost_center_category_name = reqPayInfo.CostCenterCategoryName,
                employee_tax_status = (int)(reqPayInfo.EmployeeTaxStatus ?? 0),
                nationality_code = reqPayInfo.NationalityCode,
                expatriatedn_date = reqPayInfo.ExpatriatednDate,
                created_at = reqPayInfo.CreatedAt,
                updated_at = reqPayInfo.UpdatedAt,
                employee_id = mapEmployment.employee_id,
            };
            #endregion

            #region Custom Field
            var reqCustomField = req.CustomField;
            List<Models.CustomField> customField = new List<Models.CustomField>();
            foreach (var field in reqCustomField)
            {
                customField.Add(new Models.CustomField()
                {
                    field_name = field.FieldName,
                    value = field.Value,
                    employee_id = mapEmployment.employee_id
                });
            };
            #endregion

            return new Models.Employee
            {
                employment = mapEmployment,
                sbu = listSbu,
                personal = mapPersonal,
                family = new Models.Family()
                {
                    members = lMember,
                    emergency_contacts = lEmgContact
                },
                payroll_info = mapPayrollInfo,
                custom_field = customField
            };
        }

        public async Task<Protos.ResponseMessage> MapsEmployee(Protos.Root emp, ServerCallContext ctx, string flag)
        {
            var mapsEmployeeResult = new List<Models.Employee>();

            foreach (var req in emp.Data.Employees)
            {
                string nip = req.Employee.Employment.EmployeeId;
               
                var mapEmployeesResult = EmploymentMaps(req.Employee);

                if (flag == "create")
                {
                    await _repo.db().AddEmployees(mapEmployeesResult);

                    SDLogging.Log($"Successfully {flag} employee, endpoint: /v1/talenta-receiver/employee, data = {emp}", "INFO", SDLogging.INFO);
                }

                if (flag == "update")
                {
                    var empId = req.Employee.Employment.EmployeeId;
                    var checkEmpId = await _repo.db().CheckEmploymentId(empId);
                    if (checkEmpId.IsNullOrEmpty())
                    {
                        SDLogging.Log($"Employee Id: {empId} is not found. Skipping this record.", "WARN", SDLogging.WARNING);
                        continue;
                    }

                    await _repo.db().UpdateEmployees(mapEmployeesResult);

                    SDLogging.Log($"Successfully {flag} employee, endpoint: /v1/talenta-receiver/update-employee, data = {emp}", "INFO", SDLogging.INFO);
                }
            }

            return new ResponseMessage
            {
                Code = 200,
                Message = "OK"
            };
        }

        public async Task<Protos.ResponseMessage> PostEmployee(Protos.Root request, ServerCallContext ctx)
        {
            try
            {
                #region Whitelist Nip Validation
                var lEmployeeId = request.Data.Employees.Select(o => o.Employee.Employment.EmployeeId).Distinct().ToList();
                var existingWhitelistNips = await _jobLogSAP.CheckExistingWhitelistNips(lEmployeeId);
                if (existingWhitelistNips.Count() == 0)
                {
                    _log.LogError($"400 Bad Request: Employee Id not found on the whitelist: {string.Join(", ", lEmployeeId)}, endpoints: /v1/talenta-receiver/employee");
                    return new ResponseMessage
                    {
                        Code = 400,
                        Message = $"The following NIPs are not found on the whitelist: {string.Join(", ", lEmployeeId)}"
                    };
                }
                #endregion

                // Post to db talenta receiver
                await MapsEmployee(request, ctx, "create");

                // Post to MT_Peserta_Temp
                await PostEmployeeMtPeserta(request, ctx);

                //foreach (var employeeData in request.Data.Employees)
                //{
                //    await MapEmployeeHiring(employeeData.Employee);
                //    await MapPersonalData(employeeData.Employee);
                //}

                return new ResponseMessage
                {
                    Code = 200,
                    Message = "OK"
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Protos.ResponseMessage> UpdateEmployee(Protos.Root request, ServerCallContext ctx)
        {
            //SDLogging.Log($"log update employee, endpoint: /v1/talenta-receiver/update-employee, data = {request}", "INFO", SDLogging.INFO);
            if (request == null)
            {
                SDLogging.Log($"Bad request: request payload is null, endpoint: /v1/talenta-receiver/update-employee, data = {request}", "ERROR", SDLogging.ERROR);

                return new ResponseMessage
                {
                    Code = 400,
                    Message = $"bad request, request payload is null"
                };
            }

            try
            {
                #region Whitelist Nip Validation
                var lEmployeeId = request.Data.Employees.Select(o => o.Employee.Employment.EmployeeId).Distinct().ToList();
                var existingWhitelistNips = await _jobLogSAP.CheckExistingWhitelistNips(lEmployeeId);
                if (existingWhitelistNips.Count() == 0)
                {
                    _log.LogError($"400 Bad Request: Employee Id not found on the whitelist: {string.Join(", ", lEmployeeId)}, endpoints: /v1/talenta-receiver/update-employee");
                    return new ResponseMessage
                    {
                        Code = 400,
                        Message = $"The following NIPs are not found on the whitelist: {string.Join(", ", lEmployeeId)}"
                    };
                }
                #endregion

                // Update to db talenta receiver
                await MapsEmployee(request, ctx, "update");

                // Post To MT_Peserta_Temp
                await PostEmployeeMtPeserta(request, ctx);

                //foreach(var employeeData in request.Data.Employees)
                //{
                //    await MapPersonalData(employeeData.Employee);
                //}

                return new ResponseMessage
                {
                    Code = 200,
                    Message = "OK"
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<Protos.ResponseMessage> EmployeeTransfer(Protos.EmpTransferRoot request, ServerCallContext ctx)
        {
            SDLogging.Log($"log employee transfer, endpoint: /v1/talenta-receiver/employee-transfer, data = {request}", "INFO", SDLogging.INFO);
            if (request == null)
            {
                SDLogging.Log($"Bad request: request payload is null, endpoint: /v1/talenta-receiver/employee-transfer, data = {request}", "ERROR", SDLogging.ERROR);

                return new ResponseMessage
                {
                    Code = 400,
                    Message = $"bad request"
                };
            }

            try
            {
                #region Whitelist Nip Validation
                var lEmployeeId = request.EmpTransferData.EmpTransferEmployees.Select(o => o.NewEmployment.Employment.EmployeeId).Distinct().ToList();
                var existingWhitelistNips = await _jobLogSAP.CheckExistingWhitelistNips(lEmployeeId);
                if (existingWhitelistNips.Count() == 0)
                {
                    _log.LogError($"400 Bad Request: Employee Id not found on the whitelist: {string.Join(", ", lEmployeeId)}, endpoints: /v1/talenta-receiver/employee-transfer");
                    return new ResponseMessage
                    {
                        Code = 400,
                        Message = $"The following NIPs are not found on the whitelist: {string.Join(", ", lEmployeeId)}"
                    };
                }
                #endregion

                foreach (var lEmpTf in request.EmpTransferData.EmpTransferEmployees)
                {
                    var empId = lEmpTf.NewEmployment.Employment.EmployeeId;
                    var checkEmpId = await _repo.db().CheckEmploymentId(empId);
                    if (checkEmpId.IsNullOrEmpty())
                    {
                        var mapEmpData = new Models.Employee();
                        var employeeTransferId = "";
                        foreach(var lEmpTrf in request.EmpTransferData.EmpTransferEmployees)
                        {
                            employeeTransferId = lEmpTrf.NewEmployment.Employment.EmployeeId;
                            mapEmpData =  EmploymentMaps(lEmpTf.NewEmployment);
                        }

                        await _repo.db().AddEmployees(mapEmpData);
                        SDLogging.Log($"Successfully create employee to talenta-receiver, Employee NIP = {employeeTransferId}", "INFO", SDLogging.INFO);
                    }

                    var mapEmpRes = EmploymentMaps(lEmpTf.NewEmployment);

                    await _repo.db().UpdateEmployees(mapEmpRes);

                    SDLogging.Log($"Successfully post Employee Transfer, data = {request}", "INFO", SDLogging.INFO);

                    //await MapPersonalData(lEmpTf.NewEmployment);
                }

                return new ResponseMessage
                {
                    Code = 200,
                    Message = "OK"
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<Protos.ResponseMessage> PostEmployeeMtPeserta(Protos.Root request, ServerCallContext ctx)
        {
            try
            {
                List<Models.MT_Peserta_Temp> lPesertaTemp = new List<Models.MT_Peserta_Temp>();

                var empData = request.Data.Employees;

                foreach (var item in empData)
                {
                    var resKodeBagian = "";
                    var costCenterName = item.Employee.PayrollInfo.CostCenterName;
                    if (!costCenterName.IsNullOrEmpty())
                    {
                        var countCCI = costCenterName.ToString();
                        resKodeBagian = countCCI?.Substring(countCCI.Length - 2, 2);
                    }
                    else
                    {
                        resKodeBagian = null;
                    }

                    var jobLevel = await _jobLevelRepository.db().GetByDescription(item.Employee.Employment.JobLevel);
                    var company = await _companyRepository.db().GetById(item.Employee.Employment.BranchId);

                    var statEmp = "";
                    if (item.Employee.Employment.SignDate == null | item.Employee.Employment.SignDate == "")
                    {
                        statEmp = "B";
                    }
                    else
                    {
                        statEmp = "A";
                    }

                    var signDate = item.Employee.Employment.SignDate!.Replace("-", "");

                    List<Models.Sbu> lModelSbu = new List<Models.Sbu>();
                    var lSbu = item.Employee.Employment.Sbu;
                    foreach (var sbu in lSbu)
                    {
                        lModelSbu.Add(new Models.Sbu()
                        {
                            field_id = (int)sbu.FieldId!,
                            field_name = sbu.FieldName,
                            value_id = sbu.ValueId,
                            value_name = sbu.ValueName,
                            value_code = sbu.ValueCode,
                            employee_id = item.Employee.Employment.EmployeeId
                        });
                    }

                    var poolCode = lModelSbu.Where(x => x.employee_id == item.Employee.Employment.EmployeeId && x.field_name == "Pool").FirstOrDefault()?.value_code;

                    var pesertaTemp = new Models.MT_Peserta_Temp
                    {
                        KodePool = poolCode ?? "",
                        KodeBagian = resKodeBagian!,
                        KodeJabatan = jobLevel != null ? jobLevel.Code : "",
                        KodePrsh = company != null ? company.IdBluebird : "",
                        NIP = item.Employee.Employment.EmployeeId,
                        NamaPeserta = item.Employee.Personal.FirstName + " " + item.Employee.Personal.LastName,
                        FlagPegawai = "N",
                        StatusKaryawan = statEmp, // jika sign_date not null maka "A" else maka "B"
                        StatusPeserta = item.Employee.Employment.Status == "Active" ? "A" : "T",
                        JenisKelamin = item.Employee.Personal.Gender == "Male" ? "L" : "P",
                        StatusPerkawinan = item.Employee.Personal.MaritalStatus == "Married" ? "M" : "S",
                        TanggalMasuk = item.Employee.Employment.JoinDate.Replace("-", ""),
                        TanggalAngkat = signDate.IsNullOrEmpty() ? null : signDate,
                        TanggalLahir = item.Employee.Personal.BirthDate.Replace("-", "")
                    };

                    var result = await _validator.ValidateAsync(pesertaTemp);
                    if (result.IsValid)
                    {
                        lPesertaTemp.Add(pesertaTemp);
                    }
                    else
                    {
                        if (result.Errors.Count() > 1)
                        {
                            foreach (var error in result.Errors)
                            {
                                pesertaTemp.NIP = error.PropertyName == "NIP" ? "" : pesertaTemp.NIP;
                                pesertaTemp.KodePool = error.PropertyName == "KodePool" ? "" : pesertaTemp.KodePool;
                                pesertaTemp.KodePrsh = error.PropertyName == "KodePrsh" ? "" : pesertaTemp.KodePrsh;
                                pesertaTemp.KodeJabatan = error.PropertyName == "KodeJabatan" ? "" : pesertaTemp.KodeJabatan;
                            }
                            pesertaTemp.ErrorMsg = result.ToString();
                        }
                        else
                        {
                            pesertaTemp.NIP = result.Errors[0].PropertyName == "NIP" ? "" : pesertaTemp.NIP;
                            pesertaTemp.KodePool = result.Errors[0].PropertyName == "KodePool" ? "" : pesertaTemp.KodePool;
                            pesertaTemp.KodePrsh = result.Errors[0].PropertyName == "KodePrsh" ? "" : pesertaTemp.KodePrsh;
                            pesertaTemp.KodeJabatan = result.Errors[0].PropertyName == "KodeJabatan" ? "" : pesertaTemp.KodeJabatan;
                            pesertaTemp.ErrorMsg = result.ToString();
                        }
                        lPesertaTemp.Add(pesertaTemp);
                    }
                };

                var ret = await _repoMtPeserta.db().Add(lPesertaTemp);

                SDLogging.Log($"Successfully insert data to MT_Peserta, data = {request}", "INFO", SDLogging.INFO);

                return new ResponseMessage { Message = "OK" };
            }
            catch (Exception ex)
            {
                SDLogging.Log($"Error post to MT_Peserta, {ex.Message}, UserId = {request.Data.Employees.First().Employee.Employment.EmployeeId} : {request.Data}", SDLogging.ERROR);
                throw;
            }
        }

        public async Task<Protos.ResponseMessage> EmployeeResign(Protos.ResignDataRequest resignData, ServerCallContext ctx)
        {
            try
            {
                List<ZihrPostPrsnDataRfc> lZihrPostPrsnDataRfc = new List<ZihrPostPrsnDataRfc>();
                var lZihrPostPrsnDataRfcDTO = new List<ZihrPostPrsnDataRfcDTO>();
                var getUrl = _configUrl.GetEndpoint("/SapHcm/ZihrPostPrsnDataRfc");

                if (resignData.Data == null || !resignData.Data.Employees.Any())
                {
                    return new ResponseMessage
                    {
                        Code = 400,
                        Message = "resign data cannot be null or empty"
                    };
                }

                foreach (var empListItem in resignData.Data.Employees)
                {
                    var empItem = empListItem.Employment;
                    var checkEmpId = await _repoMtPeserta.db().GetEmployeeId(empItem.EmployeeId);
                    if (!checkEmpId)
                    {
                        return new ResponseMessage
                        {
                            Code = 404,
                            Message = $"employee id: {empItem.EmployeeId} not found",
                        };
                    }

                    await _repoMtPeserta.db().EmpResign(empItem!.EmployeeId, "T", empItem.ResignDate);
                    await _repo.db().UpdateEmployeeResign(empItem.EmployeeId, empItem.Status, empItem.ResignDate);

                    #region Post to Personal Data SAP

                    var mtPesertaData = await _repoMtPeserta.db().GetMTPeserta(empItem.EmployeeId);
                    var empPrsnlData = await _repo.db().GetEmploymentTalenta(empItem.EmployeeId);
                    var EmpPersonalData = new ZihrPostPrsnDataRfc
                    {
                        pernr = empItem.EmployeeId,
                        begda = empPrsnlData.JoinDate.Replace("-", ""),
                        endda = !string.IsNullOrWhiteSpace(empItem.ResignDate) ? empItem.ResignDate.Replace("-", "") : "99991231",
                        cname = mtPesertaData.NamaPeserta,
                        anred = empPrsnlData.Address, // alamat
                        sprsl = "",
                        gbpas = mtPesertaData.TanggalLahir,
                        gbort = empPrsnlData.BirthPlace, // Tempat lahir
                        gblnd = "ID",
                        natio = empPrsnlData.CountryOfIssue, // Kewarganegaraan
                        famst = mtPesertaData.StatusPerkawinan == "M" ? "Menikah" : "Single",
                        famdt = "",
                        konfe = empPrsnlData.Religion, // Agama
                        gesch = mtPesertaData.JenisKelamin,
                    };

                    var respSap = await _helper.PostToSapApi(EmpPersonalData, getUrl, "Employee Personal Data");

                    lZihrPostPrsnDataRfcDTO.Add(new ZihrPostPrsnDataRfcDTO
                    {
                        zihrPostPrsnDataRfc = EmpPersonalData,
                        responseMetadata = new ResponseMetadata
                        {
                            created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            created_by = "talenta-receiver/employee-resign",
                            response_message = respSap?.Message!,
                            status = respSap?.Code == 200 ? "true" : "false"
                        }
                    });

                    #endregion
                }

                await _jobLogSAP.PostPersonalDataLog(lZihrPostPrsnDataRfcDTO);

                return new ResponseMessage
                {
                    Code = 200,
                    Message = "OK"
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<Protos.ResponseMessage> MapEmployeeHiring(Protos.Employee empItem)
        {
            try
            {
                var lPersonalData = new List<Models.ZihrPostInitHireRfc>();
                var lZihrPostInitHireRfcDTO = new List<ZihrPostInitHireRfcDTO>();
                ResponseMessage respSap = new ResponseMessage();
                var getUrl = _configUrl.GetEndpoint("/SapHcm/ZihrPostInitHireRfc");

                #region Mapping Sbu Item 

                var jobLevel                = await _jobLevelRepository.db().GetByDescription(empItem.Employment.JobLevel);
                var lSbu                    = MapperHelper.MapSbuFields(empItem.Employment.Sbu, empItem.Employment.EmployeeId);
                var actionTypeValue         = MapperHelper.ExtractValueCode(lSbu, "Action Type");
                var payrollAreaValue        = MapperHelper.ExtractValueName(lSbu, "Payr. Area");
                var personalSubAreaValue    = MapperHelper.ExtractValueCode(lSbu, "Personnel Subarea");
                var admTimeRecordValue      = MapperHelper.ExtractValueCode(lSbu, "Administrator for Time Recording");
                var workContractCode        = MapperHelper.ExtractValueCode(lSbu, "Work Contract");
                var reasonForActionValue    = MapperHelper.ExtractReasonForAction(lSbu);

                #endregion

                var EmpHiringResult = new ZihrPostInitHireRfc
                {
                    employeenumber = empItem.Employment.EmployeeId,
                    hiringdate = empItem.Employment.JoinDate.Replace("-", ""),
                    actiontype = actionTypeValue,
                    reasonforaction = reasonForActionValue,
                    werks = empItem.Employment.BranchCode ?? string.Empty,
                    btrtl = personalSubAreaValue,
                    persg = empItem.Employment.EmploymentStatus == "PKWT" ? "B" : "A",
                    persk = jobLevel != null ? jobLevel.Code : "",
                    abkrs = payrollAreaValue,
                    ansvh = workContractCode,
                    plans = empItem.Personal.Barcode.ToString()!,
                    stell = empItem.Employment.JobLevelId.ToString()!,
                    sachz = admTimeRecordValue
                };

                respSap = await _helper.PostToSapApi(EmpHiringResult, getUrl, "Employee Hiring");

                lZihrPostInitHireRfcDTO.Add(new ZihrPostInitHireRfcDTO
                {
                    zihrPostInitHireRfc = EmpHiringResult,
                    responseMetadata = new ResponseMetadata
                    {
                        created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        created_by = "talenta-receiver",
                        response_message = respSap?.Message!,
                        status = respSap?.Code == 200 ? "true" : "false"
                    }
                });

                await _jobLogSAP.PostEmployeeHiringLog(lZihrPostInitHireRfcDTO);

                return new ResponseMessage
                {
                    Code = 200,
                    Message = "OK"
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<Protos.ResponseMessage> MapPersonalData(Protos.Employee empItem)
        {
            try
            {
                var empHiring               = new List<Models.ZihrPostPrsnDataRfc>();
                var lZihrPostPrsnDataRfcDTO = new List<ZihrPostPrsnDataRfcDTO>();
                ResponseMessage respSap     = new ResponseMessage();
                var getUrl                  = _configUrl.GetEndpoint("/SapHcm/ZihrPostPrsnDataRfc");

                var payrollInfoModel = MapperHelper.MapPayrollInfoFromProto(empItem.PayrollInfo);
                string natioValue    = MapperHelper.GetNationalityCode(payrollInfoModel);
                string anredValue    = MapperHelper.GetSalutation(empItem.Personal.Gender, empItem.Personal.MaritalStatus);
                string famstValue    = MapperHelper.GetMaritalStatusCode(empItem.Personal.MaritalStatus);
                string konfeValue    = MapperHelper.GetReligionCode(empItem.Personal.Religion);

                var EmpPersonalData = new ZihrPostPrsnDataRfc
                {
                    pernr = empItem.Employment.EmployeeId,
                    begda = empItem.Employment.JoinDate.Replace("-", ""),
                    endda = !string.IsNullOrWhiteSpace(empItem.Employment.ResignDate) ? empItem.Employment.ResignDate.Replace("-", "") : "99991231",
                    cname = empItem.Personal.FirstName + empItem.Personal.LastName,
                    anred = anredValue,
                    sprsl = "ID",
                    gbpas = empItem.Personal.BirthDate.Replace("-", ""),
                    gbort = empItem.Personal.BirthPlace,
                    gblnd = natioValue,
                    natio = natioValue,
                    famst = famstValue,
                    famdt = "00000000",
                    konfe = konfeValue,
                    gesch = empItem.Personal.Gender == "Male" ? "1" : "2",
                };

                respSap = await _helper.PostToSapApi(EmpPersonalData, getUrl, "Employee Personal Data");

                lZihrPostPrsnDataRfcDTO.Add(new ZihrPostPrsnDataRfcDTO
                {
                    zihrPostPrsnDataRfc = EmpPersonalData,
                    responseMetadata = new ResponseMetadata
                    {
                        created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        created_by = "talenta-receiver/webhook-employee",
                        response_message = respSap?.Message!,
                        status = respSap?.Code == 200 ? "true" : "false"
                    }
                });

                await _jobLogSAP.PostPersonalDataLog(lZihrPostPrsnDataRfcDTO);

                return new ResponseMessage
                {
                    Code = 200,
                    Message = "OK"
                };
            }
            catch
            {
                throw;
            }
        }

    }
}
