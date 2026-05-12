# TaskApp API

TaskApp API is a .NET 9 backend for a task/board management application. It provides authentication with JWT access tokens and refresh tokens, user registration with ASP.NET Core Identity, and CRUD-style operations for boards and board tasks.

## Project Structure

- `TaskAppApi/`: Main ASP.NET Core Web API project.
- `TaskAppApi.Tests/`: NUnit-based unit test project.
- `TaskAppApi.sln`: Solution file.

Within `TaskAppApi/`:

- `Controllers/`: HTTP endpoints (`AuthController`, `BoardController`, `BoardTaskController`).
- `Services/`: Business logic (`AuthService`, `BoardService`, `BoardTaskService`) behind interfaces.
- `Models/`: Domain models, DTO request/response records, EF Core `AppDbContext`, enums.
- `Options/`: `JwtOptions` configuration binding class.
- `Program.cs`: Dependency injection, middleware, authentication, Swagger, CORS, and development seeding.

## Core Features

- User authentication:
  - Register user with password.
  - Login with email/password.
  - JWT access token generation.
  - Refresh token issuance and rotation.
  - Logout by revoking user refresh tokens.
  - Email confirmation endpoint contract (token generation + confirmation flow scaffolded).
- Board management:
  - Create boards for authenticated users.
  - Retrieve all boards for authenticated users.
  - Retrieve a board with its tasks.
- Task management:
  - Create tasks within boards owned by the authenticated user.
  - Update task summary, description, due date, and status.
  - Delete tasks owned by authenticated user.
  - Retrieve specific task by ID for authenticated user.

## Technology Stack

- .NET 9 / ASP.NET Core Web API
- ASP.NET Core Identity (`User`, role management)
- Entity Framework Core (InMemory provider in current setup)
- JWT bearer authentication (`Microsoft.IdentityModel`)
- Swagger / OpenAPI (`Swashbuckle`)
- NUnit test framework

## Data Model Overview

### User
Custom identity user (`User : IdentityUser<Guid>`) with:

- `FirstName`
- `LastName`

### Board

- `Id` (Guid)
- `OwnerId` (Guid)
- `Name`
- `CreatedAt`
- `BoardTasks` (navigation)

### BoardTask

- `Id` (Guid)
- `BoardId` (Guid)
- `Summary`
- `Description`
- `CreatedAt`
- `UpdatedAt`
- `CreatedByUserId`
- `UpdatedByUserId`
- `DueAt`
- `Status` (`ToDo`, `InProgress`, `Done`)

### RefreshToken
Stores refresh tokens by user with expiry, used by auth refresh/rotation logic.

## Authentication and Authorization

- JWT bearer is configured in `Program.cs`.
- Access tokens include:
  - `sub` claim (user id)
  - `email` claim
  - role claims (`ClaimTypes.Role`)
- Refresh tokens are persisted in the database and rotated during refresh.
- Most board/task endpoints require `[Authorize]`.

## Configuration

`appsettings.json` contains:

- `JwtOptions:SecretKey`
- `JwtOptions:Issuer`
- `JwtOptions:Audience`
- `JwtOptions:ExpirationInMinutes`
- `JwtOptions:RefreshExpirationInDays`

Important: set non-empty secure values for `SecretKey`, `Issuer`, and `Audience` before running outside local experimentation.

## Development Behavior

In development (`app.Environment.IsDevelopment()`), startup logic:

- Enables Swagger UI.
- Ensures `Member` and `Admin` roles exist.
- Ensures in-memory DB is created.
- Seeds a default admin user if no users exist.

Default seeded admin:

- Email: `eric.meuse@gmail.com`
- Password: `Test123!`

## API Endpoints

Base route patterns follow controller names: `/Auth`, `/Board`, `/BoardTask`.

### AuthController

- `POST /Auth/Login`
- `POST /Auth/Register`
- `POST /Auth/Refresh`
- `POST /Auth/Logout` (authorized)
- `GET /Auth/ConfirmEmail`
- `GET /Auth/Me` (authorized)

### BoardController

- `GET /Board/GetAll` (authorized)
- `POST /Board/Create` (authorized)
- `GET /Board/GetBoardWithTasksById/{id}` (authorized)
- `POST /Board/Update` (authorized)

### BoardTaskController

- `POST /BoardTask/Create` (authorized)
- `POST /BoardTask/Update` (authorized)
- `POST /BoardTask/Delete` (authorized)
- `GET /BoardTask/GetById/{id}` (authorized)

## Running the Project

### Prerequisites

- .NET SDK 9.0+

### Commands

From repository root:

```bash
dotnet restore
dotnet build TaskAppApi.sln
dotnet run --project TaskAppApi
```

Swagger UI is available in development mode at the default ASP.NET Core swagger route.

## Running Tests

```bash
dotnet test TaskAppApi.sln
```

The test project (`TaskAppApi.Tests`) includes unit tests for:

- Controllers:
  - `AuthController`
  - `BoardController`
  - `BoardTaskController`
- Services:
  - `AuthService`
  - `BoardService`
  - `BoardTaskService`

## Current Architectural Notes

- Services are cleanly separated from HTTP controllers via interfaces, making logic easier to unit test.
- The current database provider is EF Core InMemory, suitable for local development/tests but not production persistence.
- Refresh token storage and revocation are handled server-side, enabling logout and token rotation controls.

## Suggested Next Improvements

- Replace InMemory DB with PostgreSQL/SQL Server for persistent environments.
- Add input validation attributes and centralized validation handling.
- Add consistent error contracts and richer HTTP status mapping.
- Add integration tests for full auth and board/task flows.
- Add email delivery implementation for confirmation flow.

## My Thought Process
- I wanted to try and keep the backend simple with authentication using Identity for the users and JWT for stateless auth and separate CRUD controllers for the two application models, Board and BoardTask (Task is already a well used model name).
- I employed the Controller/Service architecture instead of Features and minimal API as I have more experience with that style.
- I used AI (Codex) to generate the unit tests as writing those out can take up a lot of time.
- I also used Codex to generate a majority of this README. After reviewing the output, it appears to accurately describe the project. 