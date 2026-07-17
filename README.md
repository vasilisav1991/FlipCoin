# Flipcoin

A mini crypto-gaming platform. Players hold a wallet of a fictional cryptocurrency (**FLIP**), earn coins by playing a server-side game of chance (a coin flip), and transfer coins to other players' wallet addresses. A separate **Admin** role has read-only audit views over all wallets, transactions, and game rounds.

The emphasis is **banking-grade discipline around money movement**: the server is authoritative, money is always `decimal`, balances can never go negative, transfers are atomic, and every balance change is recorded as an immutable ledger entry.

---

## Features

- **Auth** — register / login with JWT bearer tokens and role claims (Player / Admin).
- **Wallet** — balance, generated address (`FLIP-xxxxxxxx`), and paged transaction history. A user can only ever access their own wallet.
- **Game (the Flip)** — choose heads/tails, optionally stake FLIP. The server decides the outcome with a cryptographic RNG. A staked win pays 2× the stake; a practice (no-stake) win pays a flat reward.
- **Transfers** — atomic wallet-to-wallet transfers with a full double-entry ledger.
- **Admin** — audit views of all wallets, a filterable global transaction log, and all game rounds, gated by the Admin role.
- **Real-time updates** — the API pushes balance changes to the owning user over SignalR, so a recipient sees an incoming transfer (and a player sees a game result) update live, without refreshing.
- **Blazor WebAssembly client** — a standalone SPA that talks to the API over HTTP.

---

## Tech stack

