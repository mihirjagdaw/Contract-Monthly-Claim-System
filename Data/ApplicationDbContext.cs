using Microsoft.EntityFrameworkCore;
using ST10449392_CLDV6212_POE.Models;

namespace ST10449392_CLDV6212_POE.Data
{
    public class ApplicationDbContext : DbContext
    {
        // constructor
        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        // DbSets
        public DbSet<User> Users { get; set; }
        
        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
