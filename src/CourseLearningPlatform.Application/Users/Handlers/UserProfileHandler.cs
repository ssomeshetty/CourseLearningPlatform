using CourseLearningPlatform.Application.Users.Commands;
using CourseLearningPlatform.Application.Users.Dtos;
using CourseLearningPlatform.Application.Users.Interfaces;
using CourseLearningPlatform.Application.Users.Queries;
using CourseLearningPlatform.Domain.Users;
using CourseLearningPlatform.Application;

namespace CourseLearningPlatform.Application.Users.Handlers;

public sealed class UserProfileHandler(
    IUserProfileRepository userRepository,
    IPasswordHashService passwordHashService)
{
    public async Task<UserProfileDTO> CreateAsync(
        UserProfileCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = new UserProfile
        {
            Name = command.Name.Trim(),
            Email = command.Email.Trim().ToLowerInvariant(),
            EmployeeCode = command.EmployeeCode.Trim(),
            JoiningDate = command.JoiningDate,
            IsAdmin = false,
            PasswordHash = passwordHashService.Hash(command.Password)
        };

        await userRepository.AddAsync(user, cancellationToken);
        return ToDto(user);
    }

    public async Task<PagedResult<UserProfileDTO>> GetAllAsync(
        GetAllUserProfilesQuery query,
        CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetPageAsync(query.Page, query.PageSize, cancellationToken);
        return new PagedResult<UserProfileDTO>(
            users.Items.Select(ToDto).ToList(), users.Page, users.PageSize, users.TotalCount);
    }

    public async Task<UserProfileDTO?> GetByIdAsync(
        GetUserProfileByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(query.Id, cancellationToken);
        return user is null ? null : ToDto(user);
    }

    public async Task<UserProfileDTO?> UpdateAsync(
        int id,
        UserProfileUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.Name = command.Name.Trim();
        user.Email = command.Email.Trim().ToLowerInvariant();
        user.EmployeeCode = command.EmployeeCode.Trim();
        user.JoiningDate = command.JoiningDate;

        await userRepository.UpdateAsync(user, cancellationToken);
        return ToDto(user);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        await userRepository.DeleteAsync(user, cancellationToken);
        return true;
    }

    private static UserProfileDTO ToDto(UserProfile user) =>
        new(
            user.Id,
            user.Name,
            user.Email,
            user.EmployeeCode,
            user.JoiningDate,
            user.IsAdmin);
}