| Area | Choice |
| --- | --- |
| Backend | .NET 10 Web API (C#) |
| Data | EF Core + PostgreSQL |
| Frontend | Blazor WebAssembly (standalone) |
| Real-time | SignalR (server → client wallet updates) |
| Auth | JWT with role claims; passwords hashed with ASP.NET Core `PasswordHasher<T>` |
| Validation | FluentValidation |
| Logging | Serilog (structured, console) |
| API docs | Swagger / OpenAPI |
| Tests | xUnit + Moq (unit), `WebApplicationFactory` (integration) |
| Containers | multi-stage Dockerfiles + docker-compose (postgres, api, client) |

---

## Architecture

Clean Architecture with the dependency rule pointing strictly inward.

```
src/
  Flipcoin.Domain          Entities, value objects, domain rules/exceptions. References nothing.
  Flipcoin.Application     Use cases (handlers), repository & service interfaces, DTOs. References Domain.
  Flipcoin.Infrastructure  EF Core (DbContext, configurations, migrations, repositories), auth, RNG. References Application.
  Flipcoin.Api             Controllers, request DTOs, middleware, DI, Swagger. References Application + Infrastructure.
  Flipcoin.Client          Blazor WASM app. Talks to the API over HTTP only.
tests/
  Flipcoin.UnitTests         Domain + application tests (Moq for interfaces).
  Flipcoin.IntegrationTests  WebApplicationFactory endpoint tests (EF in-memory provider).
```

The Domain project compiles with zero package references. The Application layer defines interfaces (`IUserRepository`, `IJwtTokenGenerator`, `ICoinFlipper`, …); Infrastructure implements them.

---

## Quick start (Docker — recommended)

The only prerequisite is **Docker** (Docker Desktop on Windows/macOS, in Linux-containers mode). No .NET SDK, no PostgreSQL install.

> New to Docker? **[HOW_TO_RUN.md](HOW_TO_RUN.md)** is a step-by-step guide that assumes nothing — install, run, log in, reset, troubleshooting.

```bash
git clone <this repo>
cd FlipCoin
docker compose up --build
```

Then open **http://localhost:8090** and log in with a [seed account](#seed-accounts) (e.g. `player1@flipcoin.local` / `Password123!`).

The compose stack runs three services:

| Service | Image | URL |
| --- | --- | --- |
| `client` | Blazor WASM built by the .NET SDK, served by nginx | http://localhost:8090 |
| `api` | multi-stage .NET build → ASP.NET runtime | http://localhost:8095 (Swagger at `/swagger`) |
| `postgres` | `postgres:17-alpine`, named volume, healthcheck | not exposed to the host |

On startup the API waits for Postgres to be healthy, applies the EF Core migrations, and seeds the demo accounts (idempotent). Wallet data survives restarts via the named volume; `docker compose down -v` resets everything.

---

## Running locally without Docker

### Prerequisites

- **.NET 10 SDK** (built with `10.0.301`).
- **PostgreSQL** running locally and reachable (default `localhost:5432`).

### Getting started

### 1. Configure the database connection

The API reads its connection string from `src/Flipcoin.Api/appsettings.Development.json` under `ConnectionStrings:Postgres`. Update it to match your local PostgreSQL:

```json
"ConnectionStrings": {
  "Postgres": "Host=localhost;Port=5432;Database=flipcoin;Username=postgres;Password=YOUR_PASSWORD"
}
```

> The dev connection string and JWT signing key live in `appsettings.Development.json` for convenience. In production these would come from user-secrets / environment variables / a secrets manager (see [Limitations](#limitations--with-more-time)).

### 2. Run the API

```bash
dotnet run --project src/Flipcoin.Api --launch-profile http
```

- API base URL: **http://localhost:8095**
- Swagger UI: **http://localhost:8095/swagger**
- Health check: **http://localhost:8095/health**

On startup the API **applies any pending migrations** (creating the `flipcoin` database on first run) and **seeds the demo accounts** (idempotent — nothing happens if users already exist).

### 3. Run the client

In a second terminal:

```bash
dotnet run --project src/Flipcoin.Client --launch-profile http
```

Open **http://localhost:5028** and log in.

> The client's API base URL is set in `src/Flipcoin.Client/wwwroot/appsettings.json` (`ApiBaseUrl`), and the API allows the client origin via `Cors:AllowedOrigins` in `appsettings.Development.json`. If you change either port, update both.

### Seed accounts

All seeded accounts share the password **`Password123!`**.

| Email | Role | Starting balance |
| --- | --- | --- |
| `admin@flipcoin.local` | Admin | — (admins hold no funds) |
| `player1@flipcoin.local` | Player | 100 FLIP |
| `player2@flipcoin.local` | Player | 100 FLIP |

---

## API surface

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | anonymous | Create a player + wallet (100 FLIP). |
| POST | `/api/auth/login` | anonymous | Returns a JWT. |
| GET | `/api/wallet` | player | My wallet (balance, address). |
| GET | `/api/wallet/transactions?page&pageSize` | player | My transaction history (paged). |
| POST | `/api/game/play` | player | `{ choice, stake? }` → server decides → result + new balance. |
| POST | `/api/transfers` | player | `{ toAddress, amount }` → atomic transfer. |
| GET | `/api/admin/wallets` | admin | All users + balances. |
| GET | `/api/admin/transactions?type&page&pageSize` | admin | Global audit log (filterable). |
| GET | `/api/admin/game-rounds?page&pageSize` | admin | All game rounds. |
| GET | `/health` | anonymous | Liveness check. |

Errors are returned as RFC 7807 **ProblemDetails** via global exception handling, with appropriate status codes (400 validation, 401, 403, 404, 409).

---

## Running the tests

```bash
dotnet test
```

- **Unit tests** — registration/login rules, transfer rules, and the game payout math (with a deterministic coin flipper).
- **Integration tests** — hosted with `WebApplicationFactory` over an EF in-memory database, including the flagship ownership test (**a user can only ever reach their own wallet**), transfer happy-path / insufficient-balance, admin role enforcement, and request validation.

---

## Configuration reference

| Setting | Location | Purpose |
| --- | --- | --- |
| `ConnectionStrings:Postgres` | `Flipcoin.Api/appsettings.Development.json` | PostgreSQL connection string. |
| `Jwt:Key` / `Issuer` / `Audience` / `ExpiryMinutes` | `Flipcoin.Api/appsettings.Development.json` | JWT signing + validation. |
| `Cors:AllowedOrigins` | `Flipcoin.Api/appsettings.Development.json` | Origins allowed to call the API (the client). |
| `ApiBaseUrl` | `Flipcoin.Client/wwwroot/appsettings.json` | API base URL used by the client. |

---

## Design decisions

<!--
  This section is for the author to complete in their own words. Suggested
  topics to cover (each was a real decision point during the build):
-->

- **Server-authoritative money & game outcomes** — _todo: why the client never decides anything._
- **Ledger-style history** — _todo: every balance change writes an immutable Transaction with BalanceAfter._
- **Ownership by construction** — _todo: user id comes only from the JWT subject; no endpoint accepts a user/wallet id, so "A can't read B's wallet" holds structurally._
- **Atomic transfers** — _todo: debit + credit + both ledger entries in a single SaveChanges = one DB transaction._
- **Guid keys generated in the domain (`ValueGeneratedNever`)** — _todo: non-enumerable ids + why this was needed for correct EF insert tracking._
- **Clean Architecture dependency rule** — _todo: why interfaces live in Application and implementations in Infrastructure._
- **Validation at two layers** — _todo: FluentValidation at the boundary + domain invariants as defense-in-depth._
- **Cryptographic RNG for the flip** — _todo: `RandomNumberGenerator` vs `Random`._
- **Practice reward rule** — _todo: chosen interpretation (practice win → +5, loss → 0)._

---

## Limitations / with more time

These were conscious scope choices, noted for production readiness:

- **Secrets in config** — the dev DB password and JWT key are in `appsettings.Development.json`. Production should use user-secrets / environment variables / a secrets manager.
- **Minimal auth** — no refresh tokens or token revocation; a single short-lived access token by design.
- **Concurrency** — wallet balance has no optimistic-concurrency token. Concurrent money movement on the same wallet should use a `rowversion`/`xmin` concurrency token (and/or row locking).
- **Rate limiting** — practice play is a coin faucet; it should be rate-limited.
- **Migrations on startup** — the API auto-applies migrations when it starts, which keeps the demo one-command. With multiple API instances this is a race; production would apply migrations as a deploy step instead.
- **Integration test database** — tests use the EF in-memory provider. Testcontainers with real PostgreSQL would be more faithful.
- **Pagination** — offset-based; keyset pagination would scale better for large audit logs.
