---
name: run
description: Launch and drive the HealthTracker app (Docker Compose - Postgres + ASP.NET Core API + React/nginx frontend) to see a change working, or to take a screenshot. Use whenever asked to run, start, or screenshot the app, or to confirm a change works for real.
---

# Running HealthTracker

Full stack via Docker Compose, from the repo root. Requires Docker Desktop to be
running first (if `docker ps` fails with a `dockerDesktopLinuxEngine` pipe error,
start Docker Desktop and wait ~10-30s for the whale icon to settle, then retry).

## Start / stop

```bash
cd /c/Projects/HealthTracker      # repo root, where docker-compose.yml lives
docker compose up --build
```

Stop: `docker compose down`. Never add `-v` unless explicitly asked to wipe data -
it deletes the Postgres volume (`pgdata`) and the Data Protection key volume
(`apikeys`), the latter of which logs every user out permanently (see gotcha below).

Ports: frontend `http://localhost:4200`, API `http://localhost:8080` (+ `/swagger`
in Development), Postgres `5432`.

## CRITICAL gotcha: the frontend does NOT hot-reload

The `frontend` container is a **static nginx build**, not a dev server - it does
not watch the source tree. Editing anything under `frontend/src/` has **zero
effect** on the running container until the image is rebuilt. Worse, a plain
`docker compose up --build` can silently reuse a stale cached layer and produce
a container that *looks* rebuilt but still serves old code (observed in practice).

The reliable sequence after **any** frontend source change:

```bash
docker compose build --no-cache frontend
docker compose up -d frontend
```

Then verify the container actually changed before testing anything:

```bash
docker ps --format "table {{.Names}}\t{{.CreatedAt}}\t{{.Status}}"
```

`healthtracker-frontend`'s `CREATED AT` must be newer than your last edit. If it's
unchanged, the rebuild didn't really take - don't trust the test that follows.

The same applies to backend changes (`src/HealthTracker.Api`,
`src/HealthTracker.Modules.*`) and the `api` service.

## Auth - there is no seeded user

Register one directly through the API (no UI needed for this step):

```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"screenshot@example.com","password":"Screenshot123!"}'
```

Password must satisfy ASP.NET Core Identity's default policy (upper + lower +
digit + non-alphanumeric, 6+ chars). `Screenshot123!` is known-good and safe to
reuse across sessions - it's a disposable local/test account, not a real user.

## Driving the UI (Playwright, headless Chromium)

No `chromium-cli` / browser MCP tool is available in this environment - drive it
with Playwright directly. Install once into the scratchpad directory, **never**
into the project's own `node_modules` (don't pollute `frontend/package.json`):

```bash
cd <scratchpad dir>
npm init -y && npm install playwright@1.64.1
npx playwright install chromium
```

Login, then navigate client-side (routes are React Router, not full page loads -
after login, `click` the nav link rather than `page.goto`-ing a protected route
directly, otherwise `RequireAuth` bounces you to `/login` before the SPA reads
the token from `localStorage`):

```js
const { chromium } = require('playwright');
(async () => {
  const browser = await chromium.launch();
  const page = await browser.newPage({ viewport: { width: 1440, height: 900 } });
  await page.goto('http://localhost:4200/login', { waitUntil: 'networkidle' });
  await page.fill('input[type="email"]', 'screenshot@example.com');
  await page.fill('input[type="password"]', 'Screenshot123!');
  await page.click('button[type="submit"]');
  await page.waitForURL('**/', { timeout: 15000 }).catch(() => {});
  await page.click('a:has-text("Schedule")');   // or "Water" / "Calories"
  await page.screenshot({ path: 'out.png', fullPage: true });
  await browser.close();
})();
```

## Known, non-blocking bug (don't mistake it for a regression)

A brand-new user's **first** page load can trigger a real race condition:
`WaterTracker`'s `getSummary()` and `getReminder()` fire concurrently and both
try to `INSERT` the same `water.settings` row - one succeeds, one hits
`23505 duplicate key`, surfacing as a `500` in the browser console. It's
swallowed by the page's own `.catch(console.error)`, harmless to the UI, and
does not recur once that user's settings row exists (i.e. after the first
load). Pre-existing, not something you broke.

## Related: Data Protection key persistence

Login tokens are only valid against the key that signed them. The API
container persists its Data Protection key ring to a named volume (`apikeys`)
specifically so tokens survive container restarts - don't delete that volume
casually (`docker compose down -v`), and if users unexpectedly get logged out
after a restart, check whether `apikeys` still exists (`docker volume ls`)
before assuming the code regressed.
