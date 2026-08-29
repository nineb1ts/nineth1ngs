using System.IO;
using Microsoft.EntityFrameworkCore;
using nineth1ngs.Models;

namespace nineth1ngs.Data;

public sealed class Th1ngDbContext : DbContext
{
    public DbSet<Th1ng> Th1ngs => Set<Th1ng>();

    public Th1ngDbContext(DbContextOptions<Th1ngDbContext>? options = null)
        : base(options ?? new DbContextOptions<Th1ngDbContext>())
    {
    }

    public static string DatabaseDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "nineth1ngs");

    public static string DatabasePath => Path.Combine(DatabaseDirectory, "nineth1ngs.db");

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        Directory.CreateDirectory(DatabaseDirectory);
        optionsBuilder.UseSqlite($"Data Source={DatabasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Th1ng>(entity =>
        {
            entity.HasKey(th1ng => th1ng.Id);
            entity.Property(th1ng => th1ng.Text).IsRequired();
            entity.Property(th1ng => th1ng.CreatedAt).IsRequired();
            entity.HasOne<Th1ng>()
                .WithMany()
                .HasForeignKey(th1ng => th1ng.ParentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
