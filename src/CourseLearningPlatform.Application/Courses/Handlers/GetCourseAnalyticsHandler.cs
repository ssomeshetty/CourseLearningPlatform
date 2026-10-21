using CourseLearningPlatform.Application.AssignedCourses.Interfaces;
using CourseLearningPlatform.Application.Courses.DTOs;
using CourseLearningPlatform.Application.Courses.Interfaces;
using CourseLearningPlatform.Application.Courses.Queries;
using CourseLearningPlatform.Domain.AssignedCourses;

namespace CourseLearningPlatform.Application.Courses.Handlers;

public sealed class GetCourseAnalyticsHandler(
    ICourseRepository courseRepository,
    IAssignedCourseRepository assignedCourseRepository)
{
    public async Task<CourseAnalyticsDto?> HandleAsync(
        GetCourseAnalyticsQuery query,
        CancellationToken cancellationToken = default)
    {
        var course = await courseRepository.GetByIdAsync(query.CourseId, cancellationToken);
        if (course is null)
        {
            return null;
        }

        var assignments = await assignedCourseRepository.GetByCourseIdAsync(
            query.CourseId,
            cancellationToken);

        return new CourseAnalyticsDto(
            course.Id,
            course.Title,
            assignments.Count,
            assignments.Count(assignment => assignment.Status == AssignedCourseStatus.Started),
            assignments.Count(assignment => assignment.Status == AssignedCourseStatus.InProgress),
            assignments.Count(assignment => assignment.Status == AssignedCourseStatus.Completed));
    }
}
