# Changelog

Recent notable changes, newest first. Each entry maps to one commit.

## 2026-07-18

- **Optimistic concurrency on wallets** — PostgreSQL's built-in `xmin` system column is now mapped as an EF Core concurrency token on `Wallets`, closing the lost-update race between concurrent plays/transfers on the same wallet. A conflicting save now returns **409 Conflict** ("modified by another operation — please retry") instead of silently overwriting. No new column; the migration is metadata-only. Postgres-specific, so the mapping is skipped on the InMemory test provider. README's concurrency limitation replaced with a design-decision section.
- `56c1090` — Game changes, in two parts:
  - **Manual bet input** — the bet amount is a free-entry field again; the preset chips (10/50/100/250) now quick-fill the same field. The flip button stays disabled for a zero, negative, or over-balance bet.
  - **Practice play removed end-to-end** — the API previously still accepted a no-stake round paying a flat +5 FLIP even though the UI no longer offered it. A stake is now required and must be positive (enforced at the API boundary, in the use case, and in the domain), the `Reward` transaction type is gone, and the admin Game Rounds page no longer shows "practice" rounds. Tests, README, and CLAUDE.md updated to match.
- `0d78d8d` — Cleanup pass: removed the template `.http` file, tightened README wording, added the repository URL.
- `1802726` — Client polish: themed form labels, admin-only navigation, restyled admin pages, clearer seed-account presentation.
- `8f285de` — Pre-submission polish: unified EF Core package versions, made comments timeless, switched to a generic dev password.
- `67fc199` — README accuracy fixes: documented the `Apply` choke point, clarified practice-play scope, removed a stray heading.
- `bfaf637` — Moved the client to port 8090 and the API to port 8095.
- `5ff0e2b` — Added a beginner-friendly HOW_TO_RUN guide.

## 2026-07-17

- `b5b0f93` — Added the docker-compose stack: PostgreSQL, API, nginx-served client.
