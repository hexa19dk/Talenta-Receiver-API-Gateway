using TalentaReceiver.Repositories.MsSql;

namespace TalentaReceiver.Repositories
{
    public interface IMTEmployeeRepositories
    {
        IMTEmployee db();
        IMTPwsDws dbPwsDws();
    }

    public class MTEmployeeRepositories : IMTEmployeeRepositories
    {
        private readonly IMTEmployee _db;
        private readonly IMTPwsDws _dbPwsDws;
        public MTEmployeeRepositories(IMTEmployee Db, IMTPwsDws DbPwsDws)
        {
            _db = Db ?? throw new ArgumentNullException(nameof(Db));
            _dbPwsDws = DbPwsDws ?? throw new ArgumentNullException(nameof(DbPwsDws));
        }

        public IMTEmployee db() 
        {
            return _db;
        }

        public IMTPwsDws dbPwsDws()
        {
            return _dbPwsDws;
        }
    }
}
