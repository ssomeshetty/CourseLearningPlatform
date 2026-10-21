using CourseLearningPlatform.Api.Requests;
using CourseLearningPlatform.Application.Users.Commands;
using CourseLearningPlatform.Application.Users.Exceptions;
using CourseLearningPlatform.Application.Users.Handlers;
using CourseLearningPlatform.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CourseLearningPlatform.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    AuthenticationHandler authenticationHandler,
    EmailService emailService,
    IDistributedCache cache) : ControllerBase
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
    };

    [AllowAnonymous]
    [HttpPost("signup")]
    public async Task<IActionResult> SignUp(SignUpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authenticationHandler.SignUpAsync(
                new SignUpCommand(request.Name, request.Email, request.EmployeeCode, request.JoiningDate, request.Password), cancellationToken);
            await cache.SetStringAsync("users:version", Guid.NewGuid().ToString("N"), CacheOptions, cancellationToken);
            await emailService.SendAsync(
                response.Email,
                "Welcome to Course Learning Platform",
                $"Hello {response.Name}, your account has been created successfully.");
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (DuplicateAccountException exception)
        {
            return Conflict(new ProblemDetails { Title = "Account already exists.", Detail = exception.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("signin")]
    public async Task<IActionResult> SignIn(SignInRequest request, CancellationToken cancellationToken)
    {
        var response = await authenticationHandler.SignInAsync(
            new SignInCommand(request.Email, request.Password), cancellationToken);
        return response is null
            ? Unauthorized(new ProblemDetails { Title = "Invalid email or password." })
            : Ok(response);
    }

    [Authorize]
    [HttpPost("signout")]
    public IActionResult SignOutUser()
    {
        // JWT access tokens are stateless. The client discards its token; it remains valid until expiry.
        return NoContent();
    }
}
