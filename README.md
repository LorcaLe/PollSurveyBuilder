# Ballot — Poll & Survey Builder

AMD201 group coursework: a real-time poll/survey service built with **ASP.NET Core 10** (Clean Architecture),
**React 18 + Vite** (SPA), **SignalR** (live results), **SQL Server/PostgreSQL**, **Redis** (cached results),
**Docker** (multi-stage builds) and **GitHub Actions** (CI/CD to a container registry + PaaS).

Create a poll, share the link or QR code, and watch the results bar chart update on everyone's
screen the instant a vote is cast — no page refresh, no polling.

---

## 1. Live links

| | |
|---|---|
| Live app (Vercel) | https://ballote.vercel.app |
| Live API (Render) | https://ballot-api-qqxr.onrender.com |
| API docs (Scalar, dev only) | https://ballot-api-qqxr.onrender.com/scalar/v1 |


---

## 2. Architecture

```
                        ┌─────────────────────┐
                        │   React SPA (Vite)  │  <- voters, creators
                        │   hosted on Vercel   │
                        └──────────┬───────────┘
                                   │ HTTPS + WebSocket (SignalR), cross-origin (CORS)
                                   ▼
                        ┌─────────────────────┐
                        │  PollSurveyBuilder   │
                        │       .API           │  Controllers, SignalR Hub,
                        │  (ASP.NET Core 10)    │  JWT auth, rate limiting
                        │   hosted on Render    │
                        └──────────┬───────────┘
                                   │
                        ┌──────────▼───────────┐
                        │    .Infrastructure    │  EF Core, Identity,
                        │                       │  Redis cache, QR codes,
                        │                       │  background expiry job
                        └──────────┬───────────┘
                                   │
                 ┌─────────────────┼─────────────────┐
                 ▼                                     ▼
        ┌────────────────┐                   ┌────────────────┐
        │   SQL Server    │                   │      Redis      │
        │  (Azure SQL)    │                   │ (results cache) │
        └────────────────┘                   └────────────────┘

        .Domain  <-- entities/enums, no dependencies on anything else
        .Application <-- DTOs, service interfaces, FluentValidation validators
```

