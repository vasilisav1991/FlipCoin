# Flipcoin — Project Plan & Working Agreement

This document gives you (the AI assistant) full knowledge of the system we are building and the rules of how we work together. Read it fully before doing anything.

---

## PART 1 — HOW WE WORK (read this as binding rules)

You are my **assistant**, not the driver. I review, approve, and take ownership of every change that goes into this project. Therefore:

1. **One step at a time.** We work strictly through the phases in Part 3, one task at a time. Never build ahead, never scaffold future phases, never add "while I'm at it" extras. If a task is done, stop and wait for my go-ahead.
2. **Explain before you write.** Before implementing anything non-trivial, give me a 3–5 line summary: what you're about to do, the key decision involved, and any alternative worth knowing about. If I say proceed, implement it.
3. **Small diffs.** Prefer several small, reviewable changes over one large one. If a change touches more than ~3 files, tell me why before doing it.
4. **Document non-obvious choices.** When you use a pattern, API, or technique new to this project, add a one-paragraph explanation of how it works and why it's the right choice here, so the reasoning is on record.
5. **Ask before adding dependencies.** No new NuGet packages or libraries without proposing them first with a one-line justification. Default to the standard library and what's already installed.
6. **Advise, don't override.** If you think my instruction is a mistake, say so and explain why — briefly, once. If I confirm, do it my way.
7. **Simplicity is a requirement.** When choosing between a clever solution and a boring one, choose boring. This codebase values clarity over sophistication.
8. **No placeholder/dead code.** Everything committed must be used and working. No TODO stubs for features we haven't reached — the phase plan is our TODO list.
9. **When something fails,** show me the actual error, your diagnosis, and your proposed fix — then wait for my confirmation before applying it.
10. **Checkpoint reviews.** At the end of each phase there is a CHECKPOINT. Summarize what was built, list the key decisions made, and ask me to review before we continue.

---

## PART 2 — SYSTEM KNOWLEDGE

### What we are building
**Flipcoin**: a mini crypto-gaming platform. Users hold a wallet of a fictional cryptocurrency (FLIP), earn coins by playing a simple server-side game of chance, and can transfer coins to other users' wallet addresses. There is a Player role and an Admin role with an audit view. The code must demonstrate banking-grade discipline around money movement.

### Non-negotiable design principles
- **Server-authoritative everything.** The client never decides game outcomes or money movement. It sends intent (play, stake, transfer); the server validates, decides, records, and returns results.
- **Money is `decimal`.** Never float/double for amounts, anywhere.
- **Balances can never go negative.** Enforced in the domain layer, not just in validation or UI.
- **Transfers are atomic.** Debit + credit + transaction records happen in one database transaction — all or nothing.
- **Ledger-style history.** Every balance change produces an immutable Transaction record (types: TransferIn, TransferOut, Stake, Payout).
- **Users can only access their own wallet/data.** Ownership enforced server-side on every endpoint. Admin role sees all, holds no funds, cannot play.
- **Game RNG uses `RandomNumberGenerator`** (cryptographic), generated server-side per round.
- **No real blockchain.** Wallet address is a generated identifier (e.g. `FLIP-7f3a9c21`), unique per wallet.

### Stack (locked — do not propose alternatives)
- Backend: **.NET 10** Web API, C#
- Data: **EF Core + PostgreSQL** (in Docker)
- Frontend: **Blazor WebAssembly** (standalone, talks to API over HTTP)
- Auth: **JWT** with role claims; password hashing via ASP.NET Core `PasswordHasher<T>`; minimal by design (no refresh tokens — documented as a production improvement)
- Validation: **FluentValidation**
- Logging: **Serilog** (structured, console sink)
- API docs: **Swagger/OpenAPI**
- Tests: **xUnit + Moq**, integration tests via `WebApplicationFactory`
- Containers: multi-stage Dockerfiles + one **docker-compose** (api, client, postgres) — `docker compose up` must run everything

### Architecture (Clean Architecture, 4 projects + client + tests)
```
src/
  Flipcoin.Domain          — entities, domain rules/exceptions. References NOTHING.
  Flipcoin.Application     — use cases (services/handlers), repository & service INTERFACES,
                                validators, DTOs used by use cases. References Domain only.
  Flipcoin.Infrastructure  — implementations: Persistence/ (DbContext, repositories, migrations),
                                Auth/ (JWT issuance, password hashing). References Application.
  Flipcoin.Api             — controllers, request/response DTOs, middleware, DI wiring, Swagger.
                                Thin: translates HTTP <-> use cases. References Application + Infrastructure.
  Flipcoin.Client          — Blazor WASM app. Talks to Api over HTTP only.
tests/
  Flipcoin.UnitTests         — domain + application tests (Moq for interfaces)
  Flipcoin.IntegrationTests  — WebApplicationFactory endpoint tests
```
- The dependency rule is absolute: dependencies point inward. Domain compiles with zero package references. Application defines interfaces; Infrastructure implements them.
- Persistence is a FOLDER inside Infrastructure, not a fifth project.
- Controllers contain no business logic — validation + use case call + result mapping only.

