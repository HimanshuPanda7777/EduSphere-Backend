using System.Security.Claims;
using Core.Application.DTOs;
using Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Exceptions;

namespace Core.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly EnrollmentService _enrollmentService;

    public EnrollmentsController(EnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    /// <summary>
    /// Enroll the currently authenticated student in a course.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<EnrollmentResponse>> Enroll(
        [FromBody] EnrollRequest request)
    {
        try
        {
            var studentId = GetCurrentUserId();
            var enrollment = await _enrollmentService.EnrollAsync(request, studentId);
            return CreatedAtAction(
                nameof(GetEnrollmentById),
                new { id = enrollment.Id },
                enrollment);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific enrollment by ID.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<EnrollmentResponse>> GetEnrollmentById(Guid id)
    {
        try
        {
            var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
            return Ok(enrollment);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all enrollments for the currently authenticated student.
    /// </summary>
    [HttpGet("my-enrollments")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<EnrollmentResponse>>> GetMyEnrollments()
    {
        var studentId = GetCurrentUserId();
        var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(studentId);
        return Ok(enrollments);
    }

    /// <summary>
    /// Get all enrollments for a specific course. Instructors and Admins only.
    /// </summary>
    [HttpGet("course/{courseId}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<ActionResult<IEnumerable<EnrollmentResponse>>> GetEnrollmentsByCourse(
        Guid courseId)
    {
        var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(courseId);
        return Ok(enrollments);
    }

    /// <summary>
    /// Update progress percentage for an enrollment.
    /// </summary>
    [HttpPut("{id}/progress")]
    [Authorize]
    public async Task<ActionResult<EnrollmentResponse>> UpdateProgress(
        Guid id, [FromBody] UpdateProgressRequest request)
    {
        try
        {
            var studentId = GetCurrentUserId();
            var enrollment = await _enrollmentService
                .UpdateProgressAsync(id, request.ProgressPercentage, studentId);
            return Ok(enrollment);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Unenroll from a course. Only the enrolled student or Admin can unenroll.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Unenroll(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            await _enrollmentService.UnenrollAsync(id, userId, userRole);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
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

/// <summary>
/// Simple request DTO for updating progress percentage.
/// </summary>
public class UpdateProgressRequest
{
    public int ProgressPercentage { get; set; }
}
