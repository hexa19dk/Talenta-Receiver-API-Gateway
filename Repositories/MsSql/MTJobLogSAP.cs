using Dapper;
using MySql.Data.MySqlClient;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using TalentaReceiver.Config;
using TalentaReceiver.Models;

namespace TalentaReceiver.Repositories.MsSql
{
    public interface IMTJobLogSAP
    {
        Task<IEnumerable<Models.HiringDTO>> GetEmployeeHiringData(string employeeId, string createdDate);
        Task<IEnumerable<Models.PersonalDataDTO>> GetPersonalDataDTO(string employeeId, string createdDate);
        Task<List<string>> CheckExistingWhitelistNips(List<string> lnip);
        Task<List<SapDws>> GetDwsLogs(string startDate, string endDate);
        Task<List<SapPwsRfc>> GetPwsRfcs(string startDate, string endDate);
        Task<string> CheckNipOnOvertime(string nip, string begda);
        Task<bool> PostZiHrPostAbsenRfcLog(List<Models.ZiHrPostAbsenRfcLogDTO> models);
        Task<bool> PostOvertimeLog(List<Models.OvertimeSAPLogDTO> models);
        Task<bool> PostPwsLog(List<Models.SapPwsRfcDTO> pws);
        Task<bool> PostDwsLog(List<Models.SapDwsDTO> dws);
        Task<bool> PostEmployeeHiringLog(List<Models.ZihrPostInitHireRfcDTO> hires);
        Task<bool> PostPersonalDataLog(List<ZihrPostPrsnDataRfcDTO> personalDataList);
        Task<bool> PostWhitelistNipEmployee(List<string> lnip);        
    }

    public class MTJobLogSAP : IMTJobLogSAP
    {
        #region SQL Command

        #region ZiHrPostAbsenRfc
        string ZiHrPostAbsenRfc_table = "zihrpostabsenrfc_log";

        string field_ZiHrPostAbsenRfc = "pernr, begda, endda, subty, created_at, created_by, response_message, status";

        string field_insert_ZiHrPostAbsenRfc = "@pernr, @begda, @endda, @subty, @created_at, @created_by, @response_message, @status";
        #endregion

        #region OvertimeSAP
        string overtimesap_table = "overtimesap_log";
        string field_overtimesap = "pernr, begda, endda, ktart, beguz, enduz, created_at, created_by, response_message, status";
        string field_insert_overtimesap = "@pernr, @begda, @endda, @ktart, @beguz, @enduz, @created_at, @created_by, @response_message, @status";
        #endregion

        #region SapPwsRfc
        string sappwsrfc_table = "sappwsrfc_log";
        string field_sappwsrfc = $"pernr, begda, endda, schkz, zterf, created_at, created_by, status, response_message";
        string field_insert_sappwsrfc = $"@pernr, @begda, @endda, @schkz, @zterf, @created_at, @created_by, @status, @response_message";
        #endregion

        #region SapDws
        string sapdws_log = "sapdws_log";
        string field_sapdws_log = $"pernr, begda, endda, beguz, enduz, created_at, created_by, status, response_message";
        string field_insert_sapdws_log = $"@pernr, @begda, @endda, @beguz, @enduz, @created_at, @created_by, @status, @response_message";
        #endregion

        #region Employee Hiring 
        string hiring_table = "zihrpostinithirerfc_log";
        string field_hiring = "employeenumber, hiringdate, actiontype, reasonforaction, werks, btrtl, persg, persk, abkrs, ansvh, plans, stell, sachz, created_at, created_by, response_message, status";
        string param_hiring = "@employeenumber, @hiringdate, @actiontype, @reasonforaction, @werks, @btrtl, @persg, @persk, @abkrs, @ansvh, @plans, @stell, @sachz, @created_at, @created_by, @response_message, @status";
        #endregion

        #region Personal Data 
        string personal_data_table = "zihrpostprsndatarfc_log";
        string field_personal_data = "pernr, begda, endda, cname, anred, sprsl, gbpas, gbort, gblnd, natio, famst, famdt, konfe, gesch, created_at, created_by, response_message, status";
        string param_personal_data = "@pernr, @begda, @endda, @cname, @anred, @sprsl, @gbpas, @gbort, @gblnd, @natio, @famst, @famdt, @konfe, @gesch, @created_at, @created_by, @response_message, @status";
        #endregion

