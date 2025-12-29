using System.Data;
using System.Data.SqlClient;

namespace TalentaReceiver.Config.MsSql
{
    public class DbConnectionMsSqlFactory : IDbConnectionMsSqlFactory
    {
        private readonly string _connectionString;

        public DbConnectionMsSqlFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            try
            {
                var connection = new SqlConnection(_connectionString);
                return connection;
            }
            catch
            {
                throw;
            }
        }
    }

    public class DbConnectionMsSqlFactory2 : IDbConnectionMsSqlFactory2
    {
        private readonly string _connectionString;

        public DbConnectionMsSqlFactory2(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            try
            {
                var connection = new SqlConnection(_connectionString);
                return connection;
            }
            catch
            {
                throw;
            }
        }
    }
}
