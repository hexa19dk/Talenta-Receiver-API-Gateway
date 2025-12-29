using Dapper;
using System.Data.SqlClient;
using TalentaReceiver.Config;
using TalentaReceiver.Models.Talenta;

namespace TalentaReceiver.Repositories.MsSql
{
    public interface IMTBBGCompanyDb
    {
        Task<MTBBGCompany> GetById(int? IdTalenta);
    }
    public class MTBBGCompanyDb : IMTBBGCompanyDb
    {
        #region SqlCommand
        string table = "MT_BBG_Company";
        string fields = "IdTalenta,IdBluebird,[Desc]";
        #endregion

        private readonly IDbConnectionMsSqlFactory _conFactory;
        private readonly SqlConnection _conn;

        public MTBBGCompanyDb(IDbConnectionMsSqlFactory conFactory)
        {
            _conFactory = conFactory ?? throw new ArgumentNullException(nameof(conFactory));
            _conn = (SqlConnection)_conFactory.CreateConnectionAsync().Result;
        }

        public async Task<MTBBGCompany> GetById(int? IdTalenta)
        {
            string sqlQuery = $@"SELECT {fields} FROM {table} WHERE IdTalenta=@IdTalenta";
            return await _conn.QuerySingleOrDefaultAsync<MTBBGCompany>(sqlQuery, 
                new { IdTalenta = IdTalenta });
        }
    }
}
