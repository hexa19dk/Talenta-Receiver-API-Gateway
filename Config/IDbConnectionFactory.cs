using System.Data;

namespace TalentaReceiver.Config
{
    public interface IDbConnectionFactory
    {
        public Task<IDbConnection> CreateConnectionAsync();
    }

    public interface IDbConnectionMsSqlFactory
    {
        public Task<IDbConnection> CreateConnectionAsync();
    }

    public interface IDbConnectionMsSqlFactory2
    {
        public Task<IDbConnection> CreateConnectionAsync();
    }
}