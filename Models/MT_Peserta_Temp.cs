using System.ComponentModel.DataAnnotations;

namespace TalentaReceiver.Models
{
    public class MT_Peserta_Temp
    {
        [Required]
        [MaxLength(2)]
        public string KodePool { get; set; }

        [Required]
        [MaxLength(8)]
        public string NIP { get; set; }

        [Required]
        [MaxLength(100)]
        public string NamaPeserta { get; set; }

        [Required]
        public string TanggalLahir { get; set; }

        [Required]
        [MaxLength(1)]
        public string JenisKelamin { get; set; }

        [Required]
        [MaxLength(1)]
        public string StatusPerkawinan { get; set; }

        [Required]
        [MaxLength(1)]
        public string FlagPegawai { get; set; }

        [Required]
        [MaxLength(5)]
        public string KodePrsh { get; set; }

        [Required]
        [MaxLength(5)]
        public string KodeJabatan { get; set; }

        [Required]
        [MaxLength(5)]
        public string KodeBagian { get; set; }

        [Required]
        [MaxLength(1)]
        public string StatusPeserta { get; set; }

        [Required]
        [MaxLength(1)]
        public string StatusKaryawan { get; set; }

        public string TanggalMasuk { get; set; }
        public string TanggalAngkat { get; set; }
        public string CreateDate { get; set; }
        public string CreateTime { get; set; }
        //public string StateData { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class MTPesertaInfo
    {
        public string NIP { get; set; }
        public string TanggalMasuk { get; set; }
        public string TanggalNonAktif { get; set; }
        public string NamaPeserta { get; set; }
        public string TanggalLahir { get; set; }
        public string StatusPerkawinan { get; set; }
        public string JenisKelamin { get; set; }
    }

}