### Domain model (initial — we may refine at checkpoints)
- **User**: Id, Email (unique), PasswordHash, Role (Player|Admin), CreatedAt
- **Wallet**: Id, UserId (1:1), Address (unique, generated), Balance (decimal)
- **Transaction**: Id, WalletId, Type (TransferIn|TransferOut|Stake|Payout), Amount, CounterpartyAddress (nullable), Timestamp, BalanceAfter
- **GameRound**: Id, UserId, Stake, Choice, Outcome, Won (bool), Payout, PlayedAt

### API surface (target)
```
POST /api/auth/register        — create user + wallet with starting balance (100 FLIP)
POST /api/auth/login           — returns JWT
GET  /api/wallet               — my wallet (balance, address)
GET  /api/wallet/transactions  — my transaction history (paged)
POST /api/game/play            — { stake, choice } -> server decides -> result + new balance
POST /api/transfers            — { toAddress, amount } -> atomic transfer
GET  /api/admin/wallets        — [Admin] all users + balances
GET  /api/admin/transactions   — [Admin] global audit log (filterable)
GET  /api/admin/game-rounds    — [Admin] all game rounds
```
- Proper status codes: 200/201, 400 (validation), 401, 403 (role/ownership), 404, 409 where it genuinely fits.
- Errors returned as ProblemDetails via global exception-handling middleware.

### Game rules (the Flip)
- Choice: heads/tails. Stake required (must be > 0 and <= balance).
- A win pays 2x stake; a loss forfeits the stake.
- Server records the round and writes the corresponding Transaction(s) atomically with the balance change.

### Transfer rules
- amount > 0; sender balance >= amount; recipient address exists; sender != recipient.
- One DB transaction: debit sender, credit recipient, write TransferOut + TransferIn records with BalanceAfter.

---

## PART 3 — PHASED BUILD PLAN (we execute this top to bottom, together)

### Phase 0 — Skeleton & environment
0.1 Create solution + project structure exactly as in Part 2, with project references enforcing the dependency rule.
0.2 docker-compose with postgres (named volume, healthcheck) + Api service; Api multi-stage Dockerfile.
0.3 Serilog + Swagger + a /health endpoint. Verify `docker compose up` works end to end.
**CHECKPOINT 0** — I review structure and compose setup.

### Phase 1 — Domain & persistence
1.1 Domain entities + domain exceptions (e.g. InsufficientBalanceException) + wallet address generator.
1.2 EF Core DbContext, entity configurations (decimal precision, unique indexes on Email and Address), initial migration.
1.3 Repository interfaces in Application; EF implementations in Infrastructure/Persistence.
1.4 Seed data: 1 admin, 2 players with wallets and starting balances.
**CHECKPOINT 1** — I review the model and the migration SQL.

### Phase 2 — Auth & ownership
2.1 Register use case (creates user + wallet atomically), Login use case (JWT with role claim).
2.2 JWT setup in Api; [Authorize] baseline; role policy for admin endpoints.
2.3 Wallet endpoints (GET wallet, GET transactions) with ownership enforced from the JWT subject — never from a client-supplied user id.
2.4 Unit tests: registration, login failure paths. Integration test: **user A cannot read user B's wallet** (this is the flagship test of the project).
**CHECKPOINT 2** — I review the auth flow and we discuss the ownership enforcement approach.

### Phase 3 — The money & the game (the heart of the system)
3.1 Transfer use case with full rules, atomic via DB transaction. Unit tests for every rule; integration test for the happy path + insufficient balance.
3.2 Game play use case: validation, crypto RNG, outcome, payout math, atomic persistence of round + transactions + balance.
3.3 Unit tests: stake > balance rejected, payout math, transaction records written with correct BalanceAfter.
**CHECKPOINT 3** — I review the transaction boundaries and we discuss concurrency (and note optimistic concurrency as a documented improvement).

### Phase 4 — Admin & API polish
4.1 Admin endpoints (wallets, audit log with filters, game rounds) behind role policy.
4.2 Global exception middleware -> ProblemDetails; FluentValidation wired into the pipeline; pagination on list endpoints.
**CHECKPOINT 4** — I review the full API surface in Swagger.

### Phase 5 — Blazor client
5.1 WASM project setup: auth state (JWT storage in memory + persistence approach discussed at checkpoint), login/register pages, HTTP client with bearer token.
5.2 Wallet page: balance, address with copy button, transaction history.
5.3 Game page: choice + stake input, play button, coin-flip animation while awaiting server result, win/loss reveal.
5.4 Transfer page + Admin pages (wallets, audit log).
**CHECKPOINT 5** — I click through everything; we fix UX rough edges.

### Phase 6 — Final hardening & delivery
6.1 Client Dockerfile + add to compose. Full clean-clone test: `git clone` -> `docker compose up` -> everything works with seeded logins.
6.2 README (run instructions, structure, and design decisions).
6.3 Final review pass together: we walk the codebase file by file and clean up anything rough.
**CHECKPOINT 6 (final)** — release readiness review.

---

## Definition of done (applies to every task)
- Compiles, tests pass, no warnings introduced.
- I have reviewed and approved the change.
- Nothing exists in the codebase that isn't used.
- Consistent style with what's already there.
