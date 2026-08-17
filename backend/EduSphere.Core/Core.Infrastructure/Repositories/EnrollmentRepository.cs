using Core.Application.Interfaces;
using Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly CoreDbContext _dbContext;

    public EnrollmentRepository(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Enrollment?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Enrollments.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Enrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId)
    {
        return await _dbContext.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    }

    public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId)
    {
        return await _dbContext.Enrollments
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId)
    {
        return await _dbContext.Enrollments
            .Where(e => e.CourseId == courseId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();
    }

    public async Task AddAsync(Enrollment enrollment)
    {
        await _dbContext.Enrollments.AddAsync(enrollment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Enrollment enrollment)
    {
        _dbContext.Enrollments.Update(enrollment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Enrollment enrollment)
    {
        _dbContext.Enrollments.Remove(enrollment);
        await _dbContext.SaveChangesAsync();
    }
}
