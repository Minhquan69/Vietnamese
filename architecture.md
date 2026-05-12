# Architecture Documentation

# TECH STACK

Frontend:
- Angular
- Angular Material
- SCSS
- RxJS
- Signals

Backend:
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server

AI:
- OpenAI API
- Whisper API

--------------------------------------------------
# FRONTEND STRUCTURE
--------------------------------------------------

src/
 ├── app/
 │    ├── core/
 │    ├── shared/
 │    ├── layouts/
 │    ├── features/
 │    │      ├── auth/
 │    │      ├── dashboard/
 │    │      ├── courses/
 │    │      ├── lessons/
 │    │      ├── vocabulary/
 │    │      ├── quiz/
 │    │      ├── ai-chat/
 │    │      ├── speaking/
 │    │      ├── admin/

--------------------------------------------------
# BACKEND STRUCTURE
--------------------------------------------------

Backend/
 ├── API/
 ├── Application/
 ├── Domain/
 ├── Infrastructure/
 ├── Persistence/

--------------------------------------------------
# CLEAN ARCHITECTURE
--------------------------------------------------

## Domain
- entities
- enums
- interfaces

## Application
- DTOs
- services
- validators

## Infrastructure
- repositories
- external services
- AI integrations

## API
- controllers
- middleware
- auth

--------------------------------------------------
# DATABASE CONNECTION
--------------------------------------------------

Server=DESKTOP-ABBQB0D;
Database=VietnameseLearningPlatform;
Trusted_Connection=True;
TrustServerCertificate=True;