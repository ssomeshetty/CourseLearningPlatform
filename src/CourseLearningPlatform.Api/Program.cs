using CourseLearningPlatform.Application.AssignedCourses.Handlers;
using CourseLearningPlatform.Application.AssignedCourses.Interfaces;
using CourseLearningPlatform.Application.Courses.Interfaces;
using CourseLearningPlatform.Infrastructure.Persistence.Repositories;
using CourseLearningPlatform.Infrastructure.Persistence;
using CourseLearningPlatform.Application.Courses.Handlers;
using CourseLearningPlatform.Application.Users.Handlers;
using CourseLearningPlatform.Application.Users.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using CourseLearningPlatform.Infrastructure.Authentication;
using CourseLearningPlatform.Api.Authentication;
using CourseLearningPlatform.Api.Services;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "CourseLearningPlatform:";
});

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT issuer is not configured.");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT audience is not configured.");
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT signing key is not configured. Set it with user-secrets.");
if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("JWT signing key must be at least 32 bytes long.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Please sign in to continue.",
                    Detail = "Your sign-in has expired or is missing. Sign in and try again."
                });
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "You don't have permission to do that.",
                    Detail = "Ask an administrator if you think you should have access."
                });
            }
        };
    });
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IAssignedCourseRepository, AssignedCourseRepository>();
builder.Services.AddScoped<EnrollUserInCourseHandler>();
builder.Services.AddScoped<GetUserAssignedCoursesHandler>();
builder.Services.AddScoped<UpdateAssignedCourseStatusHandler>();

builder.Services.AddScoped<CreateCourseHandler>();

builder.Services.AddScoped<UpdateCourseHandler>();

builder.Services.AddScoped<DeleteCourseHandler>();

builder.Services.AddScoped<CourseQueryHandler>();
builder.Services.AddScoped<GetCourseAnalyticsHandler>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<UserProfileHandler>();
builder.Services.AddScoped<AuthenticationHandler>();
builder.Services.AddSingleton<IPasswordHashService, Pbkdf2PasswordHashService>();
builder.Services.AddSingleton<IAccessTokenService, JwtAccessTokenService>();
builder.Services.AddScoped<GetUserCourseAnalyticsHandler>();
builder.Services.AddTransient<EmailService>();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RequestValidation");
        var fields = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .Select(entry => entry.Key)
            .ToArray();
        logger.LogWarning("Invalid request for {Path}. Fields: {Fields}.",
            context.HttpContext.Request.Path, fields);

        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Please check your information.",
            Detail = "Some required information is missing or invalid. Review the fields and try again."
        };
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        return new BadRequestObjectResult(problem);
    };
});
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddOpenApi();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var timer = System.Diagnostics.Stopwatch.StartNew();
    try
    {
        await next();
    }
    finally
    {
        var level = context.Response.StatusCode >= 500
            ? LogLevel.Error
            : context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
        app.Logger.Log(level, "HTTP {Method} {Path} returned {StatusCode} in {ElapsedMilliseconds} ms.",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, timer.ElapsedMilliseconds);
    }
});
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
} 

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

