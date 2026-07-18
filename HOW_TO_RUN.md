# How to run Flipcoin (step by step)

This guide assumes you have **never used Docker before**. Follow it top to bottom — there are only three real steps: install Docker, get the code, run one command.

You do **not** need to install .NET, PostgreSQL, or any programming tools. Docker downloads and runs everything for you in isolated containers, and removing them later leaves no trace on your machine.

---

## Step 1 — Install Docker Desktop

Docker Desktop is a free application that runs the app's three components (database, server, web client) for you.

1. Download it from <https://www.docker.com/products/docker-desktop/> — the site detects your system:
   - **Windows**: run the installer, accept the defaults (it may ask to enable "WSL 2"; say yes). Restart if prompted.
   - **Mac**: open the `.dmg` and drag **Docker** into **Applications**. Choose the right download for your chip (Apple Silicon for M1/M2/M3/M4, Intel otherwise — About This Mac tells you which you have).
2. **Start Docker Desktop** (from the Start menu / Applications). The first start can take a minute.
3. Wait until the whale icon in the taskbar (Windows, bottom right) or menu bar (Mac, top right) stops animating. When the Docker Desktop window says **"Engine running"**, you're ready.

> You can close the Docker Desktop *window* — it keeps running in the background. Just don't quit it entirely while using Flipcoin.

**Windows only:** if you have used Docker before and switched it to "Windows containers", switch back: right-click the whale icon → **Switch to Linux containers…**. If you just installed it, you're already in the right mode.

---

## Step 2 — Get the code

**If you have git:**

```
git clone https://github.com/vasilisav1991/FlipCoin.git
cd FlipCoin
```

**If you don't have git:** on the repository page choose **Code → Download ZIP**, extract it somewhere, and open a terminal in the extracted folder:

- **Windows**: open the folder in File Explorer, click the address bar, type `cmd`, press Enter.
- **Mac**: open **Terminal** (Cmd+Space, type "Terminal"), type `cd `, drag the folder onto the Terminal window, press Enter.

Either way, you should now have a terminal open **in the folder that contains `docker-compose.yml`**. Check with `dir` (Windows) or `ls` (Mac) — you should see `docker-compose.yml` listed.

---

## Step 3 — Run it

In that terminal, run:

```
docker compose up --build
```

The **first run downloads and builds everything and takes a few minutes** — you'll see a long stream of log output. That's normal. Later runs take seconds.

The app is ready when the log output settles down and you see lines like:

```
flipcoin-api-1  | [INF] Starting Flipcoin API
```

Leave this terminal open — closing it (or pressing `Ctrl+C`) stops the app.

Now open your browser at:

### → **<http://localhost:8090>**

---

## Step 4 — Log in and play

The database is created with three demo accounts. **Every account has the password `Password123!`**

| Email | Role | What you can do |
| --- | --- | --- |
| `player1@flipcoin.local` | Player | Starts with 100 FLIP — play the coin flip, send FLIP |
| `player2@flipcoin.local` | Player | Starts with 100 FLIP — same as player 1 |
| `admin@flipcoin.local` | Admin | Read-only audit views: all wallets, all transactions, all game rounds |

Suggested tour:

1. Log in as **player1**. Pick Heads or Tails, choose a bet, hit **Flip the coin**.
2. Open a **second browser window in private/incognito mode** and log in as **player2** (a normal second tab would share player1's login).
3. As admin (third private window, or reuse one), open **Admin** to find player2's wallet address (it looks like `FLIP-1a2b3c4d`).
4. As player1, expand **Send FLIP**, paste player2's address, send some coins — and watch player2's balance update **instantly, without refreshing**. That's the real-time (SignalR) feature.

Also available while the app is running:

- **API documentation (Swagger)**: <http://localhost:8095/swagger> — browse and try every API endpoint.
- **Health check**: <http://localhost:8095/health>

---

## Stopping, restarting, resetting

| I want to… | Do this |
| --- | --- |
| Stop the app | Press `Ctrl+C` in the terminal, or run `docker compose down` from the project folder |
| Start it again | `docker compose up` (no `--build` needed unless the code changed) |
| Run it in the background (no log stream) | `docker compose up -d`, later `docker compose down` to stop |
| **Reset everything** (fresh database, balances back to 100) | `docker compose down -v` — the `-v` deletes the database volume; next `up` starts clean |
| Remove Flipcoin from my machine completely | `docker compose down -v --rmi all`, then uninstall Docker Desktop if you no longer want it |

Your balances and transaction history **survive** normal stops/restarts — the database lives in a Docker "volume". Only `-v` deletes it.

---

## Troubleshooting

**"Cannot connect to the Docker daemon" / "error during connect" / "docker: command not found"**
Docker Desktop isn't running (or isn't installed). Start it and wait for "Engine running", then run the command again.

**"no matching manifest for windows/amd64"** (Windows)
Docker is in Windows-containers mode. Right-click the whale icon in the taskbar → **Switch to Linux containers…** and retry.

**"port is already allocated" / "address already in use"**
Something on your machine already uses port `8090` or `8095`. Stop that program, or edit `docker-compose.yml` and change the **left** number of a port mapping (e.g. `"8081:80"` under `client`), then browse to that port instead.

**The page loads but login says "Failed to fetch"**
The API container probably isn't ready or has stopped. Check the terminal logs for `flipcoin-api-1` errors, or run `docker compose ps` — all three services should say "Up". `docker compose down -v` followed by `docker compose up --build` gives you a clean start.

**`no configuration file provided: not found`**
You're in the wrong folder. `cd` into the folder that contains `docker-compose.yml` (see Step 2) and retry.

**It's just slow the first time**
Yes — the first `docker compose up --build` downloads the .NET build tools and PostgreSQL (hundreds of MB). Every run after that reuses them and starts in seconds.
