using CourseLearningPlatform.Api.Requests;
using CourseLearningPlatform.Application.AssignedCourses.Commands;
using CourseLearningPlatform.Application.AssignedCourses.Dtos;
using CourseLearningPlatform.Application.AssignedCourses.Exceptions;
using CourseLearningPlatform.Application.AssignedCourses.Handlers;
using CourseLearningPlatform.Application.AssignedCourses.Queries;
using CourseLearningPlatform.Domain.AssignedCourses;
using Microsoft.AspNetCore.Mvc;

namespace CourseLearningPlatform.Api.Controllers;

[ApiController]
[Route("api/users/{userId:int}/courses")]
public sealed class UserAssignedCoursesController(
    EnrollUserInCourseHandler enrollUserInCourseHandler,
    GetUserAssignedCoursesHandler getUserAssignedCoursesHandler,
    UpdateAssignedCourseStatusHandler updateAssignedCourseStatusHandler) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByUser(
        int userId,
        CancellationToken cancellationToken)
    {
        var assignments = await getUserAssignedCoursesHandler.HandleAsync(
            new GetUserAssignedCoursesQuery(userId),
            cancellationToken);

        return assignments is null ? NotFound() : Ok(assignments);
    }

    [HttpPost("{courseId:int}")]
    public async Task<IActionResult> Enroll(
        int userId,
        int courseId,
        CancellationToken cancellationToken)
    {
        try
        {
            var assignment = await enrollUserInCourseHandler.HandleAsync(
                new EnrollUserInCourseCommand(userId, courseId),
                cancellationToken);

            if (assignment is null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "User or course not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var response = new AssignedCourseDto(
                assignment.UserId,
                assignment.CourseId,
                assignment.Course.Title,
                assignment.Status.ToString(),
                assignment.StartedTimestamp,
                assignment.EndedTimestamp,
                assignment.HoursCompleted);

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
        if (!Enum.TryParse<AssignedCourseStatus>(request.Status, ignoreCase: true, out var newStatus) ||
            !Enum.IsDefined(newStatus))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid course status.",
                Detail = "Status must be Started, InProgress, or Completed.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var assignment = await updateAssignedCourseStatusHandler.HandleAsync(
                new UpdateAssignedCourseStatusCommand(userId, courseId, newStatus),
                cancellationToken);

            if (assignment is null)
            {
                return NotFound();
            }

            return Ok(new
            {
                assignment.UserId,
                assignment.CourseId,
                Status = assignment.Status.ToString(),
                assignment.StartedTimestamp,
                assignment.EndedTimestamp,
                assignment.HoursCompleted
            });
        }
        catch (InvalidAssignedCourseStatusTransitionException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Invalid course status transition.",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }
}
