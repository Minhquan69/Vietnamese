# Project Skill Guide

## Mission

Build a modern, scalable, AI-powered Vietnamese learning platform for foreigners. Every new feature must improve learner confidence, measurable progress, and long-term maintainability.

## Technology Context

Frontend:

- Angular 18
- TypeScript
- RxJS
- Angular Router
- HttpClient interceptor
- Plain CSS today, target design tokens and shared UI components

Backend:

- ASP.NET Core .NET 8
- C#
- EF Core
- SQL Server
- JWT Bearer authentication
- BCrypt
- AutoMapper
- Controller -> Service -> Repository pattern

Supporting scripts:

- Python scripts for transcript/pronunciation experiments.

## Coding Standards

### General

- Do not remove existing business logic without a migration path.
- Prefer small, incremental refactors.
- Keep source files focused on one responsibility.
- Use clear names: `LearnerDashboardComponent`, `PronunciationAttempt`, `ReviewSchedule`.
- Avoid hardcoded URLs, secrets, and magic strings.
- Add comments only for non-obvious business logic.

### Angular

- Use standalone components and lazy-loaded route groups for new feature areas.
- Keep API calls in services or facades, not directly spread across UI methods.
- Use typed DTOs for every API call.
- Use route guards for auth and role checks.
- Use shared UI primitives for repeated controls.
- Use accessible HTML first.
- Use `environment.ts` for API base URLs.
- Replace browser `alert()` with toast and inline validation.

### ASP.NET Core

- Controllers should be thin.
- Services own business rules.
- Repositories own persistence queries.
- Use async EF Core calls.
- Add validation before persistence.
- Use role policies for admin/moderator actions.
- Return consistent response shapes.
- Do not hardcode JWT secrets.

### Database

- Use migrations for every schema change.
- Add indexes for lookup-heavy queries.
- Prefer explicit relationships over generic string references when feasible.
- Use audit fields for new tables.
- Add soft-delete where content should be recoverable.

## UI Rules

- Learner UI should be friendly, calm, and task-focused.
- Admin UI should be dense, clear, and operational.
- Use mobile-first responsive layout.
- Do not use giant marketing hero sections for logged-in flows.
- Use cards for repeated items, not nested page sections.
- Use clear progress indicators.
- Use icons only when they clarify actions.
- Provide loading, empty, error, and success states.

## AI Feature Rules

- AI provider keys must stay on the backend.
- AI output that becomes curriculum must be reviewed.
- AI feedback should be level-aware and concise.
- Store AI attempts, feedback, and recommendations for learning analytics.
- Add rate limits and cost tracking.

## API Creation Rules

When creating an API:

1. Define DTO.
2. Add validation.
3. Add service interface method.
4. Add service implementation.
5. Add repository query if persistence is needed.
6. Add controller endpoint.
7. Add frontend typed service method.
8. Add loading/error UI state.

## Component Creation Rules

When creating a component:

1. Define its role: page, container, or presentational component.
2. Keep data fetching out of presentational components.
3. Define inputs and outputs clearly.
4. Add responsive CSS.
5. Add keyboard and focus behavior where interactive.
6. Add empty/loading/error states.

## Testing Strategy

- Backend: service tests for business rules, repository integration tests for key queries.
- Frontend: route guard tests, service tests, and component tests for quiz/practice flows.
- E2E: login, placement test, lesson completion, quiz submission, admin content creation.

## Security Checklist

- No secrets in source.
- JWT validation uses configured issuer/audience in production.
- Role policies protect admin APIs.
- Uploads have size/type validation.
- Passwords use BCrypt.
- CORS is environment-specific.
- API errors do not leak stack traces.
