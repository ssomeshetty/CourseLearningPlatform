using CourseLearningPlatform.Application.AssignedCourses.Commands;
using CourseLearningPlatform.Application.AssignedCourses.Interfaces;
using CourseLearningPlatform.Application.AssignedCourses.Exceptions;
using CourseLearningPlatform.Application.Courses.Interfaces;
using CourseLearningPlatform.Application.Users.Interfaces;
using CourseLearningPlatform.Domain.AssignedCourses;

namespace CourseLearningPlatform.Application.AssignedCourses.Handlers;

public sealed class EnrollUserInCourseHandler(
    IAssignedCourseRepository assignedCourseRepository,
    IUserProfileRepository userProfileRepository,
    ICourseRepository courseRepository)
{
    public async Task<AssignedCourse?> HandleAsync(
        EnrollUserInCourseCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userProfileRepository.GetByIdAsync(command.UserId, cancellationToken);
        var course = await courseRepository.GetByIdAsync(command.CourseId, cancellationToken);

        if (user is null || course is null)
        {
            return null;
        }

        var existingAssignment = await assignedCourseRepository.GetByIdAsync(
            command.UserId,
            command.CourseId,
            cancellationToken);

        if (existingAssignment is not null)
        {
            throw new DuplicateEnrollmentException();
        }

        var assignedCourse = new AssignedCourse
        {
            UserId = command.UserId,
            CourseId = command.CourseId,
            User = user,
            Course = course
        };

        return await assignedCourseRepository.AddAsync(assignedCourse, cancellationToken);
    }
}


