using Course.Application.DTOs;
using Course.Application.Interfaces;
using Course.Domain;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;

namespace Course.Application.Services;

public class CourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        ICourseRepository courseRepository,
        ILogger<CourseService> logger)
    {
        _courseRepository = courseRepository;
        _logger = logger;
    }

    public async Task<CourseResponse> CreateCourseAsync(CreateCourseRequest request, Guid instructorId)
    {
        var validCategories = new[]
        {
            Category.Programming, Category.Design, Category.Business,
            Category.Marketing, Category.DataScience, Category.DevOps, Category.Other
        };

        if (!validCategories.Contains(request.Category))
        {
            throw new ValidationException($"Invalid category. Valid categories: {string.Join(", ", validCategories)}");
        }

        var course = new CourseEntity
        {
            Title = request.Title,
            Description = request.Description,
            InstructorId = instructorId,
            Price = request.Price,
            Category = request.Category,
            ImageUrl = request.ImageUrl,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _courseRepository.AddAsync(course);

        _logger.LogInformation(
            "Course created: {CourseId} '{Title}' by Instructor {InstructorId}",
            course.Id, course.Title, instructorId);

        return MapToResponse(course);
    }

    public async Task<CourseResponse> GetCourseByIdAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
        {
            throw new NotFoundException($"Course with ID {id} not found.");
        }

        return MapToResponse(course);
    }

    public async Task<IEnumerable<CourseResponse>> GetAllCoursesAsync()
    {
        var courses = await _courseRepository.GetPublishedAsync();
        return courses.Select(MapToResponse);
    }

    public async Task<IEnumerable<CourseResponse>> GetCoursesByInstructorAsync(Guid instructorId)
    {
        var courses = await _courseRepository.GetByInstructorIdAsync(instructorId);
        return courses.Select(MapToResponse);
    }

    public async Task<IEnumerable<CourseResponse>> GetCoursesByCategoryAsync(string category)
    {
        var courses = await _courseRepository.GetByCategoryAsync(category);
        return courses.Select(MapToResponse);
    }

    public async Task<CourseResponse> UpdateCourseAsync(Guid id, UpdateCourseRequest request, Guid userId, string userRole)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
        {
            throw new NotFoundException($"Course with ID {id} not found.");
        }

        if (course.InstructorId != userId && userRole != "Admin")
        {
            throw new ValidationException("You are not authorized to update this course.");
        }

        course.Title = request.Title;
        course.Description = request.Description;
        course.Price = request.Price;
        course.Category = request.Category;
        course.ImageUrl = request.ImageUrl;
        course.IsPublished = request.IsPublished;
        course.UpdatedAt = DateTime.UtcNow;

        await _courseRepository.UpdateAsync(course);

        _logger.LogInformation(
            "Course updated: {CourseId} '{Title}' (Published: {IsPublished})",
            course.Id, course.Title, course.IsPublished);

        return MapToResponse(course);
    }

    public async Task DeleteCourseAsync(Guid id, Guid userId, string userRole)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
        {
            throw new NotFoundException($"Course with ID {id} not found.");
        }

        if (course.InstructorId != userId && userRole != "Admin")
        {
            throw new ValidationException("You are not authorized to delete this course.");
        }

        await _courseRepository.DeleteAsync(course);

        _logger.LogInformation("Course deleted: {CourseId} '{Title}'", id, course.Title);
    }

    private static CourseResponse MapToResponse(CourseEntity course)
    {
        return new CourseResponse
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            InstructorId = course.InstructorId,
            Price = course.Price,
            Category = course.Category,
            ImageUrl = course.ImageUrl,
            IsPublished = course.IsPublished,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };
    }
}
