namespace CourseLearningPlatform.Application.Courses.Commands;

public sealed record UpdateCourseCommand(
    string Title,
    string Description,
    string Instructor,
    decimal Cost,
    decimal Hours
); 