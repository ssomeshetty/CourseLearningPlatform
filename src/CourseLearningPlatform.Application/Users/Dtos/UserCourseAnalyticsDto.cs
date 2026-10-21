namespace CourseLearningPlatform.Application.Users.Dtos;

public sealed record UserCourseAnalyticsDto(
    int UserId,
    int TotalEnrolledCourses,
    int NotStartedCourses,
    int InProgressCourses,
    int CompletedCourses,
    decimal CompletedCourseHours,
    decimal InProgressEstimatedHours,
    decimal TotalEstimatedLearningHours);
