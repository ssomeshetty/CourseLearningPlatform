using CourseLearningPlatform.Application.Users.Commands;
using CourseLearningPlatform.Application.Users.Dtos;
using CourseLearningPlatform.Application.Users.Handlers;
using CourseLearningPlatform.Application.Users.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using CourseLearningPlatform.Application;
using System.Text.Json;

namespace CourseLearningPlatform.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/users")]
public class UserProfilesController(
    UserProfileHandler userProfileHandler,
    IDistributedCache cache) : ControllerBase
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
    };

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] UserProfileCreateCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userProfileHandler.CreateAsync(command, cancellationToken);
        await cache.SetStringAsync("users:version", Guid.NewGuid().ToString("N"), CacheOptions, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id },
            user);
    }

    [HttpGet]
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

        var version = await cache.GetStringAsync("users:version", cancellationToken) ?? "0";
        var cacheKey = $"users:{version}:{page}:{pageSize}";
        var json = await cache.GetStringAsync(cacheKey, cancellationToken);
        var users = json is null ? null : JsonSerializer.Deserialize<PagedResult<UserProfileDTO>>(json);
        if (users is null)
        {
            users = await userProfileHandler.GetAllAsync(
                new GetAllUserProfilesQuery(page, pageSize), cancellationToken);
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(users), CacheOptions, cancellationToken);
        }

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"user:{id}";
        var json = await cache.GetStringAsync(cacheKey, cancellationToken);
        var user = json is null ? null : JsonSerializer.Deserialize<UserProfileDTO>(json);
        if (json is null)
        {
            user = await userProfileHandler.GetByIdAsync(new GetUserProfileByIdQuery(id), cancellationToken);
            if (user is not null)
            {
                await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(user), CacheOptions, cancellationToken);
            }
        }

        return user is null
            ? NotFound(new ProblemDetails { Status = 404, Title = "User not found.", Detail = "This user profile may have been removed." })
            : Ok(user);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UserProfileUpdateCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userProfileHandler.UpdateAsync(id, command, cancellationToken);
        if (user is not null)
        {
            await cache.SetStringAsync("users:version", Guid.NewGuid().ToString("N"), CacheOptions, cancellationToken);
            await cache.RemoveAsync($"user:{id}", cancellationToken);
        }

        return user is null
            ? NotFound(new ProblemDetails { Status = 404, Title = "User not found.", Detail = "This user profile may have been removed. Refresh the user list and try again." })
            : Ok(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await userProfileHandler.DeleteAsync(id, cancellationToken);
        if (deleted)
        {
            await cache.SetStringAsync("users:version", Guid.NewGuid().ToString("N"), CacheOptions, cancellationToken);
            await cache.RemoveAsync($"user:{id}", cancellationToken);
            await cache.RemoveAsync($"user-analytics:{id}", cancellationToken);
            await cache.RemoveAsync($"user-courses:{id}", cancellationToken);
        }

        return deleted
            ? NoContent()
            : NotFound(new ProblemDetails { Status = 404, Title = "User not found.", Detail = "This user profile may already have been removed." });
    }

    private static bool IsValidPage(int page, int pageSize) =>
        page > 0 && pageSize is >= 1 and <= 50 && (long)(page - 1) * pageSize <= int.MaxValue;
}
