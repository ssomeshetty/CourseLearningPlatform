using CourseLearningPlatform.Domain.Users;
using CourseLearningPlatform.Application;

namespace CourseLearningPlatform.Application.Users.Interfaces;

public interface IUserProfileRepository
{
    Task<UserProfile> AddAsync(UserProfile user, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByEmployeeCodeAsync(string employeeCode, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<PagedResult<UserProfile>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserProfile user, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserProfile user, CancellationToken cancellationToken = default);

}
