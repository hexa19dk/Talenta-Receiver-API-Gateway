using Dapper;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using TalentaReceiver.Config;
using TalentaReceiver.Models;

namespace TalentaReceiver.Repositories.MsSql
{
    public interface IMTPesertaDb
    {
        Task<string> Add(List<MT_Peserta_Temp> lData);
        Task<bool> EmpResign(string nip, string status, string resignDate);
        Task<bool> EmpCancelResign(string nip, string statusPeserta);
        Task<bool> GetEmployeeId(string empId);
        Task<Models.MTPesertaInfo> GetMTPeserta(string empId);
    }
    public class MTPesertaDb : IMTPesertaDb
    {
        private readonly IDbConnectionMsSqlFactory _conFactory;
        private readonly IDbConnectionMsSqlFactory2 _conFactory2;
        private readonly SqlConnection _conn;
        private readonly SqlConnection _conn2;

        public MTPesertaDb(IDbConnectionMsSqlFactory connectionFactory, IDbConnectionMsSqlFactory2 connectionFactory2)
        {
            _conFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _conFactory2 = connectionFactory2 ?? throw new ArgumentNullException(nameof(connectionFactory2));
            _conn = (SqlConnection)_conFactory.CreateConnectionAsync().Result; //JKT
            _conn2 = (SqlConnection)_conFactory2.CreateConnectionAsync().Result;  //SBY
        }

        public async Task<bool> GetEmployeeId(string empId) 
        {
            try
            {
                string query = @"select nip from MT_Peserta where nip=@empId";
                return await _conn.QueryFirstOrDefaultAsync<bool>(query, new { empId });
            }
            catch
            {
                throw;
            }
        }

