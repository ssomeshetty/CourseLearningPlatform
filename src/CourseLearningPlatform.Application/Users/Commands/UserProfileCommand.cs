namespace CourseLearningPlatform.Application.Users.Commands;

public sealed record UserProfileCreateCommand
(
    string Name,
    string Email,
    string EmployeeCode,
    DateOnly JoiningDate
    
);

public sealed record UserProfileUpdateCommand
(
    string Name,
    string Email,
    string EmployeeCode,
    DateOnly JoiningDate
);




