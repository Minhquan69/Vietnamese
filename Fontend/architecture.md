# Architecture Review

## Repository Shape

The repository is a monorepo:

- `Fontend/`: Angular 18 web application.
- `Backend/Backend/`: ASP.NET Core .NET 8 API.
- `Backend/Backend/wwwroot/`: uploaded media and static files.
- Root Python scripts: experimental transcript/pronunciation tooling.
- Root `.gitignore`: now protects dependencies, build output, IDE files, runtime media, and virtual environments.

## Frontend Architecture

Current frontend stack:

- Angular 18
- TypeScript 5.5
- RxJS
- Angular Router
- Angular HttpClient
- Karma/Jasmine test setup
- Plain CSS per component

Current feature areas:

- Public: home, login, register, vocabulary/video search.
- User: profile, learning units, quiz, placement practice.
- Admin/moderator: users, videos, levels, courses, units, unit detail, tests, placements.

Observed issues:

- `app.component` owns global shell, header, sidebar, footer, auth state, and navigation.
- Routes are eager-loaded and have no guards.
- Role-specific navigation exists in UI, but protected backend routes do not consistently enforce roles.
- API URLs are hardcoded as `http://localhost:5108`.
- `provideHttpClient()` is registered twice.
- No design token system, no shared component library, and no common loading/error/empty state pattern.
- CSS is component-local but inconsistent, with fixed desktop layout and limited mobile handling.

Target frontend structure:

```text
src/app/
  core/
    auth/
    guards/
    interceptors/
    layout/
    config/
  shared/
    components/
    directives/
    pipes/
    models/
    ui/
  features/
    learner/
    admin/
    auth/
    content/
    ai-tutor/
    practice/
```

## Backend Architecture

Current backend stack:

- ASP.NET Core .NET 8
- EF Core SQL Server
- JWT Bearer authentication
- BCrypt password hashing
- AutoMapper
- Controller -> Service -> Repository pattern

Current API areas:

- `api/account`: login, register, profile, users, role/status updates.
- `api/learning`: levels, courses, units, media upload, learning path, user progress.
- `api/tests`: quizzes, quiz submit, results, placement tests.
- `api/videos`: video import, transcript search, video management.

Observed issues:

- JWT signing key is duplicated: hardcoded in `Program.cs` and loaded from config in `JwtService`.
- Authorization is coarse; role policies are missing.
- Controllers return mixed text/object payloads.
- Exception middleware maps most errors to HTTP 400.
- Repository interfaces are numerous but repetitive.
- Uploads are stored under API `wwwroot`, which is acceptable for prototype but weak for production.
- EF migrations are absent after cleanup; schema evolution needs a controlled migration strategy.

Target backend structure:

```text
Backend/
  Api/
    Controllers/
    Middleware/
    Filters/
  Application/
    Services/
    DTOs/
    Validators/
    UseCases/
  Domain/
    Entities/
    Enums/
    ValueObjects/
  Infrastructure/
    Data/
    Repositories/
    Storage/
    AI/
```

## Authentication Flow

Current flow:

1. Frontend posts login/register to `api/account`.
2. Backend validates user and returns JWT.
3. Frontend stores JWT in `localStorage`.
4. HTTP interceptor adds `Authorization: Bearer <token>`.
5. Backend `[Authorize]` protects selected endpoints.

Target improvements:

- Add token expiration handling and automatic logout.
- Add route guards for authenticated and role-specific pages.
- Prefer httpOnly cookies for production, or harden localStorage approach with CSP and short-lived access tokens.
- Add refresh-token support if session persistence is required.
- Move secret management to environment variables.

## Data Model

Core entities:

- User, Role
- Level, Course, Unit
- Quiz, Part, Passage, Question, Answer
- UserQuiz, UserAnswer, UserProgress
- PlacementTest
- Video, Transcript

The model supports hierarchical content and quiz attempts, but does not yet support:

- Vocabulary items
- Grammar points
- Pronunciation attempts
- Spaced repetition
- Skill taxonomy
- Achievements and streaks
- AI feedback records
- Learning recommendations
- Content publishing workflow

## Scalability Direction

- Keep API stateless.
- Move uploaded media to object storage when deploying.
- Add pagination consistently.
- Add indexes for search and user progress queries.
- Add background jobs for transcript import, AI scoring, and recommendation generation.
- Split high-cost AI workloads from request/response paths.
