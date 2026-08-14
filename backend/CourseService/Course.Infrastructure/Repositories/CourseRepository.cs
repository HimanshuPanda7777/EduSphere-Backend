using Course.Application.Interfaces;
using Course.Domain;
using Microsoft.EntityFrameworkCore;

namespace Course.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly CourseDbContext _dbContext;

    public CourseRepository(CourseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CourseEntity?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Courses.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<CourseEntity>> GetAllAsync()
    {
        return await _dbContext.Courses.ToListAsync();
    }

    public async Task<IEnumerable<CourseEntity>> GetByInstructorIdAsync(Guid instructorId)
    {
        return await _dbContext.Courses
            .Where(c => c.InstructorId == instructorId)
            .ToListAsync();
    }

    public async Task<IEnumerable<CourseEntity>> GetByCategoryAsync(string category)
    {
        return await _dbContext.Courses
            .Where(c => c.Category == category && c.IsPublished)
            .ToListAsync();
    }

    public async Task<IEnumerable<CourseEntity>> GetPublishedAsync()
    {
        return await _dbContext.Courses
            .Where(c => c.IsPublished)
            .ToListAsync();
    }

    public async Task AddAsync(CourseEntity course)
    {
        await _dbContext.Courses.AddAsync(course);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(CourseEntity course)
    {
        _dbContext.Courses.Update(course);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(CourseEntity course)
    {
        _dbContext.Courses.Remove(course);
        await _dbContext.SaveChangesAsync();
    }
}
