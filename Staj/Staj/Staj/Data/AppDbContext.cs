using Microsoft.EntityFrameworkCore;
using Staj.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staj.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Tutanak> Tutanaklar { get; set; }//once modeldeki class sonra tabloya verilecek ad

        public DbSet<Login> Logins { get; set; }
    }
}
