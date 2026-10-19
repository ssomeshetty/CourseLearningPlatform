namespace CourseLearningPlatform.Application.AssignedCourses.Exceptions;

public sealed class DuplicateEnrollmentException()
    : Exception("The user is already enrolled in this course.")
{
}
