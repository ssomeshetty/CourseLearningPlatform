using CourseLearningPlatform.Domain.Users;

namespace CourseLearningPlatform.Application.Users.Interfaces;

public interface IAccessTokenService
{
    AccessToken Create(UserProfile user);
}

public sealed record AccessToken(string Value, DateTime ExpiresAtUtc, string Role);
