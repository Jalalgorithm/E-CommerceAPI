using Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            :base(options)
        {
            
        }
        public DbSet<Product> Products  { get; set; }
        public DbSet<Contact> Contacts { get; set; }
    }
}
