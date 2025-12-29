using TalentaReceiver.Repositories.MsSql;

namespace TalentaReceiver.Repositories
{
    public interface IMTJobLogRepository
    {
        IMTJobLogSAP db();
    }

    public class MTJobLogSAPRepository : IMTJobLogRepository
    {
        private readonly IMTJobLogSAP _Db;

        public MTJobLogSAPRepository(IMTJobLogSAP Db) 
        {
            _Db = Db ?? throw new ArgumentNullException(nameof(Db));
        }

        public IMTJobLogSAP db()
        {
            return _Db;
        }
    }
}
