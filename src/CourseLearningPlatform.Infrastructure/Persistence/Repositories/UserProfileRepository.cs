using CourseLearningPlatform.Application.Users.Interfaces;
using CourseLearningPlatform.Domain.Users;
using CourseLearningPlatform.Application;
using Microsoft.EntityFrameworkCore;
using CourseLearningPlatform.Infrastructure.Persistence;

namespace CourseLearningPlatform.Infrastructure.Persistence.Repositories;

public sealed class UserProfileRepository(AppDbContext context) : IUserProfileRepository
{
    public async Task<UserProfile> AddAsync(UserProfile user, CancellationToken cancellationToken = default)
    {
        await context.UserProfiles.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public Task<UserProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return context.UserProfiles.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<UserProfile?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return context.UserProfiles.FirstOrDefaultAsync(
            user => user.Email == normalizedEmail, cancellationToken);
    }

    public Task<UserProfile?> GetByEmployeeCodeAsync(string employeeCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = employeeCode.Trim();
        return context.UserProfiles.FirstOrDefaultAsync(
            user => user.EmployeeCode == normalizedCode, cancellationToken);
    }

    public async Task<PagedResult<UserProfile>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.UserProfiles.AsNoTracking().OrderBy(user => user.Id);
        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<UserProfile>(users, page, pageSize, totalCount);
        
    }

    public async Task UpdateAsync(UserProfile user, CancellationToken cancellationToken = default)
    {
        context.UserProfiles.Update(user);
        await context.SaveChangesAsync(cancellationToken);

    }

    public async Task DeleteAsync(UserProfile user, CancellationToken cancellationToken = default)
    {
        context.UserProfiles.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

    }
}


