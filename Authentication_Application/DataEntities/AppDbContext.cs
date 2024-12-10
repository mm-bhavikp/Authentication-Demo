using Authentication_Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace Authentication_Application.DataEntities
{
    
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
    }
    
}
