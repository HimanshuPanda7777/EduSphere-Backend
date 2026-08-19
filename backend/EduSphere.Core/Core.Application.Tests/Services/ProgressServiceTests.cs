using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Exceptions;

namespace Core.Application.Tests.Services;

public class ProgressServiceTests
{
    private readonly Mock<IProgressRepository> _progressRepositoryMock;
    private readonly Mock<IEnrollmentRepository> _enrollmentRepositoryMock;
    private readonly Mock<ILogger<ProgressService>> _loggerMock;
    private readonly ProgressService _progressService;

    public ProgressServiceTests()
    {
        _progressRepositoryMock = new Mock<IProgressRepository>();
        _enrollmentRepositoryMock = new Mock<IEnrollmentRepository>();
        _loggerMock = new Mock<ILogger<ProgressService>>();

        _progressService = new ProgressService(
            _progressRepositoryMock.Object,
            _enrollmentRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RecordProgressAsync_ShouldThrowNotFoundException_WhenEnrollmentNotFound()
    {
        // Arrange
        var request = new RecordProgressRequest { EnrollmentId = Guid.NewGuid(), LessonId = "lesson-123" };
        _enrollmentRepositoryMock.Setup(repo => repo.GetByIdAsync(request.EnrollmentId))
            .ReturnsAsync((Enrollment?)null);

        // Act
        Func<Task> act = async () => await _progressService.RecordProgressAsync(request, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Enrollment with ID {request.EnrollmentId} not found.");
    }

    [Fact]
    public async Task RecordProgressAsync_ShouldThrowValidationException_WhenNotEnrollmentOwner()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var differentStudentId = Guid.NewGuid();
        
        var enrollment = new Enrollment { Id = enrollmentId, StudentId = studentId };
        var request = new RecordProgressRequest { EnrollmentId = enrollmentId };
        
        _enrollmentRepositoryMock.Setup(repo => repo.GetByIdAsync(enrollmentId))
            .ReturnsAsync(enrollment);

        // Act
        Func<Task> act = async () => await _progressService.RecordProgressAsync(request, differentStudentId);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("You can only record progress for your own enrollment.");
    }

    [Fact]
    public async Task RecordProgressAsync_ShouldReturnExistingProgress_WhenAlreadyRecorded()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var lessonId = "lesson-123";
        
        var enrollment = new Enrollment { Id = enrollmentId, StudentId = studentId };
        var existingProgress = new Progress { Id = Guid.NewGuid(), EnrollmentId = enrollmentId, LessonId = lessonId, IsCompleted = true };
        
        var request = new RecordProgressRequest { EnrollmentId = enrollmentId, LessonId = lessonId };
        
        _enrollmentRepositoryMock.Setup(repo => repo.GetByIdAsync(enrollmentId))
            .ReturnsAsync(enrollment);
        _progressRepositoryMock.Setup(repo => repo.GetByEnrollmentAndLessonAsync(enrollmentId, lessonId))
            .ReturnsAsync(existingProgress);

        // Act
        var response = await _progressService.RecordProgressAsync(request, studentId);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(existingProgress.Id);
        response.IsCompleted.Should().BeTrue();
        
        // Ensure AddAsync is NEVER called (idempotency check)
        _progressRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Progress>()), Times.Never);
    }

    [Fact]
    public async Task RecordProgressAsync_ShouldRecordNewProgress_WhenSuccessful()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var lessonId = "lesson-123";
        
        var enrollment = new Enrollment { Id = enrollmentId, StudentId = studentId };
        
        var request = new RecordProgressRequest { EnrollmentId = enrollmentId, LessonId = lessonId, LessonTitle = "Test Lesson" };
        
        _enrollmentRepositoryMock.Setup(repo => repo.GetByIdAsync(enrollmentId))
            .ReturnsAsync(enrollment);
        _progressRepositoryMock.Setup(repo => repo.GetByEnrollmentAndLessonAsync(enrollmentId, lessonId))
            .ReturnsAsync((Progress?)null);

        // Act
        var response = await _progressService.RecordProgressAsync(request, studentId);

        // Assert
        response.Should().NotBeNull();
        response.EnrollmentId.Should().Be(enrollmentId);
        response.LessonId.Should().Be(lessonId);
        response.LessonTitle.Should().Be("Test Lesson");
        response.IsCompleted.Should().BeTrue();
        
        _progressRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Progress>()), Times.Once);
    }
}
