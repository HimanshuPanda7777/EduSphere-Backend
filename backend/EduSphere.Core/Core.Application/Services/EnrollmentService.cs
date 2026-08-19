using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Domain;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;

namespace Core.Application.Services;

public class EnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        ILogger<EnrollmentService> logger)
    {
        _enrollmentRepository = enrollmentRepository;
        _logger = logger;
    }

    public async Task<EnrollmentResponse> EnrollAsync(EnrollRequest request, Guid studentId)
    {
        // Check for duplicate enrollment
        var existing = await _enrollmentRepository
            .GetByStudentAndCourseAsync(studentId, request.CourseId);

        if (existing != null)
        {
            throw new ValidationException("You are already enrolled in this course.");
        }

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = request.CourseId,
            Status = EnrollmentStatus.Active,
            ProgressPercentage = 0,
            EnrolledAt = DateTime.UtcNow,
            CompletedAt = null
        };

        await _enrollmentRepository.AddAsync(enrollment);

        _logger.LogInformation(
            "Enrollment created: {EnrollmentId} — Student {StudentId} enrolled in Course {CourseId}",
            enrollment.Id, studentId, request.CourseId);

        return MapToResponse(enrollment);
    }

    public async Task<EnrollmentResponse> GetEnrollmentByIdAsync(Guid id)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(id);
        if (enrollment == null)
        {
            throw new NotFoundException($"Enrollment with ID {id} not found.");
        }

        return MapToResponse(enrollment);
    }

    public async Task<IEnumerable<EnrollmentResponse>> GetMyEnrollmentsAsync(Guid studentId)
    {
        var enrollments = await _enrollmentRepository.GetByStudentIdAsync(studentId);
        return enrollments.Select(MapToResponse);
    }

    public async Task<IEnumerable<EnrollmentResponse>> GetEnrollmentsByCourseAsync(Guid courseId)
    {
        var enrollments = await _enrollmentRepository.GetByCourseIdAsync(courseId);
        return enrollments.Select(MapToResponse);
    }

    public async Task<EnrollmentResponse> UpdateProgressAsync(
        Guid enrollmentId, int progressPercentage, Guid studentId)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
        if (enrollment == null)
        {
            throw new NotFoundException($"Enrollment with ID {enrollmentId} not found.");
        }

        if (enrollment.StudentId != studentId)
        {
            throw new ValidationException("You can only update your own enrollment progress.");
        }

        enrollment.ProgressPercentage = progressPercentage;

        if (progressPercentage >= 100)
        {
            enrollment.Status = EnrollmentStatus.Completed;
            enrollment.CompletedAt = DateTime.UtcNow;
            enrollment.ProgressPercentage = 100;

            _logger.LogInformation(
                "Enrollment completed: {EnrollmentId} — Student {StudentId} completed Course {CourseId}",
                enrollmentId, studentId, enrollment.CourseId);
        }

        await _enrollmentRepository.UpdateAsync(enrollment);

        return MapToResponse(enrollment);
    }

    public async Task UnenrollAsync(Guid id, Guid userId, string userRole)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(id);
        if (enrollment == null)
        {
            throw new NotFoundException($"Enrollment with ID {id} not found.");
        }

        if (enrollment.StudentId != userId && userRole != "Admin")
        {
            throw new ValidationException("You are not authorized to remove this enrollment.");
        }

        await _enrollmentRepository.DeleteAsync(enrollment);

        _logger.LogInformation(
            "Student unenrolled: {EnrollmentId} — Student {StudentId} from Course {CourseId}",
            id, enrollment.StudentId, enrollment.CourseId);
    }

    private static EnrollmentResponse MapToResponse(Enrollment enrollment)
    {
        return new EnrollmentResponse
        {
            Id = enrollment.Id,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            Status = enrollment.Status,
            ProgressPercentage = enrollment.ProgressPercentage,
            EnrolledAt = enrollment.EnrolledAt,
            CompletedAt = enrollment.CompletedAt
        };
    }
}
