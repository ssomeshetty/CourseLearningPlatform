# Course Learning Platform: End-to-End Project Guide

This guide explains how the Course Learning Platform is organized, how requests move from the browser to the database, and how to run and extend it. It is written for someone learning the project from the beginning.

## 1. What the application does

The application has two parts:

- Backend: ASP.NET Core Web API, C#, SQL Server, Entity Framework Core, Redis, JWT authentication, and SMTP email.
- Frontend: Angular. It provides sign-in/sign-up, a course catalog, learner course progress and analytics, and administrator pages for users and courses.

The database stores course information, user profiles, and the relationship between users and courses. Course image files are not uploaded by this backend; a course stores an ImageUrl.

## 2. Architecture used

The chosen style is Layered Architecture with CQRS-inspired command/query separation.

It is layered because code is grouped by responsibility. CQRS-inspired means operations that change data (commands) are kept separate in name and flow from operations that read data (queries). Both use the same application and SQL Server database. This project does not use separate read/write databases or a messaging bus.

This is not a strict Onion or Clean Architecture implementation. It uses some related ideas, such as keeping business work out of controllers and separating the domain model from database code. The practical structure is the four layers below.

| Layer | Project | Responsibility |
|---|---|---|
| Presentation | CourseLearningPlatform.Api | HTTP endpoints, request/response handling, authentication setup, error responses, CORS, logging, and dependency registration. |
| Application | CourseLearningPlatform.Application | Use cases and workflow: handlers, commands, queries, DTOs, and repository interfaces. |
| Domain | CourseLearningPlatform.Domain | Core business concepts and rules: Course, UserProfile, AssignedCourse, and enrollment status. |
| Infrastructure | CourseLearningPlatform.Infrastructure | SQL Server/EF Core, repository implementations, database mappings, password hashing and token implementation. |

Angular is a separate client. Its components display pages; its services call the API.

### Request direction

Angular page → Angular service → HTTP API controller → Application handler → Repository interface → Infrastructure repository → EF Core → SQL Server

The response travels back to the browser through the same path in reverse. Controllers translate HTTP into application requests and application results into HTTP status codes. They should not contain the main business workflow.

### CQRS terms in this project

- A command asks the application to change something: create a course, enroll a learner, update status.
- A query asks for information: list courses, get course details, calculate analytics.
- A handler performs one use case.
- A repository interface describes what data the application needs.
- A repository implementation uses EF Core to perform that work.

Handlers are called directly through dependency injection; the project does not require a mediator library.

## 3. Folder map

### Backend: src

- CourseLearningPlatform.Api/Controllers: HTTP endpoints.
- CourseLearningPlatform.Api/Requests: request body types and API inputs.
- CourseLearningPlatform.Api/Services: API-level services such as SMTP email.
- CourseLearningPlatform.Application/Courses: course commands, queries, handlers, DTOs, and repository interface.
- CourseLearningPlatform.Application/AssignedCourses: enrollment commands, queries, handlers, and interfaces.
- CourseLearningPlatform.Application/Users: user/authentication use cases, queries, DTOs, and interfaces.
- CourseLearningPlatform.Domain: entities and enrollment status enum.
- CourseLearningPlatform.Infrastructure/Persistence: AppDbContext, repositories, and EF migrations.
- CourseLearningPlatform.Infrastructure/Authentication: password hash and token implementations.

The solution file is CourseLearning.slnx. The API project filename currently uses the spelling CourseLeaningPlatform.Api.csproj; preserve that spelling in commands.

### Frontend: client-app/src/app

- app.component files: root layout, navigation, session state, and choosing which page to show.
- core/models: TypeScript data shapes shared by the UI.
- core/services: HTTP wrapper and services for authentication, courses, users, and enrollments.
- features/auth: sign-in and sign-up page.
- features/courses: course catalog and administrator course management.
- features/learning: learner course list/status and learner analytics.
- features/admin-users: administrator user list/details/edit and analytics.
- shared/components: reusable course cards, pagination, and alert messages.

Angular Router now manages browser paths. Main routes include /courses, /account, /my-courses, /my-analytics, /admin/courses, and /admin/users. Record detail routes include /courses/{id}, /admin/courses/{id}, and /admin/users/{id}. Guards redirect signed-out users to /account and users without the right role to an allowed page. Angular development hosting serves the app shell at these URLs on refresh; production hosting must also be configured to fall back to index.html for frontend paths.

