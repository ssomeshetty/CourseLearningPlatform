using CourseLearningPlatform.Application.Users.Commands;
using CourseLearningPlatform.Application.Users.Dtos;
using CourseLearningPlatform.Application.Users.Interfaces;
using CourseLearningPlatform.Application.Users.Queries;
using CourseLearningPlatform.Domain.Users;

namespace CourseLearningPlatform.Application.Users.Handlers;

public sealed class UserProfileHandler(IUserProfileRepository userRepository)
{
    public async Task<UserProfileDTO> CreateAsync(
        UserProfileCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = new UserProfile
        {
            Name = command.Name,
            Email = command.Email,
            EmployeeCode = command.EmployeeCode,
            JoiningDate = command.JoiningDate,
            IsAdmin = false
        };

        await userRepository.AddAsync(user, cancellationToken);
        return ToDto(user);
    }

    public async Task<IReadOnlyList<UserProfileDTO>> GetAllAsync(
        GetAllUserProfilesQuery query,
        CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        return users.Select(ToDto).ToList();
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

        user.Name = command.Name;
        user.Email = command.Email;
        user.EmployeeCode = command.EmployeeCode;
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

        await userRepository.DeleteAsync(id, cancellationToken);
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
