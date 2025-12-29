using TalentaReceiver.Repositories.MsSql;

namespace TalentaReceiver.Repositories
{
    public interface IMTBBGCompanyRepository
    {
        IMTBBGCompanyDb db();
    }

    public class MTBBGCompanyRepository : IMTBBGCompanyRepository
    {
        private readonly IMTBBGCompanyDb _db;

        public MTBBGCompanyRepository(IMTBBGCompanyDb db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
        public IMTBBGCompanyDb db()
        {
            return _db;
        }
    }
}
