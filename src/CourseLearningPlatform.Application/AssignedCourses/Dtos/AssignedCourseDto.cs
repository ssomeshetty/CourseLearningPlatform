namespace CourseLearningPlatform.Application.AssignedCourses.Dtos;

public sealed record AssignedCourseDto(
    int UserId,
    int CourseId,
    string CourseTitle,
    string Status,
    DateTime StartedTimestamp,
    DateTime? EndedTimestamp,
    decimal HoursCompleted);
