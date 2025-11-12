using Microsoft.EntityFrameworkCore;
using TaskAPI.Models;

namespace TaskAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<UserTask> Tasks => Set<UserTask>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserTask>(e =>
            {
                e.HasKey(t => t.Id);
                e.Property(t => t.Title).HasMaxLength(200).IsRequired();
                e.Property(t => t.Description).HasMaxLength(2000);
                e.Property(t => t.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();
                e.Property<DateTime>("CreatedAt").IsRequired();
                e.Property<DateTime>("UpdatedAt").IsRequired();
                e.HasIndex(t => t.Status);
            });
        }
    }
}
