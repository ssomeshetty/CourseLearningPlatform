namespace CourseLearningPlatform.Application.Users.Dtos;

public sealed record AuthenticationDto(
    int Id,
    string Name,
    string Email,
    string Role,
    string AccessToken,
    DateTime ExpiresAtUtc);