## 4. Main data model

### Course

A course has an integer primary key Id, required title, description, instructor, image URL, creation time, cost, and duration in hours. SQL Server stores Cost and Hours as decimal values with configured precision. ImageUrl is required and has a maximum length.

### UserProfile

A learner profile has an integer primary key, name, unique email, unique employee code, joining date, admin flag, and password hash. New signups are normalized and created with IsAdmin=false. Admin privileges are assigned separately by an administrator/database operation.

The raw password should never be stored. The application hashes it using PBKDF2 before saving it.

### AssignedCourse: enrollment

This is the join between a user and a course. Its primary key is the pair (UserId, CourseId), so a user can enroll in a course once. It has a status and optional completion timestamp.

Statuses:

- Started — displayed as “Not started” in learner analytics/UI.
- InProgress.
- Completed.

The default is Started. Moving to Completed sets CompletedAtUtc; moving away from Completed clears it.

## 5. Important end-to-end flows

### A. Sign up

1. The Angular auth page gathers the user’s details and calls AuthService.
2. ApiService sends POST /api/auth/signup.
3. AuthController converts the request into a signup command.
4. AuthenticationHandler normalizes email and employee code and checks for duplicates.
5. The password hash service hashes the password; the user is stored through IUserProfileRepository.
6. The response contains user information and a JWT access token. Angular saves the token and session in browser storage.
7. The API attempts to send a welcome email using SMTP settings.

### B. Sign in and protected requests

1. The frontend sends email/password to POST /api/auth/signin.
2. The handler finds the account and verifies the supplied password against the stored PBKDF2 hash.
3. If valid, the token service creates a JWT with identity and role claims.
4. ApiService adds Authorization: Bearer token to later requests.
5. ASP.NET Core validates the token before a protected controller action runs.
6. Role attributes and per-user checks decide whether the signed-in person is allowed to perform the operation.

Sign-out removes the token/session from browser storage. JWTs are stateless, so the API does not revoke a token on sign-out; it remains usable until expiration.

### C. Browse and manage courses

- The catalog calls GET /api/courses?page=1&pageSize=10.
- A repository query orders by ID, counts rows, skips earlier pages, and returns the requested page.
- Course details use GET /api/courses/{id}.
- Admin create/update/delete use POST, PUT, and DELETE /api/courses...
- Admin-only actions require the Admin role.
- Course create/update/delete changes the course cache version and clears relevant individual course/analytics entries.
- Course creation stores the image URL text. The browser later requests that URL directly; the backend does not proxy the image.

### D. Enroll and update course status

1. The learner clicks Enroll. Angular calls POST /api/users/{userId}/courses/{courseId} using the signed-in user ID.
2. The API confirms the caller may access that user ID.
3. EnrollUserInCourseHandler confirms both user and course exist and checks for an existing enrollment.
4. The repository inserts an AssignedCourse. The composite key also prevents duplicate user/course records at the database level.
5. The API clears that user’s course and analytics cache, then returns the enrollment.
6. My Courses calls GET /api/users/{userId}/courses and displays the course title and status.
7. Changing status calls PUT /api/users/{userId}/courses/{courseId}/status.
8. The handler updates status and completion timestamp. Cache entries are cleared. A newly completed course triggers a completion email attempt.

A duplicate enrollment returns HTTP 409 Conflict with a readable “Already enrolled” message.

### E. Analytics

User analytics are calculated from enrollment status and the course Hours value:

- Total enrolled = number of enrollment rows.
- Not started = rows with Started.
- In progress = rows with InProgress.
- Completed = rows with Completed.
- Completed hours = full course duration for each completed course.
- In-progress estimated hours = half the duration for each in-progress course.
- Total estimated learning hours = completed hours + in-progress estimated hours.

The half-duration amount is an estimate, not measured time spent watching or studying. Admin course analytics count enrolled learners by status.

## 6. API overview

