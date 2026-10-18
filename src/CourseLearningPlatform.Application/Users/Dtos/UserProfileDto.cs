namespace CourseLearningPlatform.Application.Users.Dtos;

public sealed record UserProfileDTO(
    int Id,
    string Name,
    string Email,
    string EmployeeCode,
    DateOnly JoiningDate,
    bool IsAdmin
);