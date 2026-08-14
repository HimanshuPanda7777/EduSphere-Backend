using Course.Domain;

namespace Course.Application.Interfaces;

public interface ICourseRepository
{
    Task<CourseEntity?> GetByIdAsync(Guid id);
    Task<IEnumerable<CourseEntity>> GetAllAsync();
    Task<IEnumerable<CourseEntity>> GetByInstructorIdAsync(Guid instructorId);
    Task<IEnumerable<CourseEntity>> GetByCategoryAsync(string category);
    Task<IEnumerable<CourseEntity>> GetPublishedAsync();
    Task AddAsync(CourseEntity course);
    Task UpdateAsync(CourseEntity course);
    Task DeleteAsync(CourseEntity course);
}
