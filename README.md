# Flipcoin

A mini crypto-gaming platform. Players hold a wallet of a fictional cryptocurrency (**FLIP**), earn coins by playing a server-side game of chance (a coin flip), and transfer coins to other players' wallet addresses. A separate **Admin** role has read-only audit views over all wallets, transactions, and game rounds.

The emphasis is **banking-grade discipline around money movement**: the server is authoritative, money is always `decimal`, balances can never go negative, transfers are atomic, and every balance change is recorded as an immutable ledger entry.

---

## Features

- **Auth** — register / login with JWT bearer tokens and role claims (Player / Admin).
- **Wallet** — balance, generated address (`FLIP-xxxxxxxx`), and paged transaction history. A user can only ever access their own wallet.
- **Game (the Flip)** — choose heads/tails, optionally stake FLIP. The server decides the outcome with a cryptographic RNG. A staked win pays 2× the stake; a practice (no-stake) win pays a flat reward (supported by the API; the UI always plays staked).
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
  Flipcoin.Domain          Entities, domain rules/exceptions. References nothing.
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
git clone https://github.com/vasilisav1991/FlipCoin.git
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

### Server-authoritative money & game outcomes

The client never decides anything that matters. It only sends *intent* — "play heads with a stake of 10", "transfer 25 to this address" — and the server validates the request, flips the coin, moves the money, and returns the result. Anything computed in the browser (a balance, a coin flip, a payout) can be tampered with in dev tools, so the client is treated purely as a display layer. This is the single most important rule in the project; every other decision below supports it.

### Ledger-style history

Every balance change — game stake, payout, reward, transfer in, transfer out — writes an immutable `Transaction` row that also records `BalanceAfter`, the wallet's balance immediately after the change. Balances are therefore always explainable: you can replay any wallet's history and verify that each entry's `BalanceAfter` follows from the previous one. Transactions are never updated or deleted, which is what makes the admin audit log trustworthy.

### Ownership by construction

No endpoint accepts a user id or wallet id from the client. The current user is always resolved from the JWT `sub` claim, and handlers load *that user's* wallet. This means "user A cannot read user B's wallet" is not an `if` check that someone could forget — there is simply no input through which B's wallet can be requested. This property is pinned down by the flagship integration test.

### Atomic transfers

A transfer touches four things: debit the sender, credit the recipient, and write both ledger entries (`TransferOut` + `TransferIn`). The handler stages all four changes and commits them with a **single `SaveChanges`**, which EF Core wraps in one database transaction — so the transfer either fully happens or doesn't happen at all. There is no code path where money leaves one wallet without arriving in the other.

### Guid keys generated in the domain (`ValueGeneratedNever`)

Entities create their own `Guid` ids in their constructors instead of relying on database-generated keys. Two reasons: Guids are non-enumerable (an attacker can't iterate `/wallets/1`, `/wallets/2`, …), and a domain object is fully valid — with an identity — the moment it's constructed, before it ever touches the database. `ValueGeneratedNever()` tells EF Core the key is always supplied by the application, so EF correctly treats entities with a set key as *inserts* rather than assuming a non-empty key means the row already exists.

### Clean Architecture dependency rule

Dependencies point strictly inward: `Api → Infrastructure → Application → Domain`, and Domain references nothing. The Application layer defines the interfaces it needs (`IWalletRepository`, `IUnitOfWork`, `ICoinFlipper`, `IJwtTokenGenerator`) and Infrastructure implements them. The payoff is that all business rules — transfer rules, payout math, balance invariants — live in code that knows nothing about EF Core, HTTP, or JWTs, which makes them trivially unit-testable (the game tests swap in a deterministic coin flipper) and keeps controllers thin.

### Validation at two layers

FluentValidation runs at the API boundary and rejects malformed requests early with clear 400 responses ("amount must be greater than 0"). But the domain enforces its own invariants too — every debit on `Wallet` goes through a single private `Apply` method that throws `InsufficientBalanceException` regardless of what any validator said. Boundary validation is for good error messages; domain invariants are the actual safety net. No future code path (a new endpoint, a background job) can create a negative balance, because the rule lives in the one place all paths go through.

### Cryptographic RNG for the flip

The coin flip uses `RandomNumberGenerator.GetInt32(2)` rather than `Random`. `Random` is a deterministic pseudo-random generator — with enough observed outputs its future values can be predicted, which is unacceptable when the outcome pays out money. The cryptographic RNG is unpredictable by design. The same generator is used for wallet addresses. The cost (slightly slower than `Random`) is irrelevant at one call per game round.

### Practice reward rule

Practice play (no stake) needed an interpretation: I chose *win → flat +5 FLIP, loss → nothing*, so practice still has a real coin flip and a reason to care about the outcome, while staked play keeps the meaningful risk (2× payout vs. losing the stake). Practice play is effectively a small faucet, so in production it would be rate-limited — noted in [Limitations](#limitations--with-more-time).

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
