using System.ComponentModel.DataAnnotations;

namespace Core.Application.DTOs;

public class RecordProgressRequest
{
    [Required]
    public Guid EnrollmentId { get; set; }

    [Required]
    public string LessonId { get; set; } = string.Empty;

    [Required]
    public string LessonTitle { get; set; } = string.Empty;
}

public class ProgressResponse
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public string LessonId { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
