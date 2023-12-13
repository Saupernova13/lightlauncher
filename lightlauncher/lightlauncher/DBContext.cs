using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lightlauncher
{
    public class DBContext : DbContext
    {
        public DbSet<Game> Games { get; set; }
    }
}
