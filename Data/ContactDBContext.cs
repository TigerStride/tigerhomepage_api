using Microsoft.EntityFrameworkCore;
using ContactSvc.Dtos;

namespace ContactSvc.Data
{
    public class ContactDbContext : DbContext
    {
        public DbSet<CustomerMessage> CustomerMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContactDbContext).Assembly);
        }
    }
}