**Deployment topology.** The frontend (Vercel), the API (Render) and the database (Azure) live
on three different providers and three different domains. That's why voting identity is no
longer a cookie (see below) and why CORS has to be configured explicitly — see [Section 5](#5-deploying).

**Why one DbContext instead of two?** The original Clean-Architecture sample this project
was adapted from split Identity into its own DbContext. Here, `AppDbContext` combines
ASP.NET Identity tables with the Poll/PollOption/Vote tables, because every write path in
this app (cast a vote, create a poll) is small and benefits from being inside a single
`SaveChangesAsync` transaction — splitting it in two would only add cross-context
consistency risk with no real benefit at this scale.

### Data model

- **Poll** — question, type (`SingleChoice` / `YesNo` / `Rating` / `OpenText`), status,
  short public `Code`, optional `ExpiresAt`, optional creator (`CreatedByUserId`).
- **PollOption** — one answer choice (fixed Yes/No pair, 1-5 stars, or creator-supplied
  labels for `SingleChoice`). Not used for `OpenText`.
- **Vote** — one respondent's answer. Anonymous: identified by a random voter token, not a
  user account. A unique index on `(PollId, VoterToken)` enforces one vote per browser per
  poll at the database level.

### Anonymous voter identity

The voter token used to be a server-set `HttpOnly` cookie. Once the frontend (Vercel) and
the API (Render) moved onto different domains, that stopped working reliably — Safari/iOS
Intelligent Tracking Prevention blocks third-party cookies between different sites by
design, no server-side `SameSite`/`Secure` tuning gets around it. The token is now generated
client-side instead: `frontend/src/api/voterToken.js` creates a `crypto.randomUUID()` the
first time a browser visits, stores it in `localStorage`, and `client.js`'s axios
interceptor attaches it to every request as an `X-Voter-Token` header. `VotesController`
and `PollsController` read it from `Request.Headers["X-Voter-Token"]`. The database-level
one-vote-per-browser guarantee — the unique `(PollId, VoterToken)` index — is unchanged;
only where the token itself comes from changed.

### Real-time flow

1. Browser opens the results page → connects to `/hubs/polls` over SignalR → calls
   `JoinPoll(code)` to join that poll's group.
2. Another browser casts a vote → `VotesController` saves it, recomputes the tally,
   refreshes the Redis cache, then calls
   `_hub.Clients.Group(...).SendAsync("resultsUpdated", results)`.
3. Every browser in that group receives the update and re-renders the Recharts bar chart —
   no polling.

### Caching

`GET /api/polls/{code}/results` reads from Redis first (`IDistributedCache`, 10-minute TTL).
`VoteService` bypasses the cache to compute the fresh count after a write, then repopulates
it — so the cache never serves a stale count to the next viewer, and the SignalR broadcast
and the cached HTTP response always agree.

Every Redis call in `PollService` (read, write, invalidate) is wrapped in a `try/catch` that
logs a warning and falls straight through to the database — the cache is a speed
optimisation, never a hard dependency. `InfrastructureDependencyInjection` also sets
`AbortOnConnectFail = false` plus explicit 2-second connect/sync timeouts, so a brief network
blip against the managed cloud Redis instance doesn't take the API down with it.

### Security / rate limiting

- **Identity + JWT**: creating a poll and viewing the creator dashboard are `[Authorize]`-protected
  (this is the "protect the admin/create page" requirement). Voting itself needs no account.
- **Rate limiting** (`Microsoft.AspNetCore.RateLimiting`): a global 100 req/min-per-IP limiter,
  plus tighter dedicated limiters — 10 req/10s on `POST /vote`, 10 req/10s on `POST /auth/*`,
  and 5 req/min on `POST /polls` — to blunt scripted vote-stuffing, credential stuffing, and
  poll-spamming.
- **FluentValidation** (`CreatePollValidator`, `CastVoteValidator`) + EF Core Fluent API
  constraints (`Configurations/*.cs`) validate input at both the DTO and schema level.
- Poll codes are generated with a CSPRNG and exclude visually-ambiguous characters
  (`0/O`, `1/l/I`) so QR-scanned or screen-shared codes are unambiguous and unguessable.
- Poll share links and QR codes point at `Frontend:BaseUrl` (falls back to the API's own
  host only in local dev) so `/poll/{code}` always resolves against the deployed SPA on
  Vercel, never against the API's own domain.

---

## 3. Project layout

```
PollSurveyBuilder.sln
src/
  PollSurveyBuilder.Domain/          entities, enums — no external dependencies
  PollSurveyBuilder.Application/     DTOs, service interfaces, FluentValidation validators
  PollSurveyBuilder.Infrastructure/  EF Core, Identity, Redis, QR codes, background job
  PollSurveyBuilder.API/             controllers, SignalR hub, Program.cs, Dockerfile
  PollSurveyBuilder.Tests/           xUnit unit + integration tests (EF Core InMemory)
frontend/                            React + Vite SPA
                                      - Dockerfile + nginx.conf: container-based hosting
                                      - vercel.json: SPA rewrite rule for Vercel's native hosting (live)
                                      - src/components/Layout.jsx, Footer.jsx: shared page chrome
                                      - src/pages/Terms.jsx, Privacy.jsx: static legal pages
.github/workflows/ci-cd.yml          build, test, lint, docker build/push, deploy
docker-compose.yml                   API + frontend + SQL Server + Redis, for local/demo use
```

---

## 4. Running locally

### Option A — Docker Compose (closest to production)

```bash
docker compose up --build
```

- Frontend: http://localhost:5173
- API: http://localhost:5080 (Scalar docs at `/scalar/v1` in Development)
- SQL Server: localhost:2433 (sa / see `MSSQL_SA_PASSWORD` in `docker-compose.yml` —
  change this to a real secret before you ever deploy anywhere public)
- Redis: localhost:6379

The API applies EF Core migrations automatically on startup in this template's intended
setup — if you haven't created a migration yet, run the commands in Option B once against
the compose database, then `docker compose up --build` again.

### Option B — Run backend and frontend separately (faster inner dev loop)

```bash
# 1. Start just the databases
docker compose up sqlserver redis

# 2. Create the initial migration (first time only) and update the database
cd src/PollSurveyBuilder.API
dotnet ef migrations add InitialCreate --project ../PollSurveyBuilder.Infrastructure --startup-project .
dotnet ef database update --project ../PollSurveyBuilder.Infrastructure --startup-project .

# 3. Run the API
dotnet run

# 4. In a second terminal, run the frontend
cd frontend
cp .env.example .env   # adjust VITE_API_URL if needed
npm install
npm run dev
```

> Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download) and Node.js 20+.
> Install the EF Core CLI once with `dotnet tool install --global dotnet-ef` if you don't have it.

### Running tests

```bash
dotnet test PollSurveyBuilder.sln
```

Covers: short-code generation (uniqueness, excluded characters), poll creation for every
poll type, and the full voting flow (first vote succeeds, duplicate vote from the same
browser is rejected, closed polls reject votes, invalid option IDs are rejected, open-text
answers are stored correctly).

---

## 5. Deploying

This is how the live deployment is actually set up — three separate providers, not one:

