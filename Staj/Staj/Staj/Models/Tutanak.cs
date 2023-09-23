using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staj.Models
{
    public class Tutanak
    {
        public int Id { get; set; }
        public string TTNO { get; set; }
        public Nullable<DateTime> OlusturulmaTarihi { get; set; }
        public string IsYeri { get; set; }
        public string Malzeme { get; set; }
        public string Diger { get; set; }
        public Nullable<DateTime> TeslimTarihi { get; set; }
        public int TeslimEdenId { get; set; }
        public string TeslimEdenAd { get; set; }
        public string TeslimEdenDep { get; set; }
        public string TeslimEdenGorev { get; set; }
        public int TeslimAlanId { get; set; }
        public string TeslimAlanAd { get; set; }
        public string TeslimAlanDep { get; set; }
        public string TeslimAlanGorev { get; set; }
        public bool PersMailGonDurumu { get; set; }
        public bool MailOnayDurumu { get; set; }
        public Nullable<DateTime> OnaylamaTarihi { get; set; }
        public string Resim { get; set; } //string olarak dosya yolunun al
       
    }
}
