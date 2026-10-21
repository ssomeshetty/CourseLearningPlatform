using CourseLearningPlatform.Api.Requests;
using CourseLearningPlatform.Application.AssignedCourses.Commands;
using CourseLearningPlatform.Application.AssignedCourses.Dtos;
using CourseLearningPlatform.Application.AssignedCourses.Exceptions;
using CourseLearningPlatform.Application.AssignedCourses.Handlers;
using CourseLearningPlatform.Application.AssignedCourses.Interfaces;
using CourseLearningPlatform.Application.AssignedCourses.Queries;
using CourseLearningPlatform.Domain.AssignedCourses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using CourseLearningPlatform.Api.Services;

namespace CourseLearningPlatform.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId:int}/courses")]
public sealed class UserAssignedCoursesController(
    EnrollUserInCourseHandler enrollUserInCourseHandler,
    GetUserAssignedCoursesHandler getUserAssignedCoursesHandler,
    UpdateAssignedCourseStatusHandler updateAssignedCourseStatusHandler,
    IDistributedCache cache,
    IAssignedCourseRepository assignedCourseRepository,
    EmailService emailService) : ControllerBase
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
    };

    [HttpGet]
    public async Task<IActionResult> GetByUser(int userId, CancellationToken cancellationToken)
    {
        if (!CanAccessUser(userId)) return Forbid();
        var cacheKey = $"user-courses:{userId}";
        var json = await cache.GetStringAsync(cacheKey, cancellationToken);
        var assignments = json is null
            ? null
            : JsonSerializer.Deserialize<IReadOnlyList<AssignedCourseDto>>(json);
        if (json is null)
        {
            assignments = await getUserAssignedCoursesHandler.HandleAsync(
                new GetUserAssignedCoursesQuery(userId), cancellationToken);
            if (assignments is not null)
            {
                await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(assignments), CacheOptions, cancellationToken);
            }
        }
        return assignments is null
            ? NotFound(new ProblemDetails { Status = 404, Title = "User not found.", Detail = "We couldn't find this user's course list." })
            : Ok(assignments);
    }

    [HttpPost("{courseId:int}")]
    public async Task<IActionResult> Enroll(int userId, int courseId, CancellationToken cancellationToken)
    {
        if (!CanAccessUser(userId)) return Forbid();
        try
        {
            var assignment = await enrollUserInCourseHandler.HandleAsync(
                new EnrollUserInCourseCommand(userId, courseId), cancellationToken);

            if (assignment is null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "User or course not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            await InvalidateUserCourseDataAsync(userId, courseId, cancellationToken);

            var response = new AssignedCourseDto(
                assignment.UserId,
                assignment.CourseId,
                assignment.Course.Title,
                assignment.Status.ToString(),
                assignment.CompletedAtUtc);

            return CreatedAtAction(nameof(GetByUser), new { userId }, response);
        }
        catch (DuplicateEnrollmentException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Already enrolled.",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpPut("{courseId:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int userId,
        int courseId,
        [FromBody] UpdateAssignedCourseStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!CanAccessUser(userId)) return Forbid();
        if (!Enum.TryParse<AssignedCourseStatus>(request.Status, true, out var newStatus) ||
            !Enum.IsDefined(newStatus))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid course status.",
                Detail = "Status must be Started, InProgress, or Completed.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var previousAssignment = await assignedCourseRepository.GetByIdAsync(userId, courseId, cancellationToken);
        var newlyCompleted = newStatus == AssignedCourseStatus.Completed &&
            previousAssignment?.Status != AssignedCourseStatus.Completed;

        var assignment = await updateAssignedCourseStatusHandler.HandleAsync(
            new UpdateAssignedCourseStatusCommand(userId, courseId, newStatus), cancellationToken);

        if (assignment is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Enrollment not found.",
                Detail = "This user is not enrolled in that course."
            });
        }

        await InvalidateUserCourseDataAsync(userId, courseId, cancellationToken);

        if (newlyCompleted)
        {
            if (previousAssignment is not null)
            {
                await emailService.SendAsync(
                    previousAssignment.User.Email,
                    "Course completed",
                    $"Congratulations, {previousAssignment.User.Name}! You have completed " +
                    $"{previousAssignment.Course.Title}.");
            }
        }

        return Ok(new
        {
            assignment.UserId,
            assignment.CourseId,
            Status = assignment.Status.ToString(),
            assignment.CompletedAtUtc
        });
    }

    private bool CanAccessUser(int userId) => User.IsInRole("Admin") ||
        (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var authenticatedUserId) &&
         authenticatedUserId == userId);

    private async Task InvalidateUserCourseDataAsync(
        int userId,
        int courseId,
        CancellationToken cancellationToken)
    {
        await cache.RemoveAsync($"user-courses:{userId}", cancellationToken);
        await cache.RemoveAsync($"user-analytics:{userId}", cancellationToken);
        await cache.RemoveAsync($"course-analytics:{courseId}", cancellationToken);
    }
}