1. **Database (Azure)**: a managed Azure SQL Database instance. Put its connection string
   in the API service's `ConnectionStrings__Default` environment variable and leave
   `DatabaseProvider` set to `SqlServer`. (PostgreSQL also works — set `DatabaseProvider`
   to `Postgres` and point `ConnectionStrings__Default` at a Postgres instance instead, if
   you'd rather host the database elsewhere.)
2. **Redis**: provision a managed Redis instance (e.g. on Render or Azure), set
   `ConnectionStrings__Redis`. The API keeps running even if this is briefly unreachable —
   see "Caching" above.
3. **API (Render)**: deploy `src/PollSurveyBuilder.API/Dockerfile` as a Render web service.
   Set `Jwt__Key` (32+ random characters), `Jwt__Issuer`, `Jwt__Audience`, and
   `Frontend__BaseUrl` (your Vercel URL — used to build poll share links and QR codes so
   they point at the SPA, not the API).
4. **CORS**: set `Cors__AllowedOrigins__0` on the API to your Vercel production URL, or the
   browser will refuse to let the SPA call the API at all.
5. **Frontend (Vercel)**: connect the repo's `frontend/` directory to a Vercel project
   (Framework Preset: Vite). Vercel builds and hosts the SPA directly from Git on every
   push — `frontend/vercel.json` supplies the SPA rewrite rule so client-side routes like
   `/poll/7fGh2` resolve correctly. Set the `VITE_API_URL` environment variable in the
   Vercel project settings to your Render API URL. This is independent of the
   `frontend/Dockerfile` image described below, which exists as an alternative
   container-based hosting path (e.g. if you deploy the frontend to Render/Railway
   instead of Vercel) rather than the one currently live.
6. **GitHub Actions secrets** (Repo → Settings → Secrets and variables → Actions):
   - `DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN` — push built images to Docker Hub.
   - `RENDER_DEPLOY_HOOK_API` — Render's deploy webhook URL for the API service.
   - `RENDER_DEPLOY_HOOK_FRONTEND` — only needed if you deploy the frontend's Docker image
     to Render/Railway instead of Vercel; leave the pipeline step as-is (it's harmless if
     unused) or remove it if you're committed to Vercel-only hosting.
   - Repo variable `VITE_API_URL` — baked into the frontend Docker image at build time;
     not used by Vercel's own build, which reads its own project-level environment
     variable of the same name instead.
7. Push to `main` → CI runs tests + lint → builds both multi-stage Docker images → pushes
   to Docker Hub → triggers the Render deploy hook(s). Vercel deploys independently and
   automatically on the same push, via its own Git integration.

---

## 6. Feature checklist against the brief

**Pass**
- [x] Create a poll with up to 6 options, get a short link (`/poll/{code}`)
- [x] Anyone with the link can vote once (enforced via a per-browser token stored in
      `localStorage` and sent as an `X-Voter-Token` header, no login)
- [x] Live results bar chart, updated in real time via SignalR
- [x] Creator can close a poll early
- [x] REST endpoints: `POST /polls`, `GET /polls/{code}`, `POST /polls/{code}/vote`, `GET /polls/{code}/results`
- [x] Relational database (SQL Server or PostgreSQL, switchable via config; Azure SQL Database in production)
- [x] Unit tests for core business logic

**Merit**
- [x] Poll expiry (`ExpiresInMinutes`) — auto-closed by a background hosted service
- [x] Multiple question types: Yes/No, 1-5 star rating, open text
- [x] Linting in CI (`dotnet format --verify-no-changes` for the backend, ESLint for the frontend)
- [x] Multi-stage Docker builds for both API and frontend
- [x] Integration-style tests (EF Core InMemory) covering the full vote-casting flow

**Distinction**
- [x] Redis-backed results cache, invalidated on every vote, with a database fallback if Redis is unreachable
- [x] Rate limiting on the vote, auth, and poll-creation endpoints
- [x] JWT + ASP.NET Identity protecting poll creation / the creator dashboard
- [x] QR code generation for each poll's share link
- [x] Full CI/CD: build → test → lint → Docker build/push → deploy, plus an independent Vercel deployment on every push
- [x] Deployed across three separate managed providers (Render, Vercel, Azure), which is
      why the voter-identity and CORS handling had to be made cross-origin-safe
- [ ] Anonymous Q&A mode, analytics dashboard (over-time chart, peak voting minute) — not
      yet built; see "Ideas for further work" below if your team wants to push further.

---

## 7. Ideas for further work

- Analytics dashboard for creators: votes-over-time line chart, peak voting minute.
- Anonymous Q&A mode alongside voting (submit + upvote questions live).
- Google/Facebook login via `Microsoft.AspNetCore.Authentication.Google` /
  `...Facebook`, added alongside the existing JWT scheme.
- Swap the in-process rate limiter for a distributed one (backed by the same Redis
  instance) if you ever run more than one API replica.
- Frontend test coverage — there currently isn't any, unlike the backend's xUnit suite.

---

## 8. Team

| Name | Student ID | What they built |
|---|---|---|
| Lê Công Tấn Lộc | GCS230109 | Backend: ASP.NET Core API, EF Core, SignalR hub, Identity/JWT, Redis cache, rate limiting, QR codes, background jobs, unit/integration tests, Docker, CI/CD, and the Render/Azure production configuration |
| Trần Đăng Khoa | GCS240229 | Frontend: React/Vite SPA (all pages), SignalR client, Recharts results chart, UI/UX polish, Vercel deployment |

