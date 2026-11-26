# Pawly — Habit RPG

Pawly is a game-style habit tracker. Users add habits, mark them done every day and earn XP, levels, coins and achievements. Consecutive days form a streak that breaks when a day is missed. Coins are spent on pets, and the leaderboard of friends updates in real time.

Joint final project for the courses *Programmeerimine* and *Hajusrakendused* (TTHK, TARpv24).

## Architecture

| Project | Type | Role |
| --- | --- | --- |
| `HarjumusteRPG.API` | ASP.NET Core Web API | Business logic, EF Core, Repository, DI, JWT and roles, WebSocket |
| `HarjumusteRPG.Mobile` | .NET MAUI (MVVM) | Main client for users |
| `HarjumusteRPG.Admin` | ASP.NET Core MVC | Second client: web admin panel |
| `HarjumusteRPG.Worker` | Worker Service | Background service that checks streaks every day at 00:05 |
| `HarjumusteRPG.Tests` | xUnit + Moq | Unit tests |

```
MAUI app ─┐
Admin MVC ─┼── HTTP + JWT ──► Web API ── EF Core ──► SQL Server
Worker ───┘                     │
MAUI app ◄── WebSocket /ws/leaderboard
```

Only the Web API accesses the database. The Worker logs in with a `Service` account and calls the API over `HttpClient`.

## Features

- Habits: every day or on selected weekdays; CRUD, search, filter, sorting, pagination
- Completion history, XP, levels, streaks and achievements
- Level-up animation, daily coin gift, pet shop and pet needs (joy, food, energy)
- Weekly and monthly statistics charts
- Friends and a real-time leaderboard
- Theme switching and two languages (ET/EN)

## Tech stack

.NET 10, ASP.NET Core, EF Core (SQL Server LocalDB), JWT Bearer, Swagger (OpenAPI), ProblemDetails, .NET MAUI, CommunityToolkit.Mvvm, xUnit, Moq.

## Getting started

### Requirements

- Visual Studio 2022 or later with the .NET MAUI and ASP.NET workloads
- .NET 10 SDK
- SQL Server LocalDB

### 1. Create the database

In the Package Manager Console:

```powershell
Update-Database -Project HarjumusteRPG.API -StartupProject HarjumusteRPG.API
```

Migrations also create seed data: roles, achievements and two system accounts.

### 2. Run the projects

Set **multiple startup projects**: `HarjumusteRPG.API`, `HarjumusteRPG.Mobile` (Windows Machine or Android), `HarjumusteRPG.Admin`, `HarjumusteRPG.Worker`. Set `HarjumusteRPG.Tests` to *None*.

| Project | URL |
| --- | --- |
| API | https://localhost:7008 |
| Swagger | https://localhost:7008/swagger |
| Admin | https://localhost:7079 |

Wait for `Now listening on: https://localhost:7008` in the API console before logging in.

### 3. Development accounts

| Role | Email | Password |
| --- | --- | --- |
| Admin | admin@pawly.local | Admin123! |
| Service | service@pawly.local | Service123! |

Regular users register in the mobile app. The password must contain a lowercase letter, an uppercase letter, a digit and a special character.

## API overview

| Method | Endpoint | Role | Description |
| --- | --- | --- | --- |
| POST | `/api/auth/register`, `/api/auth/login` | — | Registration and JWT login |
| GET/POST/PUT/DELETE | `/api/habits` | User | Habits CRUD |
| POST | `/api/habits/{id}/completions` | User | Mark a habit done; API calculates XP, level, coins and streak |
| GET | `/api/me/progress` | User | Level, XP, coins, achievements |
| GET | `/api/friends/leaderboard` | User | Friends leaderboard |
| GET/POST | `/api/me/pets`, `/api/me/pets/{key}/buy`, `/api/me/pet/feed` | User | Pet shop |
| GET/POST/PUT/DELETE | `/api/admin/...` | Admin | Users and achievements |
| POST | `/api/streaks/check` | Service | Breaks missed streaks and returns changed users |
| POST | `/api/demo/next-day`, `/api/demo/reset` | User | Demo time travel (Development only) |

All endpoints are documented in Swagger with descriptions and examples. Use **Authorize** and paste the JWT token. Ready-made requests are in `HarjumusteRPG.API/HarjumusteRPG.API.http`.

Errors are returned in the `ProblemDetails` format by a global exception handler.

## Real-time updates

- Endpoint: `wss://localhost:7008/ws/leaderboard?access_token=<JWT>`
- Channel: `friends-of-{userId}`
- Message: `LeaderboardChanged`

The message is sent from the service layer after data is saved, so friends see leaderboard changes immediately.

## Background service

`HarjumusteRPG.Worker` is a separate process (`BackgroundService` + `PeriodicTimer`). Every day at 00:05 it logs in with the Service account and calls `POST /api/streaks/check`. It does not reference `DbContext`. Settings are in `HarjumusteRPG.Worker/appsettings.json`.

## Tests

```powershell
dotnet test
```

Unit tests cover game rules, completions, daily gifts, friends and the pet shop; some services are tested with Moq mocks.

## Author

Oleksandra Ryshniak, TARpv24, 2026
