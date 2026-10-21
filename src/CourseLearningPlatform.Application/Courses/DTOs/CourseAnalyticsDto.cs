namespace CourseLearningPlatform.Application.Courses.DTOs;

public sealed record CourseAnalyticsDto(
    int CourseId,
    string CourseTitle,
    int TotalEnrolledLearners,
    int StartedCount,
    int InProgressCount,
    int CompletedCount);
