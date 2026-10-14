using CourseLearningPlatform.Application.Courses.Interfaces;
using CourseLearningPlatform.Infrastructure.Persistence.Repositories;
using CourseLearningPlatform.Infrastructure.Persistence;
using CourseLearningPlatform.Application.Courses.Handlers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<ICourseRepository, CourseRepository>();

builder.Services.AddScoped<CreateCourseHandler>();

builder.Services.AddScoped<CourseQueryHandler>();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();