| Method | Endpoint | Access | Purpose |
|---|---|---|---|
| POST | /api/auth/signup | Anonymous | Create a learner account. |
| POST | /api/auth/signin | Anonymous | Sign in and receive a JWT. |
| POST | /api/auth/signout | Signed in | Client sign-out endpoint; client also clears local session. |
| GET | /api/courses?page=1&pageSize=10 | Anonymous | Browse paginated courses. |
| GET | /api/courses/{id} | Anonymous | Read one course. |
| POST | /api/courses | Admin | Create a course. |
| PUT | /api/courses/{id} | Admin | Update a course. |
| DELETE | /api/courses/{id} | Admin | Delete a course. |
| GET | /api/courses/{id}/analytics | Admin | Enrollment counts for one course. |
| GET | /api/users?page=1&pageSize=10 | Admin | Paginated user list. |
| GET | /api/users/{id} | Admin | User details. |
| POST | /api/users | Admin | Create a user from admin UI. |
| PUT | /api/users/{id} | Admin | Update a user profile. |
| DELETE | /api/users/{id} | Admin | Delete a user profile. |
| GET | /api/users/{userId}/courses | That user or Admin | List a user’s enrolled courses. |
| POST | /api/users/{userId}/courses/{courseId} | That user or Admin | Enroll a user. |
| PUT | /api/users/{userId}/courses/{courseId}/status | That user or Admin | Update enrollment status. |
| GET | /api/users/{userId}/analytics | That user or Admin | Learner learning metrics. |

Common HTTP statuses: 200 successful read/update, 201 created, 204 deleted/no body, 400 invalid input, 401 missing/invalid sign-in, 403 signed in but not allowed, 404 record not found, and 409 duplicate signup/enrollment.

## 7. Security and supporting services

- JWT signs API access tokens. Issuer, audience, and signing key must match.
- PBKDF2 hashes passwords with a random salt; raw passwords are not stored.
- The API fallback policy requires sign-in, with explicit anonymous course reads and sign-up/sign-in. Admin APIs require the Admin role. Learner enrollment/analytics verifies the route user ID belongs to the caller or the caller is an admin.
- CORS allows the Angular development origin http://localhost:4200 to call the API. CORS does not grant access to Google Drive or SharePoint images.
- ProblemDetails and the exception handler provide a consistent API error shape. Request logging records HTTP method, path, status, and elapsed time.
- Redis caches course/user pages and details, enrollments, and analytics for up to two hours. Writes update a version key or remove affected entries so later reads refresh.
- SMTP email uses MailKit. SMTP credentials belong in .NET User Secrets, not source files or this guide. Email errors are logged.

### Image URL rule

The browser can display an image only if the saved URL can be loaded by the learner’s browser and returns the actual image content. The frontend converts Google Drive file-view links into a direct display URL, but the Drive file still must be viewable by the learner's browser. Organization-protected SharePoint pages may redirect to sign-in instead of returning image bytes. For a small project, use a direct image URL with appropriate sharing permissions. If images must remain private, add a provider sign-in/API integration (such as Microsoft Graph) rather than weakening security.

## 8. Configuration and secrets

appsettings.json contains non-secret configuration such as Redis address, JWT issuer/audience, SMTP port, and logging levels. Store SQL connection string, JWT signing key, and SMTP username/password with .NET User Secrets for the API project.

From the repository root, set secrets with the API project path:

~~~powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your SQL Server connection string>" --project "src/CourseLearningPlatform.Api/CourseLeaningPlatform.Api.csproj"
dotnet user-secrets set "Jwt:Key" "<a random secret at least 32 bytes long>" --project "src/CourseLearningPlatform.Api/CourseLeaningPlatform.Api.csproj"
dotnet user-secrets set "Jwt:Issuer" "CourseLearningPlatform.Api" --project "src/CourseLearningPlatform.Api/CourseLeaningPlatform.Api.csproj"
dotnet user-secrets set "Jwt:Audience" "CourseLearningPlatform.Client" --project "src/CourseLearningPlatform.Api/CourseLeaningPlatform.Api.csproj"
dotnet user-secrets set "Smtp:Host" "<smtp host>" --project "src/CourseLearningPlatform.Api/CourseLeaningPlatform.Api.csproj"
dotnet user-secrets set "Smtp:Port" "587" --project "src/CourseLearningPlatform.Api/CourseLeaningPlatform.Api.csproj"
dotnet user-secrets set "Smtp:Username" "<smtp username>" --project "src/CourseLearningPlatform.Api/CourseLeaningPlatform.Api.csproj"
dotnet user-secrets set "Smtp:Password" "<smtp app password>" --project "src/CourseLearningPlatform.Api/CourseLeaningPlatform.Api.csproj"
dotnet user-secrets set "Smtp:From" "<verified sender email>" --project "src/CourseLearningPlatform.Api/CourseLeaningPlatform.Api.csproj"
~~~

