using SharedKernel;

namespace Core.Domain;

public class Progress : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public string LessonId { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
}
