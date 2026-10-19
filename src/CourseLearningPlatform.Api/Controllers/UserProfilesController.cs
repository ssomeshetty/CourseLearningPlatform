using CourseLearningPlatform.Application.Users.Commands;
using CourseLearningPlatform.Application.Users.Handlers;
using CourseLearningPlatform.Application.Users.Queries;
using Microsoft.AspNetCore.Mvc;

namespace CourseLearningPlatform.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserProfilesController(UserProfileHandler userProfileHandler)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] UserProfileCreateCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userProfileHandler.CreateAsync(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id },
            user);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await userProfileHandler.GetAllAsync(
            new GetAllUserProfilesQuery(),
            cancellationToken);

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var user = await userProfileHandler.GetByIdAsync(
            new GetUserProfileByIdQuery(id),
            cancellationToken);

        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UserProfileUpdateCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userProfileHandler.UpdateAsync(
            id,
            command,
            cancellationToken);

        return user is null ? NotFound() : Ok(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await userProfileHandler.DeleteAsync(id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
