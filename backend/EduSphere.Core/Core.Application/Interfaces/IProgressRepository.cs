using Core.Domain;

namespace Core.Application.Interfaces;

public interface IProgressRepository
{
    Task<Progress?> GetByIdAsync(Guid id);
    Task<IEnumerable<Progress>> GetByEnrollmentIdAsync(Guid enrollmentId);
    Task<Progress?> GetByEnrollmentAndLessonAsync(Guid enrollmentId, string lessonId);
    Task AddAsync(Progress progress);
    Task UpdateAsync(Progress progress);
}
