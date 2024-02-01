//By Sauraav Jayrajh
using System.Data.Entity;
namespace lightlauncher
{
    public class DBContext : DbContext
    {
        public DbSet<Game> Games { get; set; }
    }
}
