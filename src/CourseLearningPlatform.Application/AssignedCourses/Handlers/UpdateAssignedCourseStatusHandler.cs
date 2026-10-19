using CourseLearningPlatform.Application.AssignedCourses.Commands;
using CourseLearningPlatform.Application.AssignedCourses.Exceptions;
using CourseLearningPlatform.Application.AssignedCourses.Interfaces;
using CourseLearningPlatform.Domain.AssignedCourses;

namespace CourseLearningPlatform.Application.AssignedCourses.Handlers;

public sealed class UpdateAssignedCourseStatusHandler(
    IAssignedCourseRepository assignedCourseRepository)
{
    public async Task<AssignedCourse?> HandleAsync(
        UpdateAssignedCourseStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var assignment = await assignedCourseRepository.GetByIdAsync(
            command.UserId,
            command.CourseId,
            cancellationToken);

        if (assignment is null)
        {
            return null;
        }

        var allowedNextStatus = assignment.Status switch
        {
            AssignedCourseStatus.Started => AssignedCourseStatus.InProgress,
            AssignedCourseStatus.InProgress => AssignedCourseStatus.Completed,
            _ => (AssignedCourseStatus?)null
        };

        if (allowedNextStatus != command.NewStatus)
        {
            throw new InvalidAssignedCourseStatusTransitionException(
                $"Cannot change course status from {assignment.Status} to {command.NewStatus}. " +
                "Allowed transitions are Started to InProgress, then InProgress to Completed.");
        }

        assignment.Status = command.NewStatus;
        if (command.NewStatus == AssignedCourseStatus.Completed)
        {
            assignment.EndedTimestamp = DateTime.UtcNow;
        }

        await assignedCourseRepository.UpdateAsync(assignment, cancellationToken);
        return assignment;
    }
}
