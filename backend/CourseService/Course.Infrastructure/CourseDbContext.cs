using Microsoft.EntityFrameworkCore;
using Course.Domain;

namespace Course.Infrastructure;

public class CourseDbContext : DbContext
{
    public CourseDbContext(DbContextOptions<CourseDbContext> options) : base(options)
    {
    }

    public DbSet<CourseEntity> Courses => Set<CourseEntity>();
}
