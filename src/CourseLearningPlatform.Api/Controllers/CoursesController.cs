using CourseLearningPlatform.Application.Courses.Commands;
using CourseLearningPlatform.Application.Courses.Handlers;
using CourseLearningPlatform.Application.Courses.Queries;
using Microsoft.AspNetCore.Mvc;

namespace CourseLearningPlatform.Api.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(CreateCourseHandler createCourseHandler, UpdateCourseHandler updateCourseHandler, DeleteCourseHandler deleteCourseHandler, CourseQueryHandler courseQueryHandler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseCommand command,
        CancellationToken cancellationToken = default)
    {

        var course = await createCourseHandler.HandleAsync(command, cancellationToken);
        return Created($"/api/courses/{course.Id}", course);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var courses = await courseQueryHandler.GetAllAsync(new GetCoursesQuery(), cancellationToken);

        return Ok(courses);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var course = await courseQueryHandler.GetByIdAsync(new GetCourseByIdQuery(id), cancellationToken);

        return course is null ? NotFound() : Ok(course);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseCommand command,
        CancellationToken cancellationToken)
    {

        var course = await updateCourseHandler.HandleAsync(id, command, cancellationToken);

        return course is null ? NotFound() : Ok(course);

    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var course = await deleteCourseHandler.HandleAsync(id, cancellationToken);

        return course is null ? NotFound() : NoContent();
    }
}