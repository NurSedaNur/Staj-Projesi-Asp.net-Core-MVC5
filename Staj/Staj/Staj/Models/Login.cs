using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staj.Models
{
    public class Login
    {
        public int Id { get; set; }

        public string Sifre { get; set; }
        public string Adı { get; set; }
        public string Rol { get; set; }
        public string Departman { get; set; }
        public string Gorev { get; set; }
    }
}
