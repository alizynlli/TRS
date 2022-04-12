using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TRS.Data.Models;

namespace TRS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ClientTask> ClientTasks { get; set; }
        public DbSet<ClientTaskType> ClientTaskTypes { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<TaskOperation> TaskOperations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
