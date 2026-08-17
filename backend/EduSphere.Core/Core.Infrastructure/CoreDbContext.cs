using Microsoft.EntityFrameworkCore;
using Core.Domain;

namespace Core.Infrastructure;

public class CoreDbContext : DbContext
{
    public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options)
    {
    }

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Progress> ProgressRecords => Set<Progress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enrollment configuration
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.Property(e => e.Status).HasMaxLength(20);

            // Unique index: a student can only enroll in a course once
            entity.HasIndex(e => new { e.StudentId, e.CourseId })
                .IsUnique();
        });

        // Progress configuration
        modelBuilder.Entity<Progress>(entity =>
        {
            entity.Property(e => e.LessonId).HasMaxLength(100);
            entity.Property(e => e.LessonTitle).HasMaxLength(300);

            // Unique index: a lesson can only be recorded once per enrollment
            entity.HasIndex(e => new { e.EnrollmentId, e.LessonId })
                .IsUnique();
        });
    }
}
