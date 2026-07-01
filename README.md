# Auto Repair Kiosk

A compact ASP.NET Core Razor Pages application for an auto repair shop kiosk. Employees can sign in, manage customers and vehicles, and create work orders with service and parts line items.

## Tech Stack

- .NET 10 ASP.NET Core Razor Pages
- PostgreSQL 16
- Entity Framework Core with Npgsql
- Cookie authentication with seeded employee credentials
- Docker Compose for local database setup

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or a local PostgreSQL 16 server)

## Quick Start

1. Start PostgreSQL:

   ```bash
   docker compose up -d
   ```

2. Restore and run the app:

   ```bash
   dotnet restore
   dotnet run
   ```

3. Open the URL printed by `dotnet run`, usually:

   ```
   http://localhost:5000
   ```

4. Sign in with either seeded employee account:

   | Username | Password |
   |----------|----------|
   | admin    | password |
   | tech     | wrench   |

## Configuration

The default connection string is in `appsettings.json`:

```json
"Host=localhost;Port=5432;Database=autorepair_kiosk;Username=autorepair;Password=autorepair"
```

Override it with an environment variable if needed:

```bash
# PowerShell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=autorepair_kiosk;Username=autorepair;Password=autorepair"
dotnet run
```

## Database Setup

Two setup paths are included:

- `docker-compose.yml` starts PostgreSQL and runs `database/init/01-schema-and-seed.sql` automatically on first run.
- The app also runs `DatabaseInitializer` on startup — it calls EF Core `EnsureCreatedAsync()` and inserts demo data only when tables are empty.

To fully reset the database:

```bash
docker compose down -v
docker compose up -d
```

## Project Structure

```
Data/                 EF Core DbContext, entity configuration, and startup seeding
Models/               Employee, Customer, Vehicle, WorkOrder, and WorkOrderLineItem entities
Pages/                Razor Pages for Login, Customers, Vehicles, and Work Orders
Services/             IEmployeeAuthenticator abstraction and implementation
wwwroot/css/          Kiosk-oriented styling
database/init/        PostgreSQL schema DDL and seed data script
```

## Design Decisions

**Razor Pages over a SPA framework**
The exercise called for a simple, reviewable internal tool. Razor Pages keeps the server-side logic close to the UI without introducing a separate frontend build pipeline, making the project easier to run and review out of the box.

**EF Core with PostgreSQL**
EF Core was chosen for its first-class .NET integration and because it maps naturally to the relational model here: customers own vehicles, vehicles have work orders, work orders have line items. Cascade deletes are configured for owned child records.

**Cookie authentication**
Authentication is intentionally minimal for this exercise — employee credentials are seeded directly into the database. The `IEmployeeAuthenticator` interface is abstracted so it can be swapped for ASP.NET Core Identity or an external provider without changing the page logic.

**Docker Compose + SQL seed script**
The Docker Compose file and `database/init/` script allow a reviewer to get a fully staged environment with a single command. The app also seeds on startup as a fallback for reviewers running a local PostgreSQL instance.

**Work order status lifecycle**
Status progresses as: `Pending → In Progress → Completed`. This covers the core drop-off-through-pickup workflow described in the brief. Additional statuses (e.g. Waiting on Parts, Invoiced) would be easy to add.

## Trade-offs & What I Would Add With More Time

- **User roles and permissions** — The two seeded accounts (`admin`, `tech`) demonstrate the concept, but role-based access control (e.g. restricting delete to admins only) would be a natural next step using ASP.NET Core authorization policies.
- **Stronger authentication** — Plain-text passwords are used for demo simplicity. A production version would use ASP.NET Core Identity with hashed credentials or integrate with an IdP.
- **Invoicing** — Generate a printable customer-facing invoice from a completed work order, including subtotals, tax, and totals.
- **Service history** — A dedicated view showing all past work orders for a given customer or vehicle.
- **Search and filtering** — Filter the work order list by status, customer name, or date range.
- **Automated tests** — Unit tests for the authentication service and integration tests for the EF Core data layer and page handlers.
- **Audit trail** — Track created/updated timestamps and the employee who made each change.
- **Dashboard** — At-a-glance metrics: open work orders, revenue this week, vehicles currently in shop.
- **API layer** — Expose work order data via a RESTful API with OpenAPI/Swagger documentation for potential future integrations (e.g. a mobile app for mechanics).
