using CourseLearningPlatform.Application.Users.Commands;
using CourseLearningPlatform.Application.Users.Dtos;
using CourseLearningPlatform.Application.Users.Exceptions;
using CourseLearningPlatform.Application.Users.Interfaces;
using CourseLearningPlatform.Domain.Users;

namespace CourseLearningPlatform.Application.Users.Handlers;

public sealed class AuthenticationHandler(
    IUserProfileRepository users,
    IPasswordHashService passwordHashService,
    IAccessTokenService accessTokenService)
{
    public async Task<AuthenticationDto> SignUpAsync(
        SignUpCommand command, CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var employeeCode = command.EmployeeCode.Trim();

        if (await users.GetByEmailAsync(email, cancellationToken) is not null ||
            await users.GetByEmployeeCodeAsync(employeeCode, cancellationToken) is not null)
        {
            throw new DuplicateAccountException("An account with this email or employee code already exists.");
        }

        var user = new UserProfile
        {
            Name = command.Name.Trim(),
            Email = email,
            EmployeeCode = employeeCode,
            JoiningDate = command.JoiningDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            IsAdmin = false,
            PasswordHash = passwordHashService.Hash(command.Password)
        };

        await users.AddAsync(user, cancellationToken);
        return CreateResponse(user);
    }

    public async Task<AuthenticationDto?> SignInAsync(
        SignInCommand command, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByEmailAsync(command.Email, cancellationToken);
        if (user?.PasswordHash is null || !passwordHashService.Verify(user.PasswordHash, command.Password))
        {
            return null;
        }

        return CreateResponse(user);
    }

    private AuthenticationDto CreateResponse(UserProfile user)
    {
        var token = accessTokenService.Create(user);
        return new AuthenticationDto(user.Id, user.Name, user.Email, token.Role, token.Value, token.ExpiresAtUtc);
    }
}
