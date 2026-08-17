using System.Security.Claims;
using Core.Application.DTOs;
using Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Exceptions;

namespace Core.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly ProgressService _progressService;

    public ProgressController(ProgressService progressService)
    {
        _progressService = progressService;
    }

    /// <summary>
    /// Record lesson completion for an enrollment.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProgressResponse>> RecordProgress(
        [FromBody] RecordProgressRequest request)
    {
        try
        {
            var studentId = GetCurrentUserId();
            var progress = await _progressService
                .RecordProgressAsync(request, studentId);
            return Ok(progress);
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
    /// Get all progress records for a specific enrollment.
    /// </summary>
    [HttpGet("{enrollmentId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ProgressResponse>>> GetProgress(
        Guid enrollmentId)
    {
        try
        {
            var studentId = GetCurrentUserId();
            var progressItems = await _progressService
                .GetProgressByEnrollmentAsync(enrollmentId, studentId);
            return Ok(progressItems);
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
}
