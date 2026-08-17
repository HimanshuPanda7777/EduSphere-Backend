using SharedKernel;

namespace Core.Domain;

public class Enrollment : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public string Status { get; set; } = EnrollmentStatus.Active;
    public int ProgressPercentage { get; set; } = 0;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