        public async Task<Models.MTPesertaInfo> GetMTPeserta(string empId)
        {
            try
            {
                string query = @"select NIP, TanggalMasuk, TanggalNonAktif, NamaPeserta, TanggalLahir, StatusPerkawinan, JenisKelamin from MT_Peserta where NIP=@empId";
                return await _conn.QueryFirstOrDefaultAsync<MTPesertaInfo>(query, new { empId });
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> Add(List<MT_Peserta_Temp> lData)
        {
            try
            {
                //_ = await _conn.ExecuteAsync("delete from MT_PESERTA_TEMP");
                //_ = await _conn2.ExecuteAsync("delete from MT_PESERTA_TEMP");

                foreach (var item in lData)
                {
                    var region = Region(item.KodePool);

                    if (item.ErrorMsg.IsNullOrEmpty())
                    {
                        await InsertToTemp(item, item.StatusPeserta);
                    }
                    else
                    {
                        await InsertToTempErr(item, item.StatusPeserta);
                    }
                }

                return "OK";
            }
            catch (SqlException ex)
            {
                throw new Exception($"An error occurred while executing SQL: {ex.Message}", ex);
            }
        }

        public async Task InsertToTemp(MT_Peserta_Temp o, string statusPeserta)
        {
            try
            {
                string sqlQuery = $@"insert into MT_PESERTA_TEMP (KodePool,
                        NIP, NamaPeserta,TanggalLahir,JenisKelamin,StatusPerkawinan,
                        FlagPegawai,KodePrsh,KodeJabatan,KodeBagian,
                        StatusPeserta,StatusKaryawan,TanggalMasuk,TanggalAngkat,
                        CreateDate,CreateTime) values (@KodePool,
                        @NIP, @NamaPeserta,@TanggalLahir,@JenisKelamin,@StatusPerkawinan,
                        @FlagPegawai,@KodePrsh,@KodeJabatan,@KodeBagian,
                        @StatusPeserta,@StatusKaryawan,@TanggalMasuk,@TanggalAngkat,
                        @CreateDate,@CreateTime)";

                if (o != null)
                {
                    if (o.KodePool == "LS" || o.KodePool == "DK")
                    {
                        await _conn2.ExecuteAsync(sqlQuery, new
                        {
                            KodePool            = o.KodePool,
                            NIP                 = o.NIP,
                            NamaPeserta         = o.NamaPeserta,
                            TanggalLahir        = o.TanggalLahir,
                            JenisKelamin        = o.JenisKelamin,
                            StatusPerkawinan    = o.StatusPerkawinan,
                            FlagPegawai         = o.FlagPegawai,
                            KodePrsh            = o.KodePrsh,
                            KodeJabatan         = o.KodeJabatan,
                            KodeBagian          = o.KodeBagian,
                            //StatusPeserta     = o.StatusPeserta,
                            StatusPeserta       = statusPeserta,
                            StatusKaryawan      = o.StatusKaryawan,
                            TanggalMasuk        = o.TanggalMasuk,
                            TanggalAngkat       = o.TanggalAngkat,
                            CreateDate          = DateTime.Today.ToString("yyyyMMdd"),
                            CreateTime          = DateTime.Now.ToString("HHmmss"),
                        });
                    }
                    else
                    {
                        var result = await _conn.ExecuteAsync(sqlQuery, new
                        {
                            KodePool            = o.KodePool,
                            NIP                 = o.NIP,
                            NamaPeserta         = o.NamaPeserta,
                            TanggalLahir        = o.TanggalLahir,
                            JenisKelamin        = o.JenisKelamin,
                            StatusPerkawinan    = o.StatusPerkawinan,
                            FlagPegawai         = o.FlagPegawai,
                            KodePrsh            = o.KodePrsh,
                            KodeJabatan         = o.KodeJabatan,
                            KodeBagian          = o.KodeBagian,
                            StatusPeserta       = statusPeserta,
                            StatusKaryawan      = o.StatusKaryawan,
                            TanggalMasuk        = o.TanggalMasuk,
                            TanggalAngkat       = o.TanggalAngkat,
                            CreateDate          = DateTime.Today.ToString("yyyyMMdd"),
                            CreateTime          = DateTime.Now.ToString("HHmmss"),
                        });
                    }
                }
            }
            catch { throw; }
        }        

        public async Task InsertToTempErr(MT_Peserta_Temp o, string statusPeserta)
        {
            string sqlQuery = $@"insert into MT_PESERTA_Temp_Error (KodePool,
                        NIP, NamaPeserta,TanggalLahir,JenisKelamin,StatusPerkawinan,
                        FlagPegawai,KodePrsh,KodeJabatan,KodeBagian,
                        StatusPeserta,StatusKaryawan,TanggalMasuk,TanggalAngkat,
                        CreateDate,CreateTime, ErrorMsg) values (@KodePool,
                        @NIP, @NamaPeserta,@TanggalLahir,@JenisKelamin,@StatusPerkawinan,
                        @FlagPegawai,@KodePrsh,@KodeJabatan,@KodeBagian,
                        @StatusPeserta,@StatusKaryawan,@TanggalMasuk,@TanggalAngkat,
                        @CreateDate,@CreateTime, @ErrorMsg)";

            if (o != null)
            {
                if (o.KodePool == "LS" || o.KodePool == "DK")
                {
                    await _conn2.ExecuteAsync(sqlQuery, new
                    {
                        KodePool = o.KodePool,
                        NIP = o.NIP,
                        NamaPeserta = o.NamaPeserta,
                        TanggalLahir = o.TanggalLahir,
                        JenisKelamin = o.JenisKelamin,
                        StatusPerkawinan = o.StatusPerkawinan,
                        FlagPegawai = o.FlagPegawai,
                        KodePrsh = o.KodePrsh,
                        KodeJabatan = o.KodeJabatan,
                        KodeBagian = o.KodeBagian,
                        //StatusPeserta     = o.StatusPeserta,
                        StatusPeserta = statusPeserta,
                        StatusKaryawan = o.StatusKaryawan,
                        TanggalMasuk = o.TanggalMasuk,
                        TanggalAngkat = o.TanggalAngkat,
                        CreateDate = DateTime.Today.ToString("yyyyMMdd"),
                        CreateTime = DateTime.Now.ToString("HHmmss"),
                        ErrorMsg = o.ErrorMsg,
                    });
                }
                else
                {
                    var result = await _conn.ExecuteAsync(sqlQuery, new
                    {
                        KodePool = o.KodePool,
                        NIP = o.NIP,
                        NamaPeserta = o.NamaPeserta,
                        TanggalLahir = o.TanggalLahir,
                        JenisKelamin = o.JenisKelamin,
                        StatusPerkawinan = o.StatusPerkawinan,
                        FlagPegawai = o.FlagPegawai,
                        KodePrsh = o.KodePrsh,
                        KodeJabatan = o.KodeJabatan,
                        KodeBagian = o.KodeBagian,
                        StatusPeserta = statusPeserta,
                        StatusKaryawan = o.StatusKaryawan,
                        TanggalMasuk = o.TanggalMasuk,
                        TanggalAngkat = o.TanggalAngkat,
                        CreateDate = DateTime.Today.ToString("yyyyMMdd"),
                        CreateTime = DateTime.Now.ToString("HHmmss"),
                        ErrorMsg = o.ErrorMsg,
                    });
                }
            }
        }

        private string Region(string kodePool)
        {
            var data = _conn.QueryFirstOrDefault<Models.MT_Pool>("select Kode, KodeRegion from MT_Pool where Kode = @kodepool", new
            {
                @kodePool = kodePool,
            });
            return data.Region;
        }

