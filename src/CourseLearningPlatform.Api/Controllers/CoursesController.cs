using CourseLearningPlatform.Application.Courses.Commands;
using CourseLearningPlatform.Application.Courses.Handlers;
using CourseLearningPlatform.Application.Courses.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using CourseLearningPlatform.Application;
using CourseLearningPlatform.Domain.Courses;
using CourseLearningPlatform.Application.Courses.DTOs;
using System.Text.Json;

namespace CourseLearningPlatform.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/courses")]
public class CoursesController(
    CreateCourseHandler createCourseHandler,
    UpdateCourseHandler updateCourseHandler,
    DeleteCourseHandler deleteCourseHandler,
    CourseQueryHandler courseQueryHandler,
    GetCourseAnalyticsHandler getCourseAnalyticsHandler,
    IDistributedCache cache) : ControllerBase
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
    };

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateCourseCommand command,
        CancellationToken cancellationToken = default)
    {
        var course = await createCourseHandler.HandleAsync(command, cancellationToken);
        await cache.SetStringAsync("courses:version", Guid.NewGuid().ToString("N"), CacheOptions, cancellationToken);
        return Created($"/api/courses/{course.Id}", course);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidPage(page, pageSize))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "The page number is invalid.",
                Detail = "Page must be 1 or greater, and page size must be between 1 and 50."
            });
        }

        var version = await cache.GetStringAsync("courses:version", cancellationToken) ?? "0";
        var cacheKey = $"courses:{version}:{page}:{pageSize}";
        var json = await cache.GetStringAsync(cacheKey, cancellationToken);
        var courses = json is null ? null : JsonSerializer.Deserialize<PagedResult<Course>>(json);
        if (courses is null)
        {
            courses = await courseQueryHandler.GetAllAsync(new GetCoursesQuery(page, pageSize), cancellationToken);
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(courses), CacheOptions, cancellationToken);
        }

        return Ok(courses);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var cacheKey = $"course:{id}";
        var json = await cache.GetStringAsync(cacheKey, cancellationToken);
        var course = json is null ? null : JsonSerializer.Deserialize<Course>(json);
        if (json is null)
        {
            course = await courseQueryHandler.GetByIdAsync(new GetCourseByIdQuery(id), cancellationToken);
            if (course is not null)
            {
                await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(course), CacheOptions, cancellationToken);
            }
        }

        return course is null
            ? NotFound(new ProblemDetails { Status = 404, Title = "Course not found.", Detail = "This course may have been removed." })
            : Ok(course);
    }

    [HttpGet("{id:int}/analytics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAnalytics(int id, CancellationToken cancellationToken)
    {
        var cacheKey = $"course-analytics:{id}";
        var json = await cache.GetStringAsync(cacheKey, cancellationToken);
        var analytics = json is null ? null : JsonSerializer.Deserialize<CourseAnalyticsDto>(json);
        if (json is null)
        {
            analytics = await getCourseAnalyticsHandler.HandleAsync(new GetCourseAnalyticsQuery(id), cancellationToken);
            if (analytics is not null)
            {
                await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(analytics), CacheOptions, cancellationToken);
            }
        }
        return analytics is null
            ? NotFound(new ProblemDetails { Status = 404, Title = "Course not found.", Detail = "We couldn't find this course to show its analytics." })
            : Ok(analytics);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCourseCommand command,
        CancellationToken cancellationToken)
    {
        var course = await updateCourseHandler.HandleAsync(id, command, cancellationToken);
        if (course is not null)
        {
            await cache.SetStringAsync("courses:version", Guid.NewGuid().ToString("N"), CacheOptions, cancellationToken);
            await cache.RemoveAsync($"course:{id}", cancellationToken);
            await cache.RemoveAsync($"course-analytics:{id}", cancellationToken);
        }
        return course is null
            ? NotFound(new ProblemDetails { Status = 404, Title = "Course not found.", Detail = "This course may have been removed. Refresh the course list and try again." })
            : Ok(course);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var course = await deleteCourseHandler.HandleAsync(id, cancellationToken);
        if (course is not null)
        {
            await cache.SetStringAsync("courses:version", Guid.NewGuid().ToString("N"), CacheOptions, cancellationToken);
            await cache.RemoveAsync($"course:{id}", cancellationToken);
            await cache.RemoveAsync($"course-analytics:{id}", cancellationToken);
        }
        return course is null
            ? NotFound(new ProblemDetails { Status = 404, Title = "Course not found.", Detail = "This course may already have been removed." })
            : NoContent();
    }

    private static bool IsValidPage(int page, int pageSize) =>
        page > 0 && pageSize is >= 1 and <= 50 && (long)(page - 1) * pageSize <= int.MaxValue;
}
