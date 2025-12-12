using Microsoft.EntityFrameworkCore;
using Baseer.Models;

namespace Baseer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Reporter> Reporters => Set<Reporter>();
        public DbSet<EmergencyReport> EmergencyReports => Set<EmergencyReport>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reporter>()
                .HasMany(r => r.EmergencyReports)
                .WithOne(e => e.Reporter)
                .HasForeignKey(e => e.ReporterId)
                .OnDelete(DeleteBehavior.Cascade);
            
        }
    }
}