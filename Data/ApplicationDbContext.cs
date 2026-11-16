using Microsoft.EntityFrameworkCore;
using ScholarTrack.Models;

namespace ScholarTrack.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Scholar> Scholars => Set<Scholar>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Store enums as readable text in the database.
        modelBuilder.Entity<Scholar>().Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Scholar>().Property(s => s.Allowance).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Scholar>().Property(s => s.FullName).HasMaxLength(100);
        modelBuilder.Entity<Scholar>().Property(s => s.Program).HasMaxLength(50);
    }
}
