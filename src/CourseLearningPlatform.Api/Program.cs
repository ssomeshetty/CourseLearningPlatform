using CourseLearningPlatform.Application.AssignedCourses.Handlers;
using CourseLearningPlatform.Application.AssignedCourses.Interfaces;
using CourseLearningPlatform.Application.Courses.Interfaces;
using CourseLearningPlatform.Infrastructure.Persistence.Repositories;
using CourseLearningPlatform.Infrastructure.Persistence;
using CourseLearningPlatform.Application.Courses.Handlers;
using CourseLearningPlatform.Application.Users.Handlers;
using CourseLearningPlatform.Application.Users.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IAssignedCourseRepository, AssignedCourseRepository>();
builder.Services.AddScoped<EnrollUserInCourseHandler>();
builder.Services.AddScoped<GetUserAssignedCoursesHandler>();
builder.Services.AddScoped<UpdateAssignedCourseStatusHandler>();

builder.Services.AddScoped<CreateCourseHandler>();

builder.Services.AddScoped<UpdateCourseHandler>();

builder.Services.AddScoped<DeleteCourseHandler>();

builder.Services.AddScoped<CourseQueryHandler>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<UserProfileHandler>();

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



