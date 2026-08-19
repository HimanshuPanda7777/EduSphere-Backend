using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Exceptions;

namespace Core.Application.Tests.Services;

public class EnrollmentServiceTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepositoryMock;
    private readonly Mock<ILogger<EnrollmentService>> _loggerMock;
    private readonly EnrollmentService _enrollmentService;

    public EnrollmentServiceTests()
    {
        _enrollmentRepositoryMock = new Mock<IEnrollmentRepository>();
        _loggerMock = new Mock<ILogger<EnrollmentService>>();

        _enrollmentService = new EnrollmentService(
            _enrollmentRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task EnrollAsync_ShouldThrowValidationException_WhenAlreadyEnrolled()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var request = new EnrollRequest { CourseId = Guid.NewGuid() };
        
        _enrollmentRepositoryMock.Setup(repo => repo.GetByStudentAndCourseAsync(studentId, request.CourseId))
            .ReturnsAsync(new Enrollment());

        // Act
        Func<Task> act = async () => await _enrollmentService.EnrollAsync(request, studentId);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("You are already enrolled in this course.");
    }

    [Fact]
    public async Task EnrollAsync_ShouldReturnEnrollmentResponse_WhenSuccessful()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var request = new EnrollRequest { CourseId = Guid.NewGuid() };
        
        _enrollmentRepositoryMock.Setup(repo => repo.GetByStudentAndCourseAsync(studentId, request.CourseId))
            .ReturnsAsync((Enrollment?)null);

        // Act
        var response = await _enrollmentService.EnrollAsync(request, studentId);

        // Assert
        response.Should().NotBeNull();
        response.StudentId.Should().Be(studentId);
        response.CourseId.Should().Be(request.CourseId);
        response.Status.Should().Be(EnrollmentStatus.Active);
        response.ProgressPercentage.Should().Be(0);
        
        _enrollmentRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Enrollment>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProgressAsync_ShouldCompleteEnrollment_WhenProgressIs100()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var enrollment = new Enrollment { Id = enrollmentId, StudentId = studentId, Status = EnrollmentStatus.Active, ProgressPercentage = 50 };
        
        _enrollmentRepositoryMock.Setup(repo => repo.GetByIdAsync(enrollmentId))
            .ReturnsAsync(enrollment);

        // Act
        var response = await _enrollmentService.UpdateProgressAsync(enrollmentId, 100, studentId);

        // Assert
        response.Should().NotBeNull();
        response.ProgressPercentage.Should().Be(100);
        response.Status.Should().Be(EnrollmentStatus.Completed);
        response.CompletedAt.Should().NotBeNull();
        
        _enrollmentRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Enrollment>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProgressAsync_ShouldThrowValidationException_WhenNotEnrollmentOwner()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var differentStudentId = Guid.NewGuid();
        var enrollment = new Enrollment { Id = enrollmentId, StudentId = studentId };
        
        _enrollmentRepositoryMock.Setup(repo => repo.GetByIdAsync(enrollmentId))
            .ReturnsAsync(enrollment);

        // Act
        Func<Task> act = async () => await _enrollmentService.UpdateProgressAsync(enrollmentId, 50, differentStudentId);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("You can only update your own enrollment progress.");
    }
}
