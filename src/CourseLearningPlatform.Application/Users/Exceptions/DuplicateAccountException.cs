namespace CourseLearningPlatform.Application.Users.Exceptions;

public sealed class DuplicateAccountException(string message) : Exception(message);
