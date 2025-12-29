using jpk3service.Repositories.Cache;
using TalentaReceiver.Repositories.MySql;

namespace TalentaReceiver.Repositories
{
    public interface IJobRepository
    {
        IJobDb db();
        IJobCache cache();
    }

    public class JobRepository : IJobRepository
    {
        private readonly IJobDb _Db;
        private readonly IJobCache _Cache;

        public JobRepository(IJobDb Db, IJobCache Cache)
        {
            _Db = Db ?? throw new ArgumentNullException(nameof(Db));
            _Cache = Cache ?? throw new ArgumentNullException(nameof(Cache));
        }

        public IJobDb db()
        {
            return _Db;
        }

        public IJobCache cache()
        {
            return _Cache;
        }
    }
}