Replace placeholders in your own terminal. Never paste real secrets into source code, Markdown, screenshots, or chat.

## 9. Run the project

Prerequisites: .NET SDK matching the project target, Node/npm, SQL Server, and Redis. The API HTTP port is 5198; Angular development server uses 4200.

Start Redis if using Docker Desktop:

~~~powershell
docker run --name course-learning-redis -p 6379:6379 -d redis:7-alpine
~~~

If that container already exists, start it with docker start course-learning-redis. Docker Desktop must be running.

From a PowerShell window at the repository root, start the API:

~~~powershell
dotnet run --project "src/CourseLearningPlatform.Api/CourseLeaningPlatform.Api.csproj"
~~~

Open a second PowerShell window and start Angular:

~~~powershell
cd "C:\Users\sak\OneDrive - Itron\Desktop\CourseLearningPlatform\client-app"
npm start
~~~

Browse to http://localhost:4200. Leave both terminals open. Stop a server by focusing its terminal and pressing Ctrl+C.

The frontend API address is in client-app/src/app/core/services/api.service.ts. If the API port changes, update that address to match src/CourseLearningPlatform.Api/Properties/launchSettings.json.

## 10. EF Core migrations

Migrations describe database schema changes over time. After editing an entity or EF mapping, create and apply a migration from the repository root:

~~~powershell
dotnet ef migrations add <MigrationName> --project "src/CourseLearningPlatform.Infrastructure/CourseLeaningPlatform.Infrastructure.csproj" --startup-project "src/CourseLearningPlatform.Api/CourseLeaningPlatform.Api.csproj" --output-dir Persistence/Migrations
dotnet ef database update --project "src/CourseLearningPlatform.Infrastructure/CourseLeaningPlatform.Infrastructure.csproj" --startup-project "src/CourseLearningPlatform.Api/CourseLeaningPlatform.Api.csproj"
~~~

Choose a descriptive migration name, review the generated migration before applying it, and keep migrations with the code. Back up important data before risky schema changes.

## 11. How to add a feature

1. Describe the user action and expected result in plain language.
2. Decide which entity or data must change.
3. Add/update the domain model only if business data or rules are genuinely new.
4. Add an application command or query and a handler.
5. Add a repository interface method if the handler needs data access.
6. Implement that method in Infrastructure with EF Core.
7. Register new dependencies in Program.cs.
8. Add or adjust the API endpoint and choose meaningful HTTP status codes and validation.
9. Update the Angular model/service and the page/component that uses it.
10. If the database shape changed, create and apply an EF migration.
11. Walk through the full browser-to-database flow and verify authorization, errors, and cache refresh.

Keep each layer focused: controllers translate HTTP; handlers coordinate the use case; repositories read/write data; domain types represent the business.

## 12. Current limitations to remember

- This is a learning project, not a production deployment. Production hosting needs secure deployment settings, secret management, backups, reviewed account/role administration, and operational monitoring.
- JWT sign-out clears the browser session but does not revoke an issued token.
- Redis must be available for cache-backed endpoints. If Redis is stopped, those API calls may fail until Redis starts or the cache setup is changed.
- Email delivery depends on valid SMTP settings and provider rules.
- ImageUrl stores a URL only; the backend does not upload, authenticate to, or proxy the image.
- The project currently targets net11.0 while its EF Core packages are version 10. Check .NET/EF Core version alignment if restore, build, tooling, or migration commands fail.

## 13. Glossary

- API: HTTP endpoints called by the frontend.
- Controller: ASP.NET class that receives a request and returns an HTTP response.
- DTO: data shape sent between application/API or API/frontend.
- EF Core: .NET library mapping C# objects to database tables and queries.
- Entity: a business object that is stored, such as Course.
- Handler: application class performing one use case.
- JWT: signed token sent with protected API requests.
- Migration: versioned database schema change.
- Pagination: returning a subset of a long list, such as 10 users per page.
- Repository: data-access abstraction.
- Redis cache: fast temporary storage for data that can be read again from the database.
- SMTP: protocol used by the application to send email.



