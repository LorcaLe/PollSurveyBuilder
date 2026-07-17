# Ballot — Poll & Survey Builder

AMD201 group coursework: a real-time poll/survey service built with **ASP.NET Core 10** (Clean Architecture),
**React 18 + Vite** (SPA), **SignalR** (live results), **PostgreSQL/SQL Server**, **Redis** (cached results),
**Docker** (multi-stage builds) and **GitHub Actions** (CI/CD to a container registry + PaaS).

Create a poll, share the link or QR code, and watch the results bar chart update on everyone's
screen the instant a vote is cast — no page refresh, no polling.

---

## 1. Live links

| | |
|---|---|
| Live app | `TODO: paste your deployed frontend URL here` |
| Live API | `TODO: paste your deployed API URL here` |
| API docs (Scalar, dev only) | `<api-url>/scalar/v1` |

> Fill these in before the demo — an undeployed app cannot score above 4 on this coursework.

---

## 2. Architecture

```
                        ┌─────────────────────┐
                        │   React SPA (Vite)  │  <- voters, creators
                        │  nginx (Docker)      │
                        └──────────┬───────────┘
                                   │ HTTPS + WebSocket (SignalR)
                                   ▼
                        ┌─────────────────────┐
                        │  PollSurveyBuilder   │
                        │       .API           │  Controllers, SignalR Hub,
                        │  (ASP.NET Core 10)    │  JWT auth, rate limiting
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
        │  SQL Server /   │                   │      Redis      │
        │   PostgreSQL    │                   │ (results cache) │
        └────────────────┘                   └────────────────┘

        .Domain  <-- entities/enums, no dependencies on anything else
        .Application <-- DTOs, service interfaces, FluentValidation validators
```

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
- **Vote** — one respondent's answer. Anonymous: identified by a random `voter_token`
  cookie, not a user account. A unique index on `(PollId, VoterToken)` enforces one vote
  per browser per poll at the database level.

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

### Security / rate limiting

- **Identity + JWT**: creating a poll and viewing the creator dashboard are `[Authorize]`-protected
  (this is the "protect the admin/create page" requirement). Voting itself needs no account.
- **Rate limiting** (`Microsoft.AspNetCore.RateLimiting`): a global 100 req/min-per-IP limiter,
  plus a tighter 10 req/10s limiter on `POST /vote` and `POST /auth/*` to blunt scripted
  vote-stuffing and credential stuffing.
- **FluentValidation** (`CreatePollValidator`, `CastVoteValidator`) + EF Core Fluent API
  constraints (`Configurations/*.cs`) validate input at both the DTO and schema level.
- Poll codes are generated with a CSPRNG and exclude visually-ambiguous characters
  (`0/O`, `1/l/I`) so QR-scanned or screen-shared codes are unambiguous and unguessable.

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
frontend/                            React + Vite SPA, Dockerfile (nginx)
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
- SQL Server: localhost:1433 (sa / `Your_password123` — change this before you ever deploy)
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

1. **Database**: provision a managed Postgres or SQL Server instance (Render/Railway/Azure
   all offer one) and put its connection string in the API service's `ConnectionStrings__Default`
   environment variable. Set `DatabaseProvider` to `Postgres` or `SqlServer` to match.
2. **Redis**: provision a managed Redis instance, set `ConnectionStrings__Redis`.
3. **Secrets**: set `Jwt__Key` (32+ random characters), `Jwt__Issuer`, `Jwt__Audience`.
4. **CORS**: set `Cors__AllowedOrigins__0` to your deployed frontend's URL.
5. **GitHub Actions secrets** (Repo → Settings → Secrets and variables → Actions):
   - `DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN` — push built images to Docker Hub.
   - `RENDER_DEPLOY_HOOK_API`, `RENDER_DEPLOY_HOOK_FRONTEND` — PaaS deploy webhook URLs
     (Render/Railway both expose one per service).
   - Repo variable `VITE_API_URL` — your deployed API's public URL, baked into the frontend
     build.
6. Push to `main` → CI runs tests + lint → builds both multi-stage Docker images → pushes
   to Docker Hub → triggers the PaaS deploy hooks.

---

## 6. Feature checklist against the brief

**Pass**
- [x] Create a poll with up to 6 options, get a short link (`/poll/{code}`)
- [x] Anyone with the link can vote once (enforced via a per-browser cookie token, no login)
- [x] Live results bar chart, updated in real time via SignalR
- [x] Creator can close a poll early
- [x] REST endpoints: `POST /polls`, `GET /polls/{code}`, `POST /polls/{code}/vote`, `GET /polls/{code}/results`
- [x] Relational database (SQL Server or PostgreSQL, switchable via config)
- [x] Unit tests for core business logic

**Merit**
- [x] Poll expiry (`ExpiresInMinutes`) — auto-closed by a background hosted service
- [x] Multiple question types: Yes/No, 1-5 star rating, open text
- [x] Linting in CI (`dotnet build -warnaserror`, ESLint for the frontend)
- [x] Multi-stage Docker builds for both API and frontend
- [x] Integration-style tests (EF Core InMemory) covering the full vote-casting flow

**Distinction**
- [x] Redis-backed results cache, invalidated on every vote
- [x] Rate limiting on the vote and auth endpoints
- [x] JWT + ASP.NET Identity protecting poll creation / the creator dashboard
- [x] QR code generation for each poll's share link
- [x] Full CI/CD: build → test → lint → Docker build/push → deploy
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

---

## 8. Team

| Name | Student ID | What they built |
|---|---|---|
| TODO | TODO | TODO |
| TODO | TODO | TODO |
| TODO | TODO | TODO |

(Each member's individual report is submitted separately per the assignment brief.)
