namespace CourseLearningPlatform.Application.AssignedCourses.Commands;

public sealed record EnrollUserInCourseCommand(
    int UserId,
    int CourseId);
