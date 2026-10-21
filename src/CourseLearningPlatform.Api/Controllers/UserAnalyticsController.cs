using System.Security.Claims;
using System.Text.Json;
using CourseLearningPlatform.Api.Services;
using CourseLearningPlatform.Application.Users.Dtos;
using CourseLearningPlatform.Application.Users.Handlers;
using CourseLearningPlatform.Application.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CourseLearningPlatform.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId:int}/analytics")]
public sealed class UserAnalyticsController(
    GetUserCourseAnalyticsHandler analyticsHandler,
    IDistributedCache cache) : ControllerBase
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
    };

    [HttpGet]
    public async Task<IActionResult> Get(int userId, CancellationToken cancellationToken)
    {
        if (!CanAccessUser(userId)) return Forbid();

        var cacheKey = $"user-analytics:{userId}";
        var json = await cache.GetStringAsync(cacheKey, cancellationToken);
        var analytics = json is null
            ? null
            : JsonSerializer.Deserialize<UserCourseAnalyticsDto>(json);

        if (json is null)
        {
            analytics = await analyticsHandler.HandleAsync(
                new GetUserCourseAnalyticsQuery(userId), cancellationToken);
            if (analytics is not null)
            {
                await cache.SetStringAsync(
                    cacheKey, JsonSerializer.Serialize(analytics), CacheOptions, cancellationToken);
            }
        }

        return analytics is null
            ? NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "User not found.",
                Detail = "We couldn't find this user to show their analytics."
            })
            : Ok(analytics);
    }

    private bool CanAccessUser(int userId) => User.IsInRole("Admin") ||
        (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var authenticatedUserId) &&
         authenticatedUserId == userId);
}
