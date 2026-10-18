using CourseLearningPlatform.Application.Users.Interfaces;
using CourseLearningPlatform.Domain.Users;
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

    public async Task<IReadOnlyList<UserProfile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.UserProfiles.AsNoTracking().ToListAsync(cancellationToken);
        
    }

    public async Task UpdateAsync(UserProfile user, CancellationToken cancellationToken = default)
    {
        context.UserProfiles.Update(user);
        await context.SaveChangesAsync(cancellationToken);

    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await context.UserProfiles.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        if (user is null)
        {
            return;

        }

        context.UserProfiles.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

    }
}


