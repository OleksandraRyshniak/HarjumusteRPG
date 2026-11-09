# Harjumuste RPG (Sproutly)

A gamified habit tracker. Users add habits (every day or on selected weekdays), mark them as done and earn XP, level up, build daily streaks and unlock achievements. Friends are shown on a live leaderboard.

## Tech stack

- ASP.NET Core Web API (.NET 10), Entity Framework Core, SQL Server
- JWT authentication with roles: User, Admin, Service
- Worker Service (checks streaks every day at 00:05)
- WebSocket (`/ws/leaderboard`) for live leaderboard updates
- .NET MAUI (MVVM) mobile client
- Web admin client (React or MVC)

## Project structure

```
HarjumusteRPG/HarjumusteRPG.API   Web API
docs/                             documentation and ER diagram
```


More projects (Worker, Mobile, Admin, Tests) will be added as the work progresses.

## Getting started

1. Clone the repository.
2. Open `HarjumusteRPG/HarjumusteRPG.sln` in Visual Studio.
3. Run the `HarjumusteRPG.API` project.

## Author

Oleksandra Ryshniak