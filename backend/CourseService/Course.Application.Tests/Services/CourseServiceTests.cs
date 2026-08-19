using Course.Application.DTOs;
using Course.Application.Interfaces;
using Course.Application.Services;
using Course.Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Exceptions;

namespace Course.Application.Tests.Services;

public class CourseServiceTests
{
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<ILogger<CourseService>> _loggerMock;
    private readonly CourseService _courseService;

    public CourseServiceTests()
    {
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _loggerMock = new Mock<ILogger<CourseService>>();

        _courseService = new CourseService(
            _courseRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateCourseAsync_ShouldThrowValidationException_WhenCategoryIsInvalid()
    {
        // Arrange
        var request = new CreateCourseRequest
        {
            Title = "Test Course",
            Description = "Test Description",
            Price = 100,
            Category = "InvalidCategory",
            ImageUrl = "http://test.com/img.jpg"
        };
        var instructorId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _courseService.CreateCourseAsync(request, instructorId);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Invalid category. Valid categories: Programming, Design, Business, Marketing, Data Science, DevOps, Other");
    }

    [Fact]
    public async Task CreateCourseAsync_ShouldReturnCourseResponse_WhenSuccessful()
    {
        // Arrange
        var request = new CreateCourseRequest
        {
            Title = "Test Course",
            Description = "Test Description",
            Price = 100,
            Category = Category.Programming,
            ImageUrl = "http://test.com/img.jpg"
        };
        var instructorId = Guid.NewGuid();

        // Act
        var response = await _courseService.CreateCourseAsync(request, instructorId);

        // Assert
        response.Should().NotBeNull();
        response.Title.Should().Be(request.Title);
        response.Category.Should().Be(request.Category);
        response.InstructorId.Should().Be(instructorId);
        response.IsPublished.Should().BeFalse();
        _courseRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<CourseEntity>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCourseAsync_ShouldThrowNotFoundException_WhenCourseDoesNotExist()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var request = new UpdateCourseRequest();
        _courseRepositoryMock.Setup(repo => repo.GetByIdAsync(courseId))
            .ReturnsAsync((CourseEntity?)null);

        // Act
        Func<Task> act = async () => await _courseService.UpdateCourseAsync(courseId, request, Guid.NewGuid(), "Instructor");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Course with ID {courseId} not found.");
    }

    [Fact]
    public async Task UpdateCourseAsync_ShouldThrowValidationException_WhenUserIsNotOwnerOrAdmin()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var instructorId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        
        var existingCourse = new CourseEntity { Id = courseId, InstructorId = instructorId };
        var request = new UpdateCourseRequest();
        
        _courseRepositoryMock.Setup(repo => repo.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);

        // Act
        Func<Task> act = async () => await _courseService.UpdateCourseAsync(courseId, request, differentUserId, "Instructor");

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("You are not authorized to update this course.");
    }

    [Fact]
    public async Task UpdateCourseAsync_ShouldUpdateCourse_WhenUserIsOwner()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var instructorId = Guid.NewGuid();
        
        var existingCourse = new CourseEntity { Id = courseId, InstructorId = instructorId, Title = "Old Title" };
        var request = new UpdateCourseRequest { Title = "New Title", Category = Category.Programming, Price = 50 };
        
        _courseRepositoryMock.Setup(repo => repo.GetByIdAsync(courseId))
            .ReturnsAsync(existingCourse);

        // Act
        var response = await _courseService.UpdateCourseAsync(courseId, request, instructorId, "Instructor");

        // Assert
        response.Should().NotBeNull();
        response.Title.Should().Be("New Title");
        response.Price.Should().Be(50);
        _courseRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<CourseEntity>()), Times.Once);
    }
}
