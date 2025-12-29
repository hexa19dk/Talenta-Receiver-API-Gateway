using TalentaReceiver.Repositories.MsSql;

namespace TalentaReceiver.Repositories
{
    public interface IMTBBGJobLevelRepository
    {
        IMTBBGJobLevelDb db();
    }

    public class MTBBGJobLevelRepository : IMTBBGJobLevelRepository
    {
        private readonly IMTBBGJobLevelDb _db;

        public MTBBGJobLevelRepository(IMTBBGJobLevelDb db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public IMTBBGJobLevelDb db()
        {
            return _db;
        }
    }
}
