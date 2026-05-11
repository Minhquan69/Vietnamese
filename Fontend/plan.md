# Development Plan

## Current System Summary

This repository is a full-stack Vietnamese learning platform with:

- Angular 18 frontend using standalone app configuration, TypeScript, RxJS, and plain CSS.
- ASP.NET Core .NET 8 backend using Controllers, Services, Repositories, EF Core, SQL Server, AutoMapper, BCrypt, and JWT authentication.
- Current product modules: account/authentication, user profile, admin user management, video transcript search, level/course/unit management, quiz/test management, placement tests, user progress, and uploaded media.
- Python scripts for pronunciation/video/transcript experiments, currently outside the main API integration.

## Key Problems To Fix

1. Security risks:
   - JWT secret is hardcoded in `Program.cs`.
   - `appsettings.json` contains a real secret.
   - Role checks are mostly UI-driven and `[Authorize]` lacks role policies.
   - Uploaded files need stricter validation, size limits, malware-safe storage, and private access rules.

2. Frontend architecture:
   - Routes are eager-loaded and not guarded.
   - API base URLs are hardcoded in services.
   - `provideHttpClient()` is registered twice.
   - UI uses global layout inside `app.component`, making responsive redesign harder.
   - Components mix UI, data fetching, form state, and business rules.

3. Backend architecture:
   - Repository/service pattern exists but endpoints are not fully RESTful.
   - Some controller names do not match classes, for example `LearningController.cs` contains `LevelController`.
   - No consistent request validation layer.
   - No migrations currently tracked after cleanup.
   - Exception middleware returns many errors as HTTP 400 instead of typed status codes.

4. Product gaps:
   - No structured onboarding or placement-driven learning path.
   - No spaced repetition, flashcards, vocabulary mastery, pronunciation scoring, streaks, badges, certificates, or AI tutor.
   - No teacher feedback or community layer.
   - No analytics dashboard for admins.

## Phase 1: Stabilize And Secure

- Move JWT key and connection strings to environment variables or .NET user secrets.
- Add `AuthGuard`, `RoleGuard`, and route-level permission handling in Angular.
- Add backend role policies: `Admin`, `Moderator`, `User`.
- Replace hardcoded frontend API URLs with `environment.ts`.
- Add DTO validation using data annotations or FluentValidation.
- Restore EF Core migration workflow.
- Add API response envelope and typed error handling.
- Add upload size limits and allowed MIME validation.

## Phase 2: Frontend Architecture Upgrade

- Split layout into `core/layout/app-shell`, `shared/components`, and feature modules.
- Lazy-load user/admin routes.
- Add shared UI primitives: button, card, modal, toast, empty state, loading state, form field, progress ring, badge.
- Replace `alert()` with a toast/notification service.
- Create centralized API client and error interceptor.
- Add responsive mobile navigation and accessible keyboard states.

## Phase 3: Learning Experience

- Add learner dashboard with daily goal, streak, recommended lesson, progress map, weak skills, and review queue.
- Add structured skills: pronunciation, tones, vocabulary, grammar, listening, speaking, reading, writing.
- Add CEFR-like progression: Starter, A1, A2, B1, B2, Advanced.
- Add flashcard and spaced repetition tables.
- Add quiz review screen with explanation and retry flow.
- Add placement test result -> recommended starting level.

## Phase 4: AI-Powered Features

- Add AI tutor service behind backend API.
- Add pronunciation scoring workflow using browser audio upload, backend processing, and AI feedback.
- Add AI-generated exercises with teacher/admin review before publishing.
- Add personalized recommendations based on quiz results, progress, and mistakes.
- Add AI writing correction for Vietnamese tone marks, grammar, and natural phrasing.
- Add AI conversation roleplay for practical scenarios.

## Phase 5: Admin And Content Platform

- Build content builder for levels, courses, units, media, quizzes, passages, and explanations.
- Add moderation workflow: draft, review, published, archived.
- Add analytics: active learners, completion rate, hardest questions, retention, AI usage.
- Add import/export content support.

## Phase 6: Production Readiness

- Add backend unit tests and integration tests.
- Add frontend component tests for core flows.
- Add CI pipeline: restore, build, test, lint.
- Add Docker deployment or cloud deployment profile.
- Add observability: structured logs, request IDs, health checks.
- Add backup and restore strategy for SQL Server and uploaded media.

## Immediate Next Sprint

1. Fix security baseline.
2. Introduce Angular environments and guards.
3. Create shared UI foundation.
4. Refactor app shell from `app.component`.
5. Add learner dashboard shell.
6. Add database migration baseline.
7. Add AI service interface without binding to a provider yet.
