using Dapper;
using Grpc.Core;
using MySql.Data.MySqlClient;
using System.Data;
using TalentaReceiver.Config;

namespace TalentaReceiver.Repositories.MsSql
{
    public interface IMTPwsDws
    {
        Task<bool> PostEventPwsDws(Models.DataEmpPwsDws request);
        Task<IEnumerable<Models.CustomField>> GetManagementStatusByEmpId(string employeeId);
        Task<Models.DailyWorkSchedule> GetStartEndDate(string employeeId);
        Task<string> GetEmployeeId();
        Task<Models.PwsDwsDetailDetail> GetStartEndTime(string employeeId, string dailyWorkScheduleCode);
        Task<bool> AddPws(List<Models.PeriodWorkSchedule> pws);
        Task<bool> AddDws(List<Models.DailyWorkSchedule> dws);
        Task<string> GetStartDateFromEmployment(string employeeId);
    }

    public class MTPwsDws : IMTPwsDws
    {
        #region SqlCommand

        #region Period Work Schedule
        string table_period_work_schedule = "period_work_schedules";

        string period_work_schedule_field = "period_work_schedule_code, work_days, daily_work_schedule_code, period_work_schedule_text, daily_work_schedule";

        string period_work_schedule_field_insert = "@period_work_schedule_code, @work_days, @daily_work_schedule_code, @period_work_schedule_text, @daily_work_schedule";
        #endregion

        #region Daily Work Schedule
        string table_daily_work_schedule = "daily_work_schedule";

        string daily_work_schedule_field = "daily_work_schedule_group, daily_work_schedule_code, end_date, start_date, daily_work_schedule_text, planed_working_hours, work_break_schedule_code, employee_id";

        string daily_work_schedule_field_insert = "@daily_work_schedule_group, @daily_work_schedule_code, @end_date, @start_date, @daily_work_schedule_text, @planed_working_hours, @work_break_schedule_code, @employee_id";
        #endregion

        #region Daily Work Schedule Detail 
        string table_daily_work_schedule_detail = "daily_work_schedule_detail";

        string field_daily_work_schedule_detail = "employee_id, start_date, month_hours, week_hours, daily_hours, week_days, work_schedule_rule,  work_break_schedule";

        string field_insert_daily_work_schedule_detail = "@employee_id, @start_date, @month_hours, @week_hours, @daily_hours, @week_days, @work_schedule_rule, @work_break_schedule";
        #endregion

        #region Daily Work Schedule Detail Detail
        string table_daily_work_schedule_detaildetail = "daily_work_schedule_detaildetail";

        string field_daily_work_schedule_detaildetail = "employee_id, daily_work_schedule_code, planned_working_hours, planned_working_time";

        string field_insert_daily_work_schedule_detaildetail = "@employee_id, @daily_work_schedule_code, @planned_working_hours, @planned_working_time";
        #endregion

        #endregion

        private readonly IDbConnectionFactory _connFactory;
        public MTPwsDws(IDbConnectionFactory connFactory)
        {
            _connFactory = connFactory ?? throw new ArgumentNullException(nameof(connFactory));
        }

