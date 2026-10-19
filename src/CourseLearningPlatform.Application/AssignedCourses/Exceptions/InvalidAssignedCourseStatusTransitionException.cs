namespace CourseLearningPlatform.Application.AssignedCourses.Exceptions;

public sealed class InvalidAssignedCourseStatusTransitionException(string message)
    : Exception(message)
{
}
