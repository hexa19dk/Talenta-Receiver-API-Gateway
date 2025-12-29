using TalentaReceiver.Repositories.MsSql;

namespace jpk3service.Repositories
{
    public interface IMTPesertaTempRepository
    {
        IMTPesertaDb db();
    }
    public class MTPesertaTempRepository: IMTPesertaTempRepository
    {
        private readonly IMTPesertaDb _db;

        public MTPesertaTempRepository(IMTPesertaDb Db)
        {
            _db = Db ?? throw new ArgumentNullException(nameof(Db));
        }

        public IMTPesertaDb db()
        {
            return _db;
        }
    }
}
