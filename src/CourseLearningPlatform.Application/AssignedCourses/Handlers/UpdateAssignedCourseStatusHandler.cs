using CourseLearningPlatform.Application.AssignedCourses.Commands;
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

        assignment.Status = command.NewStatus;
        if (command.NewStatus == AssignedCourseStatus.Completed)
        {
            assignment.CompletedAtUtc ??= DateTime.UtcNow;
        }
        else
        {
            assignment.CompletedAtUtc = null;
        }

        await assignedCourseRepository.UpdateAsync(assignment, cancellationToken);
        return assignment;
    }
}
