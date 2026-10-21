using CourseLearningPlatform.Application.AssignedCourses.Interfaces;
using CourseLearningPlatform.Application.Users.Dtos;
using CourseLearningPlatform.Application.Users.Interfaces;
using CourseLearningPlatform.Application.Users.Queries;
using CourseLearningPlatform.Domain.AssignedCourses;

namespace CourseLearningPlatform.Application.Users.Handlers;

public sealed class GetUserCourseAnalyticsHandler(
    IUserProfileRepository userProfileRepository,
    IAssignedCourseRepository assignedCourseRepository)
{
    public async Task<UserCourseAnalyticsDto?> HandleAsync(
        GetUserCourseAnalyticsQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await userProfileRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var assignments = await assignedCourseRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        var completedCourseHours = assignments
            .Where(assignment => assignment.Status == AssignedCourseStatus.Completed)
            .Sum(assignment => assignment.Course.Hours);

        // In-progress learning time is an estimate: count half of the course duration.
        var inProgressEstimatedHours = assignments
            .Where(assignment => assignment.Status == AssignedCourseStatus.InProgress)
            .Sum(assignment => assignment.Course.Hours * 0.5m);

        completedCourseHours = decimal.Round(completedCourseHours, 2, MidpointRounding.AwayFromZero);
        inProgressEstimatedHours = decimal.Round(inProgressEstimatedHours, 2, MidpointRounding.AwayFromZero);

        return new UserCourseAnalyticsDto(
            query.UserId,
            assignments.Count,
            assignments.Count(assignment => assignment.Status == AssignedCourseStatus.Started),
            assignments.Count(assignment => assignment.Status == AssignedCourseStatus.InProgress),
            assignments.Count(assignment => assignment.Status == AssignedCourseStatus.Completed),
            completedCourseHours,
            inProgressEstimatedHours,
            completedCourseHours + inProgressEstimatedHours);
    }
}
