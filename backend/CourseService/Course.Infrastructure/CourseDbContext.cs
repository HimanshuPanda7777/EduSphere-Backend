using Microsoft.EntityFrameworkCore;
using Course.Domain;

namespace Course.Infrastructure;

public class CourseDbContext : DbContext
{
    public CourseDbContext(DbContextOptions<CourseDbContext> options) : base(options)
    {
    }

    public DbSet<CourseEntity> Courses => Set<CourseEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CourseEntity>(entity =>
        {
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(50);
        });
    }
}
