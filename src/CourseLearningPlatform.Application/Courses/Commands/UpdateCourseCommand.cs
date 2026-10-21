using System.ComponentModel.DataAnnotations;

namespace CourseLearningPlatform.Application.Courses.Commands;

public sealed record UpdateCourseCommand(
    [param: Required, StringLength(160, MinimumLength = 2)] string Title,
    [param: Required, StringLength(4000, MinimumLength = 2)] string Description,
    [param: Required, StringLength(120, MinimumLength = 2)] string Instructor,
    [param: Required, Url]
    string ImageUrl,
    [param: Range(0, 999999999)] decimal Cost,
    [param: Range(0, 100000)] decimal Hours
);
