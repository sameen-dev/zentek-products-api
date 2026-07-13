# Products

[![CI](https://github.com/sameen-dev/zentek-products-api/actions/workflows/ci.yml/badge.svg)](https://github.com/sameen-dev/zentek-products-api/actions/workflows/ci.yml)

A .NET 8 Web API for managing a product catalogue, plus a React frontend that consumes it.

- **Anonymous**: `GET /health`
- **Secured (JWT bearer)**: create a product, list all products, list products filtered by colour
- Unit + integration tests
- [Architecture diagram](docs/architecture.md) showing this service in a distributed, event-driven system alongside Orders/Payments

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
