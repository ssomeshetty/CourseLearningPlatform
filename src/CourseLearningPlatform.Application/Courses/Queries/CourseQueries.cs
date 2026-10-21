namespace CourseLearningPlatform.Application.Courses.Queries;

public sealed record GetCourseByIdQuery(int Id);

public sealed record GetCoursesQuery(int Page = 1, int PageSize = 10);
