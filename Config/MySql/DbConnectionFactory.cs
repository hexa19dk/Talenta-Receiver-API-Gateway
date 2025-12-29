using MySql.Data.MySqlClient;
using System.Data;
using TalentaReceiver.Config;

namespace TalentaReceiver.Config.MySql
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            try
            {
                var connection = new MySqlConnection(_connectionString);
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                return connection;
            }
            catch
            {
                throw;
            }
        }
    }
}
