namespace CourseLearningPlatform.Application.Users.Queries;

public sealed record GetUserProfileByIdQuery(int Id);

public sealed record GetAllUserProfilesQuery(int Page = 1, int PageSize = 10);

