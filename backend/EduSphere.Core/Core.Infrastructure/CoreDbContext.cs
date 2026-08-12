using Microsoft.EntityFrameworkCore;
using Core.Domain;

namespace Core.Infrastructure;

public class CoreDbContext : DbContext
{
    public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options)
    {
    }

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
}
