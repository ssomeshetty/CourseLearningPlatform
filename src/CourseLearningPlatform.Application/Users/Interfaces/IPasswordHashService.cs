namespace CourseLearningPlatform.Application.Users.Interfaces;

public interface IPasswordHashService
{
    string Hash(string password);
    bool Verify(string passwordHash, string password);
}
