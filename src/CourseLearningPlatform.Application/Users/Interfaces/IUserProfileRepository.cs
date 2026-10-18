using CourseLearningPlatform.Domain.Users;

namespace CourseLearningPlatform.Application.Users.Interfaces;

public interface IUserProfileRepository
{
    Task<UserProfile> AddAsync(UserProfile user, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserProfile>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(UserProfile user, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

}