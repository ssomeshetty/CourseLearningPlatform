namespace CourseLearningPlatform.Application.Courses.Commands;

public sealed record CreateCourseCommand(
    string Title,
    string Description,
    string Instructor,
    decimal Cost,
    decimal Hours
);