        #endregion

        private readonly IDbConnectionFactory _connFactory;
        //private readonly MySqlConnection _mysqlConn;
        public MTJobLogSAP(IDbConnectionFactory connFactory)
        {
            _connFactory = connFactory ?? throw new ArgumentNullException(nameof(connFactory));            
        }

        public async Task<IEnumerable<Models.HiringDTO>> GetEmployeeHiringData(string employeeId, string createdDate)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

            try
            {

                string query = $@"SELECT {field_hiring}
                    FROM {hiring_table} 
                    WHERE(@EmployeeNumber = '' OR employeenumber LIKE CONCAT('%', @EmployeeNumber, '%'))
                    AND(@CreatedDate = '' OR created_at LIKE CONCAT('%', @CreatedDate, '%'))
                    AND(status = FALSE OR status IS NULL OR status = '')
                ";

                var parameters = new
                {
                    EmployeeNumber = employeeId ?? string.Empty,
                    CreatedDate = createdDate ?? string.Empty
                };

                var result = await _mysqlConn.QueryAsync<Models.HiringDTO>(query, parameters);
                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Models.PersonalDataDTO>> GetPersonalDataDTO(string employeeId, string createdDate)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

            try
            {
                string query = $@"SELECT {field_personal_data}
                    FROM {personal_data_table} 
                    WHERE(@Pernr = '' OR pernr LIKE CONCAT('%', @Pernr, '%'))
                    AND(@CreatedDate = '' OR created_at LIKE CONCAT('%', @CreatedDate, '%'))
                    AND(status = FALSE OR status IS NULL OR status = '')";

                var parameters = new
                {
                    Pernr = employeeId ?? string.Empty,
                    CreatedDate = createdDate ?? string.Empty
                };

                var result = await _mysqlConn.QueryAsync<Models.PersonalDataDTO>(query, parameters);
                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> CheckNipOnOvertime(string nip, string begda)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

            try
            {
                string query = $@"SELECT pernr FROM {overtimesap_table} WHERE pernr = @Pernr AND begda = @Begda";
                var parameters = new
                {
                    Pernr = nip ?? string.Empty,
                    Begda = begda ?? string.Empty
                };

                var result = await _mysqlConn.QueryFirstOrDefaultAsync<string>(query, parameters);
                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<string>> CheckExistingWhitelistNips(List<string> lnip)
        {
            var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            
            try
            {
                if (!lnip.Any())
                {
                    throw new ArgumentException("NIP value cannot be null or empty.");
                }
                string query = "select nip from whitelist_nip_employee where nip in @Nip";
                var nipResult = await _mysqlConn.QueryAsync<string>(query, new { Nip = lnip });
                var existingNips = nipResult.ToList();

                return existingNips;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> PostZiHrPostAbsenRfcLog(List<Models.ZiHrPostAbsenRfcLogDTO> models)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            if (_mysqlConn.State != System.Data.ConnectionState.Open)
            {
                await _mysqlConn.OpenAsync();
            }

            await using var trx = await _mysqlConn.BeginTransactionAsync();

            try
            {
                foreach (var z in models)
                {
                    var query = $"insert into {ZiHrPostAbsenRfc_table} ({field_ZiHrPostAbsenRfc}) values ({field_insert_ZiHrPostAbsenRfc})";
                    await _mysqlConn.ExecuteAsync(query, new
                    {
                        pernr = z.ziHrPostAbsenRfc.pernr,
                        begda = z.ziHrPostAbsenRfc.begda,
                        endda = z.ziHrPostAbsenRfc.endda,
                        subty = z.ziHrPostAbsenRfc.subty,
                        created_at = z.responseMetadata.created_at,
                        created_by = z.responseMetadata.created_by,
                        status = z.responseMetadata.status,
                        response_message = z.responseMetadata.response_message
                    }, trx);
                }
                await trx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> PostOvertimeLog(List<Models.OvertimeSAPLogDTO> models)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

            if (_mysqlConn.State != System.Data.ConnectionState.Open)
            {
                await _mysqlConn.OpenAsync();
            }

            using var trx = await _mysqlConn.BeginTransactionAsync();

            try
            {
                foreach (var o in models)
                {
                    var query = $"insert into {overtimesap_table} ({field_overtimesap}) values ({field_insert_overtimesap})";
                    var res = await _mysqlConn.ExecuteAsync(query, new
                    {
                        pernr = o.overtimeSAP.pernr,
                        begda = o.overtimeSAP.begda,
                        endda = o.overtimeSAP.endda,
                        ktart = o.overtimeSAP.ktart,
                        beguz = o.overtimeSAP.beguz,
                        enduz = o.overtimeSAP.enduz,
                        status = o.responseMetadata.status,
                        created_at = o.responseMetadata.created_at,
                        created_by = o.responseMetadata.created_by,
                        response_message = o.responseMetadata.response_message
                    }, trx);
                }
                await trx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> PostPwsLog(List<Models.SapPwsRfcDTO> pws)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            if (_mysqlConn.State != System.Data.ConnectionState.Open)
            {
                await _mysqlConn.OpenAsync();
            }

            await using var trx = await _mysqlConn.BeginTransactionAsync();

            try
            {
                foreach (var o in pws)
                {
                    var query = $"insert into {sappwsrfc_table} ({field_sappwsrfc}) values ({field_insert_sappwsrfc})";
                    await _mysqlConn.ExecuteAsync(query, new
                    {
                        pernr = o.sapPwsRfc.pernr,
                        begda = o.sapPwsRfc.begda,
                        endda = o.sapPwsRfc.endda,
                        schkz = o.sapPwsRfc.schkz,
                        zterf = o.sapPwsRfc.zterf,
                        status = o.responseMetadata.status,
                        created_at = o.responseMetadata.created_at,
                        created_by = o.responseMetadata.created_by,
                        response_message = o.responseMetadata.response_message
                    }, trx);
                }
                await trx.CommitAsync();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> PostDwsLog(List<Models.SapDwsDTO> dwsList)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            if (_mysqlConn.State != System.Data.ConnectionState.Open)
            {
                await _mysqlConn.OpenAsync();
            }

            using var trx = await _mysqlConn.BeginTransactionAsync();

            try
            {
                foreach (var o in dwsList)
                {
                    var query = $"INSERT INTO {sapdws_log} ({field_sapdws_log}) VALUES ({field_insert_sapdws_log})";
                    await _mysqlConn.ExecuteAsync(query, new
                    {
                        pernr = o.sapDws.pernr,
                        begda = o.sapDws.begda,
                        endda = o.sapDws.endda,
                        beguz = o.sapDws.beguz,
                        enduz = o.sapDws.enduz,
                        created_at = o.responseMetadata.created_at,
                        created_by = o.responseMetadata.created_by,
                        status = o.responseMetadata.status,
                        response_message = o.responseMetadata.response_message
                    }, trx);
                }
                await trx.CommitAsync();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> PostEmployeeHiringLog(List<Models.ZihrPostInitHireRfcDTO> hires)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            var trx = _mysqlConn.BeginTransaction();

            try
            {
                foreach (var h in hires)
                {
                    var query = $"INSERT INTO {hiring_table} ({field_hiring}) VALUES ({param_hiring})";

                    await _mysqlConn.ExecuteAsync(query, new
                    {
                        employeenumber = h.zihrPostInitHireRfc.employeenumber,
                        hiringdate = h.zihrPostInitHireRfc.hiringdate,
                        actiontype = h.zihrPostInitHireRfc.actiontype,
                        reasonforaction = h.zihrPostInitHireRfc.reasonforaction,
                        werks = h.zihrPostInitHireRfc.werks,
                        btrtl = h.zihrPostInitHireRfc.btrtl,
                        persg = h.zihrPostInitHireRfc.persg,
                        persk = h.zihrPostInitHireRfc.persk,
                        abkrs = h.zihrPostInitHireRfc.abkrs,
                        ansvh = h.zihrPostInitHireRfc.ansvh,
                        plans = h.zihrPostInitHireRfc.plans,
                        stell = h.zihrPostInitHireRfc.stell,
                        sachz = h.zihrPostInitHireRfc.sachz,
                        created_at = h.responseMetadata.created_at,
                        created_by = h.responseMetadata.created_by,
                        status = h.responseMetadata.status,
                        response_message = h.responseMetadata.response_message
                    }, trx);
                }

                trx.Commit();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> PostPersonalDataLog(List<ZihrPostPrsnDataRfcDTO> personalDataList)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            var trx = _mysqlConn.BeginTransaction();

            try
            {
                foreach (var o in personalDataList)
                {
                    var query = $"INSERT INTO {personal_data_table} ({field_personal_data}) VALUES ({param_personal_data})";

                    string famdtString = o.zihrPostPrsnDataRfc.famdt;
                    DateTime? famdtValue = null;

                    if (!string.IsNullOrWhiteSpace(famdtString) && famdtString != "00000000")
                    {
                        if (DateTime.TryParseExact(famdtString, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                        {
                            famdtValue = parsedDate;
                        }
                    }
                    await _mysqlConn.ExecuteAsync(query, new
                    {
                        pernr = o.zihrPostPrsnDataRfc.pernr,
                        begda = o.zihrPostPrsnDataRfc.begda,
                        endda = o.zihrPostPrsnDataRfc.endda,
                        cname = o.zihrPostPrsnDataRfc.cname,
                        anred = o.zihrPostPrsnDataRfc.anred,
                        sprsl = o.zihrPostPrsnDataRfc.sprsl,
                        gbpas = o.zihrPostPrsnDataRfc.gbpas,
                        gbort = o.zihrPostPrsnDataRfc.gbort,
                        gblnd = o.zihrPostPrsnDataRfc.gblnd,
                        natio = o.zihrPostPrsnDataRfc.natio,
                        famst = o.zihrPostPrsnDataRfc.famst,
                        famdt = famdtValue,
                        konfe = o.zihrPostPrsnDataRfc.konfe,
                        gesch = o.zihrPostPrsnDataRfc.gesch,
                        created_at = o.responseMetadata.created_at,
                        created_by = o.responseMetadata.created_by,
                        status = o.responseMetadata.status,
                        response_message = o.responseMetadata.response_message
                    }, trx);
                }

                trx.Commit();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> PostWhitelistNipEmployee(List<string> lnip)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            var trx = _mysqlConn.BeginTransaction();

            try
            {
                foreach (var nip in lnip)
                {
                    var query = $"INSERT INTO whitelist_nip_employee (nip) VALUES (@Nip)";
                    await _mysqlConn.ExecuteAsync(query, new
                    {
                        Nip = nip
                    }, trx);
                }

                trx.Commit();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                throw;
            }
        }

        public async Task<List<SapDws>> GetDwsLogs(string startDate, string endDate)
        {
            try
            {
                var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

                string query = $@"SELECT pernr, begda, endda, beguz, enduz FROM bbone_talenta.sapdws_log" +
                    " WHERE created_at BETWEEN @StartDate AND @EndDate AND status='false'";

                var result = await _mysqlConn.QueryAsync<SapDws>(query, new
                {
                    StartDate = startDate ?? string.Empty,
                    EndDate = endDate ?? string.Empty
                });

                return result.ToList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<SapPwsRfc>> GetPwsRfcs(string startDate, string endDate)
        {
            try
            {
                var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

                string query = $@"SELECT pernr, begda, endda, schkz, zterf FROM bbone_talenta.sappwsrfc_log" +
                    " WHERE created_at BETWEEN @StartDate AND @EndDate AND status='false'";

                var result = await _mysqlConn.QueryAsync<SapPwsRfc>(query, new
                {
                    StartDate = startDate ?? string.Empty,
                    EndDate = endDate ?? string.Empty
                });

                return result.ToList();
            }
            catch
            {
                throw;
            }
        }

    }
}
