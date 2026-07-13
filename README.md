# Products

[![CI](https://github.com/sameen-dev/zentek-products-api/actions/workflows/ci.yml/badge.svg)](https://github.com/sameen-dev/zentek-products-api/actions/workflows/ci.yml)

A .NET 8 Web API for managing a product catalogue, plus a React frontend that consumes it.

- **Anonymous**: `GET /health`
- **Secured (JWT bearer)**: create a product, list all products, list products filtered by colour
- Unit + integration tests
- [Architecture diagram](docs/architecture.md) showing this service in a distributed, event-driven system alongside Orders/Payments

## Quickstart (for reviewers)

> **Platform note:** the database is SQL Server **LocalDB**, which is **Windows-only**. If you're
> reviewing from Mac/Linux, you can't run the DB-backed parts locally without swapping the
> connection string for a containerized SQL Server (not set up in this repo) — but you don't have
> to take it on faith either: the CI badge at the top of this page is a real `windows-latest`
> [GitHub Actions run](https://github.com/sameen-dev/zentek-products-api/actions/workflows/ci.yml)
> that builds, migrates a fresh LocalDB database, and passes all 31 tests on every push.

On Windows, here's the exact path from a fresh clone to a working app in your browser:

1. **Install prerequisites** (skip any you already have):
   - [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (a newer SDK with the 8.0 runtime also works — `dotnet --list-sdks` / `--list-runtimes` to check)
   - **SQL Server Express LocalDB** — ships with Visual Studio, or grab the standalone [SQL Server Express installer](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) and choose the LocalDB feature. Verify with:
     ```powershell
     sqllocaldb info
     ```
     You should see `MSSQLLocalDB` listed. If the list is empty, run `sqllocaldb create MSSQLLocalDB` once.
   - [Node.js 20+](https://nodejs.org/) and npm (for the frontend)
   - Git

2. **Clone the repo:**
   ```bash
   git clone https://github.com/sameen-dev/zentek-products-api.git
   cd zentek-products-api
   ```

3. **Trust the local HTTPS dev certificate** (one-time, lets your browser trust `https://localhost:7100`):
   ```bash
   dotnet dev-certs https --trust
   ```

4. **Restore, build, and create the database:**
   ```bash
   dotnet restore
   dotnet build
   dotnet ef database update --project src/Products.Infrastructure --startup-project src/Products.Api
   ```
   That last command applies the EF Core migration and creates `ProductsDb` on your LocalDB
   instance right now, so you can inspect it in SSMS/Azure Data Studio immediately (server name
   `(localdb)\MSSQLLocalDB`) even before you run the app. If you skip this step, the API creates it
   automatically on first run anyway (`ApplyMigrationsOnStartup: true`) — this just lets you see it
   sooner. If `dotnet ef` isn't found, install the tool once: `dotnet tool install --global dotnet-ef`.

5. **Run the API** (leave this terminal running):
   ```bash
   dotnet run --project src/Products.Api
   ```
   It listens on `https://localhost:7100` / `http://localhost:5100`. A browser tab opens
   automatically to Swagger UI (`/swagger`) — try `POST /api/auth/token` with `demo` / `Demo123!`,
   click **Authorize** (padlock icon) and paste `Bearer <accessToken>`, then try the product
   endpoints directly from there if you want to test the API in isolation.

6. **Run the tests** (in a second terminal — the API doesn't need to be running for this):
   ```bash
   dotnet test
   ```
   All 31 tests should pass (19 unit, 12 integration — the integration tests spin up and tear down
   their own disposable LocalDB database, separate from `ProductsDb`).

7. **Run the frontend** (in a third terminal):
   ```bash
   cd frontend/products-ui
   npm install
   npm run dev
   ```
   Open the printed URL (`http://localhost:5173`), sign in with `demo` / `Demo123!`, create a
   product, and use the colour filter dropdown to confirm filtering works.

8. **(Optional) See the data land in the DB** — connect SSMS/Azure Data Studio to
   `(localdb)\MSSQLLocalDB` (Windows Authentication) and run `SELECT * FROM ProductsDb.dbo.Products;`
   after creating a product through the UI or Swagger.

That's the whole loop — clone → trust cert → restore/build/migrate → run API → run tests → run
frontend → click through it. Everything below goes into more depth on each of these pieces.

## Solution layout

```
Products.sln
src/
  Products.Domain          Product entity, ProductColour enum
  Products.Application      DTOs, IProductService, FluentValidation validators, mapping
  Products.Infrastructure   EF Core DbContext, repository, JWT issuing, migrations
  Products.Api               Program.cs, minimal-API endpoints, auth/CORS/Swagger/rate limiting
tests/
  Products.UnitTests         xUnit + Moq + FluentAssertions
  Products.IntegrationTests  xUnit + WebApplicationFactory against a disposable LocalDB database
frontend/
  products-ui                 Vite + React + TypeScript
docs/
  architecture.md             Diagram + write-up
```

## Prerequisites

- .NET 8 SDK (or newer SDK with the 8.0 runtime installed)
- SQL Server LocalDB (`sqllocaldb info` should list `MSSQLLocalDB`) — ships with Visual Studio / SQL Server Express LocalDB
- Node.js 20+ and npm (for the frontend)
- `dotnet dev-certs https --trust` run at least once, so the browser trusts the API's local HTTPS certificate

## Running the API

```bash
dotnet restore
dotnet run --project src/Products.Api
```

On first run the app applies EF Core migrations automatically (`ApplyMigrationsOnStartup: true` in
`appsettings.json`) and creates the `ProductsDb` database on your LocalDB instance. In a real
production pipeline, migrations would be applied as a separate deployment step rather than at
app startup — this is a pragmatic shortcut for local/demo use.

The API listens on `https://localhost:7100` and `http://localhost:5100` (see
`src/Products.Api/Properties/launchSettings.json`). Swagger UI is available at `/swagger` and lets
you exercise every endpoint directly, including the padlock for pasting in a bearer token.

### Demo credentials

The API ships with a single demo account (hashed, not stored in plaintext) for obtaining a JWT:

```
POST /api/auth/token
{ "username": "demo", "password": "Demo123!" }
```

Use the returned `accessToken` as `Authorization: Bearer <token>` on the product endpoints.

> The JWT signing key and demo password hash in `appsettings.Development.json` are placeholders
> clearly marked dev-only. A real deployment would source these from a secret store (Azure Key
> Vault, AWS Secrets Manager, etc.) or environment variables, never from a committed file.

### Endpoints

| Method | Path                          | Auth | Notes |
|--------|-------------------------------|------|-------|
| GET    | `/health`                     | none | liveness check |
| POST   | `/api/auth/token`              | none | demo login → JWT |
| POST   | `/api/products`                | JWT  | create a product |
| GET    | `/api/products`                | JWT  | list all products |
| GET    | `/api/products?colour=Red`     | JWT  | list products of one colour |
| GET    | `/api/products/{id}`           | JWT  | fetch a single product |

Valid colours: `Black, White, Red, Blue, Green, Yellow, Silver, Grey, Other`.

## Running the tests

```bash
dotnet test
```

This runs both:
- **`Products.UnitTests`** — service, validator and JWT-issuing logic against mocks, no I/O.
- **`Products.IntegrationTests`** — boots the real ASP.NET Core pipeline via
  `WebApplicationFactory<Program>` against a uniquely-named LocalDB database created fresh and
  dropped per test run, covering the anonymous health check, unauthenticated 401s, and the full
  create → list → filter-by-colour → validation/conflict flows.

Both projects require LocalDB to be reachable (the same instance the API itself uses).

## Running the frontend

```bash
cd frontend/products-ui
npm install
npm run dev
```

Open the printed URL (default `http://localhost:5173`). The dev server is pre-configured
(`.env.development`) to call the API at `https://localhost:7100`, and the API's CORS policy
already allows that origin (`Cors:AllowedOrigins` in `appsettings.json`). Sign in with the demo
credentials above, then create and filter products from the UI.

`npm run build` type-checks and produces a production bundle in `dist/`.

> Manual click-through in a real browser wasn't performed as part of this build (no browser
> automation tool was available in this environment) — verification was via `npm run build`
> (type-check + bundle) and a scripted CORS/login round trip against the running API confirming
> the exact request the browser would make succeeds end-to-end.

## What a real production deployment would add

This is a self-contained demo; a production rollout of this service would additionally need:

- Secrets sourced from a vault/KMS instead of `appsettings.Development.json`
- Refresh tokens / short-lived access tokens instead of a single long-lived demo JWT
- Migrations applied via a release pipeline, not on app startup
- OpenTelemetry tracing/metrics wired to a collector, and structured log shipping
- CI (build, test, security scanning) and containerization (Dockerfile, health/readiness probes)
- A real identity provider instead of the self-issued demo login
- API versioning and a deprecation policy once external consumers exist

## Architecture

See [docs/architecture.md](docs/architecture.md) for a diagram and explanation of how this
Products service fits into a distributed, event-driven system alongside Orders, Payments and
Notifications services.
