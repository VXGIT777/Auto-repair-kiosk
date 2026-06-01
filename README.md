# Auto Repair Kiosk

A compact ASP.NET Core Razor Pages application for an auto repair shop kiosk. Employees can sign in, manage customers and vehicles, and create work orders with service/parts line items.

## Tech Stack

- .NET 10 ASP.NET Core Razor Pages
- PostgreSQL 16
- Entity Framework Core with Npgsql
- Cookie authentication with demo employee credentials stored in PostgreSQL
- Docker Compose for local database setup

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker Desktop, or a local PostgreSQL server

## Quick Start

1. Start PostgreSQL:

   ```powershell
   docker compose up -d
   ```

2. Restore and run the app:

   ```powershell
   dotnet restore
   dotnet run
   ```

3. Open the URL printed by `dotnet run`, usually:

   ```text
   https://localhost:5001
   ```

4. Sign in with either seeded employee account:

   ```text
   Username: admin
   Password: password

   Username: tech
   Password: wrench
   ```

## Configuration

The default connection string is in `appsettings.json`:

```json
"Host=localhost;Port=5432;Database=autorepair_kiosk;Username=autorepair;Password=autorepair"
```

Override it with an environment variable when needed:

```powershell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=autorepair_kiosk;Username=autorepair;Password=autorepair"
dotnet run
```

## Database Staging

Two setup paths are included:

- `docker-compose.yml` starts PostgreSQL and runs `database/init/01-schema-and-seed.sql` the first time the volume is created.
- The app also calls `DatabaseInitializer` on startup. It uses EF Core `EnsureCreatedAsync()` and inserts demo data only when tables are empty.

To reset the Docker database:

```powershell
docker compose down -v
docker compose up -d
```

## Project Structure

```text
Data/                 EF Core DbContext, entity configuration, and startup seeding
Models/               Employee, customer, vehicle, work order, and line item entities
Pages/                Razor Pages for login, customers, vehicles, and work orders
Services/             Authentication service abstraction and implementation
wwwroot/css/site.css  Kiosk-oriented styling
database/init/        PostgreSQL schema and sample data script
```

## Notes for Reviewers

- Authentication is intentionally simple for the exercise: employee usernames and passwords are stored as plain text.
- Razor Pages were chosen to keep the demo small and easy to run while still showing model binding, validation, DI, relational data access, and page-level workflows.
- EF Core relationships use cascade delete for owned child records such as customer vehicles and work order line items.
- The UI is designed for shop employees at a kiosk: dense, direct, and workflow-oriented rather than a public marketing site.