        public void InsertPersertaJkt(MT_Peserta_Temp req, string statusPeserta)
        {
            try
            {
                string sqlQuery = $@"insert into MT_PESERTA_TEMP (KodePool,
                        NIP, NamaPeserta,TanggalLahir,JenisKelamin,StatusPerkawinan,
                        FlagPegawai,KodePrsh,KodeJabatan,KodeBagian,
                        StatusPeserta,StatusKaryawan,TanggalMasuk,TanggalAngkat,
                        CreateDate,CreateTime) values (@KodePool,
                        @NIP, @NamaPeserta,@TanggalLahir,@JenisKelamin,@StatusPerkawinan,
                        @FlagPegawai,@KodePrsh,@KodeJabatan,@KodeBagian,
                        @StatusPeserta,@StatusKaryawan,@TanggalMasuk,@TanggalAngkat,
                        @CreateDate,@CreateTime)";

                if (req != null)
                {
                    _conn.ExecuteAsync(sqlQuery, new
                    {
                        KodePool = req.KodePool,
                        NIP = req.NIP,
                        NamaPeserta = req.NamaPeserta,
                        TanggalLahir = req.TanggalLahir,
                        JenisKelamin = req.JenisKelamin,
                        StatusPerkawinan = req.StatusPerkawinan,
                        FlagPegawai = req.FlagPegawai,
                        KodePrsh = req.KodePrsh,
                        KodeJabatan = req.KodeJabatan,
                        KodeBagian = req.KodeBagian,
                        StatusPeserta = statusPeserta,
                        StatusKaryawan = req.StatusKaryawan,
                        TanggalMasuk = req.TanggalMasuk,
                        TanggalAngkat = req.TanggalAngkat,
                        CreateDate = DateTime.Today.ToString("yyyyMMdd"),
                        CreateTime = DateTime.Now.ToString("HHmmss"),
                    });
                }
            }
            catch(Exception ex)
            {
                throw;
            }            
        }

        public void InsertPesertaSby(MT_Peserta_Temp req, string statusPeserta)
        {
            string sqlQuery = $@"insert into MT_PESERTA_TEMP (KodePool,
                        NIP, NamaPeserta,TanggalLahir,JenisKelamin,StatusPerkawinan,
                        FlagPegawai,KodePrsh,KodeJabatan,KodeBagian,
                        StatusPeserta,StatusKaryawan,TanggalMasuk,TanggalAngkat,
                        CreateDate,CreateTime) values (@KodePool,
                        @NIP, @NamaPeserta,@TanggalLahir,@JenisKelamin,@StatusPerkawinan,
                        @FlagPegawai,@KodePrsh,@KodeJabatan,@KodeBagian,
                        @StatusPeserta,@StatusKaryawan,@TanggalMasuk,@TanggalAngkat,
                        @CreateDate,@CreateTime)";

            if (req != null)
            {
                _conn2.ExecuteAsync(sqlQuery, new
                {
                    KodePool         = req.KodePool,
                    NIP              = req.NIP,
                    NamaPeserta      = req.NamaPeserta,
                    TanggalLahir     = req.TanggalLahir,
                    JenisKelamin     = req.JenisKelamin,
                    StatusPerkawinan = req.StatusPerkawinan,
                    FlagPegawai      = req.FlagPegawai,
                    KodePrsh         = req.KodePrsh,
                    KodeJabatan      = req.KodeJabatan,
                    KodeBagian       = req.KodeBagian,
                    StatusPeserta    = statusPeserta,
                    StatusKaryawan   = req.StatusKaryawan,
                    TanggalMasuk     = req.TanggalMasuk,
                    TanggalAngkat    = req.TanggalAngkat,
                    CreateDate       = DateTime.Today.ToString("yyyyMMdd"),
                    CreateTime       = DateTime.Now.ToString("HHmmss"),
                });
            }
        }

        public async Task<bool> EmpResign(string nip, string statusPeserta, string resignDate)
        {
            try
            {
                var query = await _conn.ExecuteAsync(@"update MT_Peserta_Temp set StatusPeserta = @statusPeserta, TanggalNonAktif = @resignDate where NIP = @nip", param: new
                {
                    NIP = nip,
                    StatusPeserta = statusPeserta,
                    TanggalNonAktif = resignDate
                });

                return query > 0;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> EmpCancelResign(string nip, string statusPeserta)
        {
            try
            {
                var query = await _conn.ExecuteAsync(@"update MT_Peserta_Temp set StatusPeserta = @statusPeserta where NIP = @nip", param: new
                {
                    NIP = nip,
                    StatusPeserta = statusPeserta
                });

                return query > 0;
            }
            catch
            {
                throw;
            }
        }
    }
}
