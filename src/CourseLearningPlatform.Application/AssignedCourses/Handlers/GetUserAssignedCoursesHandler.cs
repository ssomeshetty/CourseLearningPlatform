using CourseLearningPlatform.Application.AssignedCourses.Dtos;
using CourseLearningPlatform.Application.AssignedCourses.Interfaces;
using CourseLearningPlatform.Application.AssignedCourses.Queries;
using CourseLearningPlatform.Application.Users.Interfaces;

namespace CourseLearningPlatform.Application.AssignedCourses.Handlers;

public sealed class GetUserAssignedCoursesHandler(
    IAssignedCourseRepository assignedCourseRepository,
    IUserProfileRepository userProfileRepository)
{
    public async Task<IReadOnlyList<AssignedCourseDto>?> HandleAsync(
        GetUserAssignedCoursesQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await userProfileRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var assignments = await assignedCourseRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        return assignments
            .Select(assignment => new AssignedCourseDto(
                assignment.UserId,
                assignment.CourseId,
                assignment.Course.Title,
                assignment.Status.ToString(),
                assignment.CompletedAtUtc))
            .ToList();
    }
}
