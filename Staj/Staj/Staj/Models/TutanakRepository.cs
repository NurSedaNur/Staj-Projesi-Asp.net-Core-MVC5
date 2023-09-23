using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staj.Models
{
    public class TutanakRepository
    {
        private static List<Tutanak> _tutanaklar = new List<Tutanak>()
        {


            new() {TTNO = "", IsYeri = "Technocast", TeslimAlanAd = "a", TeslimAlanDep = "IT", TeslimAlanGorev = "Stajyer1" },
            new() { TTNO = "", IsYeri = "Technocast", TeslimAlanAd = "aa", TeslimAlanDep = "IT", TeslimAlanGorev = "Stajyer2"},
            new() { TTNO ="", IsYeri = "Technocast", TeslimAlanAd = "aaa", TeslimAlanDep = "IT", TeslimAlanGorev = "Stajyer3"}
        };


        public List<Tutanak> GetAll() => _tutanaklar;

        public void Add(Tutanak newTutanak) => _tutanaklar.Add(newTutanak);


    }
}
