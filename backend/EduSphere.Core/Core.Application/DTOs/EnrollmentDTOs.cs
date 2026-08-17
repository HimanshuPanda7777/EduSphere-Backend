using System.ComponentModel.DataAnnotations;

namespace Core.Application.DTOs;

public class EnrollRequest
{
    [Required]
    public Guid CourseId { get; set; }
}

public class EnrollmentResponse
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ProgressPercentage { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
