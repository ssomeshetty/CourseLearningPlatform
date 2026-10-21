using System.ComponentModel.DataAnnotations;

namespace CourseLearningPlatform.Application.Users.Commands;

public sealed record UserProfileCreateCommand(
    [param: Required, StringLength(120, MinimumLength = 2)] string Name,
    [param: Required, EmailAddress, StringLength(256)] string Email,
    [param: Required, StringLength(50)] string EmployeeCode,
    DateOnly JoiningDate,
    [param: Required, RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{12,128}$",
        ErrorMessage = "Password must be 12 to 128 characters and include a letter, a number, and a symbol.")] string Password);

public sealed record UserProfileUpdateCommand(
    [param: Required, StringLength(120, MinimumLength = 2)] string Name,
    [param: Required, EmailAddress, StringLength(256)] string Email,
    [param: Required, StringLength(50)] string EmployeeCode,
    DateOnly JoiningDate);
