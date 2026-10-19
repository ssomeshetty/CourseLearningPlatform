using CourseLearningPlatform.Domain.AssignedCourses;

namespace CourseLearningPlatform.Application.AssignedCourses.Commands;

public sealed record UpdateAssignedCourseStatusCommand(
    int UserId,
    int CourseId,
    AssignedCourseStatus NewStatus);
