using Dapper;
using System.Data.SqlClient;
using TalentaReceiver.Config;
using TalentaReceiver.Models.Talenta;

namespace TalentaReceiver.Repositories.MsSql
{
    public interface IMTBBGJobLevelDb
    {
        Task<MTBBGJobLevel> GetByDescription(string description);
    }

    public class MTBBGJobLevelDb : IMTBBGJobLevelDb
    {
        #region SqlCommand
        string table = "MT_BBG_JobLevel";
        string fields = "Code,[Desc]";
        #endregion

        private readonly IDbConnectionMsSqlFactory _conFactory;
        private readonly SqlConnection _conn;

        public MTBBGJobLevelDb(IDbConnectionMsSqlFactory connectionFactory)
        {
            _conFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _conn = (SqlConnection)_conFactory.CreateConnectionAsync().Result; // JKT
        }

        public async Task<MTBBGJobLevel> GetByDescription(string description)
        {
            string sqlQuery = $@"SELECT {fields} FROM {table} WHERE [Desc]=@Desc";
            return await _conn.QuerySingleOrDefaultAsync<MTBBGJobLevel>(sqlQuery,
                new { Desc = description });
        }
    }
}
