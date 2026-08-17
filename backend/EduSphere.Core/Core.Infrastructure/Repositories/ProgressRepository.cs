using Core.Application.Interfaces;
using Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Repositories;

public class ProgressRepository : IProgressRepository
{
    private readonly CoreDbContext _dbContext;

    public ProgressRepository(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Progress?> GetByIdAsync(Guid id)
    {
        return await _dbContext.ProgressRecords.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Progress>> GetByEnrollmentIdAsync(Guid enrollmentId)
    {
        return await _dbContext.ProgressRecords
            .Where(p => p.EnrollmentId == enrollmentId)
            .OrderBy(p => p.CompletedAt)
            .ToListAsync();
    }

    public async Task<Progress?> GetByEnrollmentAndLessonAsync(
        Guid enrollmentId, string lessonId)
    {
        return await _dbContext.ProgressRecords
            .FirstOrDefaultAsync(p =>
                p.EnrollmentId == enrollmentId && p.LessonId == lessonId);
    }

    public async Task AddAsync(Progress progress)
    {
        await _dbContext.ProgressRecords.AddAsync(progress);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Progress progress)
    {
        _dbContext.ProgressRecords.Update(progress);
        await _dbContext.SaveChangesAsync();
    }
}
