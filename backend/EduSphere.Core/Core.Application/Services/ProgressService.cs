using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Domain;
using SharedKernel.Exceptions;

namespace Core.Application.Services;

public class ProgressService
{
    private readonly IProgressRepository _progressRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public ProgressService(
        IProgressRepository progressRepository,
        IEnrollmentRepository enrollmentRepository)
    {
        _progressRepository = progressRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<ProgressResponse> RecordProgressAsync(
        RecordProgressRequest request, Guid studentId)
    {
        // Verify enrollment exists and belongs to this student
        var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId);
        if (enrollment == null)
        {
            throw new NotFoundException(
                $"Enrollment with ID {request.EnrollmentId} not found.");
        }

        if (enrollment.StudentId != studentId)
        {
            throw new ValidationException(
                "You can only record progress for your own enrollment.");
        }

        // Check if this lesson was already completed (idempotent)
        var existing = await _progressRepository
            .GetByEnrollmentAndLessonAsync(request.EnrollmentId, request.LessonId);

        if (existing != null)
        {
            // Lesson already recorded — return existing record
            return MapToResponse(existing);
        }

        var progress = new Progress
        {
            EnrollmentId = request.EnrollmentId,
            LessonId = request.LessonId,
            LessonTitle = request.LessonTitle,
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow
        };

        await _progressRepository.AddAsync(progress);

        return MapToResponse(progress);
    }

    public async Task<IEnumerable<ProgressResponse>> GetProgressByEnrollmentAsync(
        Guid enrollmentId, Guid studentId)
    {
        // Verify enrollment exists and belongs to this student
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
        if (enrollment == null)
        {
            throw new NotFoundException(
                $"Enrollment with ID {enrollmentId} not found.");
        }

        if (enrollment.StudentId != studentId)
        {
            throw new ValidationException(
                "You can only view progress for your own enrollment.");
        }

        var progressItems = await _progressRepository
            .GetByEnrollmentIdAsync(enrollmentId);

        return progressItems.Select(MapToResponse);
    }

    private static ProgressResponse MapToResponse(Progress progress)
    {
        return new ProgressResponse
        {
            Id = progress.Id,
            EnrollmentId = progress.EnrollmentId,
            LessonId = progress.LessonId,
            LessonTitle = progress.LessonTitle,
            IsCompleted = progress.IsCompleted,
            CompletedAt = progress.CompletedAt
        };
    }
}