        public async Task<bool> PostEventPwsDws(Models.DataEmpPwsDws request)
        {
            try
            {
                await using var conn = (MySqlConnection)await _connFactory.CreateConnectionAsync();                

                if (conn.State != System.Data.ConnectionState.Open)
                {
                    await conn.OpenAsync();
                }

                await using var transaction = await conn.BeginTransactionAsync();

                try
                {
                    await addDailyWorkScheduleDetail(request.EmployeesPwsDwsDetails, transaction, conn);
                    await addDailyWorkScheduleDetailDetail(request.PwsDwsDetailDetails, transaction, conn);

                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> addDailyWorkScheduleDetail(List<Models.EmployeesPwsDwsDetail2> request,  MySqlTransaction transaction, MySqlConnection conn)
        {
            try
            {
                foreach (var lemp in request)
                {
                    string query = $"INSERT INTO  {table_daily_work_schedule_detail}  ({field_daily_work_schedule_detail}) VALUES ({field_insert_daily_work_schedule_detail})";

                    await conn.ExecuteAsync(query, new
                    {
                        employee_id = lemp.employee_id,
                        start_date = lemp.start_date,
                        month_hours = lemp.month_hours,
                        week_hours = lemp.week_hours,
                        daily_hours = lemp.daily_hours,
                        week_days = lemp.week_days,
                        work_schedule_rule = lemp.work_schedule_rule,
                        work_break_schedule = lemp.work_break_schedule,
                    }, transaction);
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> addDailyWorkScheduleDetailDetail(List<Models.PwsDwsDetailDetail> request, MySqlTransaction transaction, MySqlConnection conn)
        {
            try
            {
                foreach (var item in request)
                {
                    string query = $"INSERT INTO {table_daily_work_schedule_detaildetail} ({field_daily_work_schedule_detaildetail}) VALUES ({field_insert_daily_work_schedule_detaildetail})";

                    await conn.ExecuteAsync(query, new
                    {
                        employee_id = item.employee_id,
                        daily_work_schedule_code = item.daily_work_schedule_code,
                        planned_working_hours = item.planned_working_hours,
                        planned_working_time = item.planned_working_time,

                    }, transaction);
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> AddPws(List<Models.PeriodWorkSchedule> pws)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            var transaction = _mysqlConn.BeginTransaction();

            try
            {
                foreach (var model in pws)
                {
                    string query = $"INSERT INTO {table_period_work_schedule} ({period_work_schedule_field}) VALUES ({period_work_schedule_field_insert})";

                    await _mysqlConn.ExecuteAsync(query, new
                    {
                        period_work_schedule_code = model.period_work_schedule_code,
                        work_days = model.work_days,
                        daily_work_schedule_code = model.daily_work_schedule_code,
                        period_work_schedule_text = model.period_work_schedule_text,
                        daily_work_schedule = string.Join(",", model.daily_work_schedule)
                    });
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> AddDws(List<Models.DailyWorkSchedule> dws)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();
            var transaction = _mysqlConn.BeginTransaction();

            try
            {
                foreach (var model in dws)
                {
                    string query = $"INSERT INTO {table_daily_work_schedule} ({daily_work_schedule_field}) VALUES ({daily_work_schedule_field_insert})";

                    await _mysqlConn.ExecuteAsync(query, new
                    {
                        daily_work_schedule_group = model.daily_work_schedule_group,
                        daily_work_schedule_code = model.daily_work_schedule_code,
                        end_date = model.end_date,
                        start_date = model.start_date,
                        daily_work_schedule_text = model.daily_work_schedule_text,
                        planed_working_hours = model.planed_working_hours,
                        work_break_schedule_code = model.work_break_schedule_code,
                        employee_id = model.employee_id
                    }, transaction);
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Models.CustomField>> GetManagementStatusByEmpId(string employeeId)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

            try
            {
                string query = $"SELECT field_name, value FROM custom_field WHERE field_name in ('time_management_status', 'work_schedule_rule') AND employee_id=@employeeId";

                var result = await _mysqlConn.QueryAsync<Models.CustomField>(query, new { employeeId });

                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> GetEmployeeId()
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

            try
            {
                string query = $"SELECT employee_id FROM employment order by id desc limit 1";
                var result = await _mysqlConn.QueryFirstOrDefaultAsync<string?>(query);

                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Models.DailyWorkSchedule> GetStartEndDate(string employeeId)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

            try
            {
                string query = $"SELECT * FROM daily_work_schedule WHERE employee_id = @employeeId";
                return await _mysqlConn.QueryFirstOrDefaultAsync<Models.DailyWorkSchedule>(query, new { employeeId });
            }
            catch
            {
                throw;
            }
        }

        public async Task<Models.PwsDwsDetailDetail> GetStartEndTime(string employeeId, string dailyWorkScheduleCode)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

            try
            {
                string query = $"SELECT * FROM daily_work_schedule_detaildetail WHERE employee_id=@employeeId AND daily_work_schedule_code=@dailyWorkScheduleCode";
                return await _mysqlConn.QueryFirstOrDefaultAsync<Models.PwsDwsDetailDetail>(query, new { employeeId, dailyWorkScheduleCode });
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> GetStartDateFromEmployment(string employeeId)
        {
            using var _mysqlConn = (MySqlConnection)await _connFactory.CreateConnectionAsync();

            try
            {
                string query = $"SELECT join_date FROM employment WHERE employee_id = @employeeId";
                var result = await _mysqlConn.QueryFirstOrDefaultAsync<string?>(query, new { employeeId });
                return result!;
            }
            catch
            {
                throw;
            }
        }
    }
}
