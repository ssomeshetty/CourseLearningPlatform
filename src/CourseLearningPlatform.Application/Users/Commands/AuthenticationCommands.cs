namespace CourseLearningPlatform.Application.Users.Commands;

public sealed record SignUpCommand(
    string Name,
    string Email,
    string EmployeeCode,
    DateOnly? JoiningDate,
    string Password);

public sealed record SignInCommand(string Email, string Password);
