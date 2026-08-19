using System.Security.Claims;
using Course.Application.DTOs;
using Course.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Course.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly CourseService _courseService;

    public CoursesController(CourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>
    /// Get all published courses. Optionally filter by category.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseResponse>>> GetAllCourses(
        [FromQuery] string? category = null)
    {
        IEnumerable<CourseResponse> courses;

        if (!string.IsNullOrEmpty(category))
        {
            courses = await _courseService.GetCoursesByCategoryAsync(category);
        }
        else
        {
            courses = await _courseService.GetAllCoursesAsync();
        }

        return Ok(courses);
    }

    /// <summary>
    /// Get a specific course by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CourseResponse>> GetCourseById(Guid id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        return Ok(course);
    }

    /// <summary>
    /// Get all courses by the currently authenticated instructor.
    /// </summary>
    [HttpGet("my-courses")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<ActionResult<IEnumerable<CourseResponse>>> GetMyCourses()
    {
        var instructorId = GetCurrentUserId();
        var courses = await _courseService.GetCoursesByInstructorAsync(instructorId);
        return Ok(courses);
    }

    /// <summary>
    /// Create a new course. Only Instructors and Admins can create courses.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<ActionResult<CourseResponse>> CreateCourse(
        [FromBody] CreateCourseRequest request)
    {
        var instructorId = GetCurrentUserId();
        var course = await _courseService.CreateCourseAsync(request, instructorId);
        return CreatedAtAction(
            nameof(GetCourseById),
            new { id = course.Id },
            course);
    }

    /// <summary>
    /// Update an existing course. Only the course owner or Admin can update.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<CourseResponse>> UpdateCourse(
        Guid id, [FromBody] UpdateCourseRequest request)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();
        var course = await _courseService.UpdateCourseAsync(id, request, userId, userRole);
        return Ok(course);
    }

    /// <summary>
    /// Delete a course. Only the course owner or Admin can delete.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteCourse(Guid id)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();
        await _courseService.DeleteCourseAsync(id, userId, userRole);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token.");
        }
        return userId;
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }
}
