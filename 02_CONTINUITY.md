# 02 — CONTINUITY: progress & project state

> Claude reads this, and the synced GitHub repository, at the start of every chat. At the end of every lesson Claude outputs this **whole file**, updated — replace it in Project knowledge after filling in the `__` fields.

## CURRENT STATE

**Last completed lesson:** L02 — ASP.NET Core fundamentals + REST API design done right
**Next lesson:** L03 — Git & DevOps foundations: branches, pull requests, CI & supply-chain hygiene
**Pace:** 4 lessons a day (~2 hours each), 20 lessons over 5 days
**Today:** Day 1 — L01 ✅, L02 ✅, L03, L04
**Behind schedule?** no
**Interview:** right after Day 5 — _fill in the date_
**Target role:** Senior .NET Developer, authentication middleware team, European energy group (OAuth2/OIDC/SAML2, Keycloak incl. plugins, IdentityServer/Ping, web security, REST, Git & DevOps)

### Project state (CampusGate — university identity & access platform)
- **Last project step completed:** L02 — ✅ code provided — tick when it runs on your machine: [ ]
- **Physical location:** `D:\CampusGate\` (solution file `D:\CampusGate\CampusGate.slnx`)
- **Solution tree:**
    CampusGate (D:\CampusGate)
    ├─ CampusGate.slnx
    ├─ .gitignore · .gitattributes
    ├─ docs
    │  ├─ api-guidelines.md
    │  └─ api-checklist.md
    └─ src
       ├─ CampusGate.Contracts
       │  ├─ Common/PageRequest.cs · PagedResponse.cs
       │  ├─ Courses/CourseDto.cs · CourseCodeFormat.cs · UpsertCourseRequest.cs · CreateEnrolmentRequest.cs · EnrolmentDto.cs
       │  └─ Grading/GradingPolicyDto.cs
       └─ CampusGate.Api
          ├─ Configuration/GradingOptions.cs
          ├─ Controllers/CoursesController.cs · EnrolmentsController.cs · GradingPolicyController.cs · DiagnosticsController.cs (DEV ONLY)
          ├─ ErrorHandling/GlobalExceptionHandler.cs
          ├─ Middleware/CorrelationIdMiddleware.cs
          ├─ Stores/ICourseStore.cs · InMemoryCourseStore.cs · StoreModels.cs
          ├─ Properties/launchSettings.json
          ├─ CampusGate.Api.http (22 requests = manual test plan)
          ├─ Program.cs
          └─ appsettings.json (Grading section) · appsettings.Development.json
- **What works:** `api/v1/courses` GET (paged, default 20, capped at 100) / GET `{code}` / PUT (201 create, 200 update) / DELETE (204, then 404); `api/v1/courses/{code}/enrolments` POST (201 + Location; 409 `already_enrolled` / `course_full`) and GET `{id:guid}`; `api/v1/grading-policy`; malformed course code → 400; non-JSON body → 415; every error is ProblemDetails with `traceId`, `correlationId`, `errorCode` where relevant; 500 without internals; `X-Correlation-Id` on every response (validated, else generated); `/health`; `ValidateOnStart` on `GradingOptions`; OpenAPI in Development only.
- **Differences from the lessons / known issues:** default branch is `master` (not `main`) — decide in L03 (rename or protect `master`). `Microsoft.OpenApi` package removed and `.http` switched to HTTPS in L02 Step 1/9 (they were still present in the synced repo at L02 start). Student number still comes from the request body (no auth yet — fixed in L08). Reducing a course's capacity below its current enrolments is not prevented yet (domain rule in L04). Old `api/courses` routes now 404 (moved to `v1`).
- **Ports:** API HTTPS `7072` · API HTTP `5287` · Portal _fill in (L13)_ · Tuition _fill in (L16)_ · Keycloak _fill in (L09/L11)_ · Seq _fill in (L11)_ · Meridian IdentityServer _fill in (L19)_
- **GitHub repository URL:** https://github.com/rabeefarhat/CampusGate (public)
- **Repository in Project knowledge:** connected (`src/`, `.gitignore`, `CampusGate.slnx`; add `docs/` from L02) — sync after every merged PR
- **Last PR merged and synced:** none — L02 committed directly to `master`: `feat: pipeline, DI, validation, ProblemDetails, REST v1 conventions` (PRs start in L03)

### Code facts (only what the repo can't show)
- `InMemoryCourseStore` is a singleton guarded by `System.Threading.Lock`; it is temporary — the interface becomes an Application port and an EF Core repository replaces it in L04.
- `GradingOptions` lives in the Api temporarily; moves to Application in L04.
- `DiagnosticsController` (`GET api/v1/diagnostics/boom`) throws on purpose in Development only (404 elsewhere, hidden from OpenAPI).
- Break-it verified: removing the lock (+ a 50 ms sleep) lets ~10 students into a capacity-2 course; parallel PowerShell script in the L02 lesson / `docs/api-checklist.md` row 24.

### Identity facts (never real secrets)
| Item | Value |
|---|---|
| Keycloak version (pinned) | _(L09/L11)_ |
| Realm / issuer URL | _(L11)_ |
| Clients (ID · type · flows · redirect URIs) | _(L11 →)_ |
| Client scopes · audiences | _(L11 →)_ |
| Roles · groups · test users (fictional) | _(L11)_ |
| Mappers / SPIs installed | _(L11, L18)_ |
| Identity providers (brokered) | _(L19)_ |
| Other trusted issuers / schemes in the API | _(L15, L19)_ |
| User Secrets keys in use (names only) | _(L04 →)_ |

### Working agreements
1. One chat = one lesson: `START Lxx` or `START Lxx PARTS`; five parts (Understand · Build · Test · Practice · Review).
2. **No TDD.** Build and run first; all tests in Part 3; one test per business and security rule; one 💥 break-it per lesson.
3. **Zero prior knowledge of security, identity and DevOps** — every term defined, every protocol traced in raw HTTP, exact error messages shown.
4. Every 🟢 concept ends with ⬛ production best practices and an 🎤 interview angle.
5. Standards before products; current best practice only (code + PKCE, no implicit/password grants, BFF, RFC 9700); dev shortcuts labelled ⚠️ DEV ONLY.
6. Clean Architecture (Domain ← Application ← Infrastructure ← hosts); `CampusGate.Auth` is the shared auth library.
7. Controllers only; REST conventions from L02 (`docs/api-guidelines.md`); DTOs only.
8. From L03: feature branch → small commits → PR → green CI → squash-merge → Sync.
9. Testing toolkit: xUnit · FluentAssertions **7.x** · NSubstitute (ports only) · Bogus (fixed seed) · Testcontainers (Keycloak) · JUnit for SPIs.
10. Never log or share tokens, cookies, secrets or personal data; decode tokens only with TokenInspector.

### Project steps
- Day 1: [x] L01 First API · [x] L02 Fundamentals + REST · [ ] L03 Git & DevOps · [ ] L04 Clean Architecture + MediatR
- Day 2: [ ] L05 Web security I · [ ] L06 Web security II · [ ] L07 Cryptography · [ ] L08 ASP.NET Core auth
- Day 3: [ ] L09 OAuth 2.0 · [ ] L10 OIDC · [ ] L11 Keycloak · [ ] L12 Securing the API
- Day 4: [ ] L13 Portal BFF · [ ] L14 Service-to-service · [ ] L15 Auth middleware · [ ] L16 SAML 2.0
- Day 5: [ ] L17 Keycloak customisation · [ ] L18 Keycloak SPIs · [ ] L19 IdentityServer & platforms · [ ] L20 Troubleshooting + mock interview

### Ubiquitous language (source of truth: docs/ubiquitous-language.md)
- _(starts in L04: student number, course, course code, enrolment, capacity, grade, published grade, correction, faculty…)_
- Pre-terms used in L01–L02: course, course code (2–4 letters + 3 digits), faculty (`science`, `arts`), capacity, enrolment, student number (8 digits starting with 2; Luhn from L04), grading policy (max grade 20, max 30 days in the past).

### Environment
- Visual Studio 2026 version: 10.0.302 · workload: ASP.NET and web development ✓
- .NET SDK: _fill in (10.0.x)_
- GitHub account: ✓
- Docker Desktop: ✓ (Engine running, ≥ 4 GB RAM — confirm before L04, SQL Server container)
- HTTPS dev certificate trusted: _yes/no_
- Second browser profile: _yes/no_ (L13) · SAML-tracer extension: _yes/no_ (L16)
- JDK 21: _not yet — install before Day 5_ · IntelliJ IDEA: _not yet — install before Day 5_

### Confidence & mock scores
- Day 2 end self-rating (security basics /5): _?_ · Day 4 end (protocols /5): _?_ · L20 mock interview: _?_

### Weak spots (max 15)
- (candidate) Explaining the full request path: Kestrel → middleware → routing → new controller instance → model binding → result.
- (candidate) SDK vs runtime vs target framework (`net10.0`) and why `global.json` matters.
- (candidate) Why a public repo makes any committed secret permanent (history) → rotate, not just delete.
- (candidate) Idempotency is about server state, not the status code (DELETE → 204 then 404); POST needs an idempotency key to be retry-safe.
- (candidate) Check-then-act races in singletons; why a lock (in-process) or the database (across servers) must make it atomic.
- (candidate) When 404 instead of 403 (enumeration / BOLA) and how traceId keeps it debuggable.

### Things that confused me
- _(your own words)_

### Skipped / deferred items
- Malformed course code → 400: ✅ done in L02.
- `async`/`CancellationToken` in actions: deferred to L04 (in-memory store has no I/O).
- Idempotency keys for POST, ETags/`If-Match`: 🟣 optional (`DEEPER`).

### Decisions (ADRs, one line each)
- _(none yet — ADR-001 Clean Architecture comes in L04)_
- Informal: controllers over minimal APIs (structure, filters, enterprise conventions); .NET 10 LTS.
- Informal (L02): URL versioning `api/v1`; offset paging, default 20, server max 100 (capped, not rejected); RFC 9457 ProblemDetails with `traceId`, `correlationId`, `errorCode`; 404 for objects a caller may not see; DTOs in a separate `CampusGate.Contracts` library.

### "Explain CampusGate in 2 minutes" versions
- _(starts after L08; identity version after L13; final version in L20)_

## LESSON LOG
| Lesson | Date | Time spent | Key takeaways (short) | Self-score (avg /5) | Open questions |
|---|---|---|---|---|---|
| L01 | __ | __ | C# → IL → JIT in Kestrel; config layers (last wins); raw HTTP anatomy; one controller instance per request; `MapControllers` missing → empty 404 | __ | __ |
| L02 | __ | __ | Pipeline order is behaviour; lifetimes flow downward; singletons need atomic check-then-act; REST safe/idempotent; ProblemDetails without internals; 404 vs 403; correlation IDs validated; `ValidateOnStart` | __ | __ |

---

## INTERVIEW STORY BANK
_(1–2 STAR-style talking points per lesson: Situation · Task · Action · Result — honest, lab-scale, no inflated claims.)_

- **L01 — Starting a secure-by-default API.** S: new lab project for a fictional university identity platform. T: a first REST API on .NET 10 LTS, versioned in a public GitHub repo. A: created a controller-based ASP.NET Core 10 API, kept HTTPS on, cleaned up a redundant package pin, kept secrets out of config. R: a running API with a repeatable `.http` test plan; I can explain the request path from Kestrel to the controller.
- **L01 — Diagnosing a "silent" 404.** S: removed `MapControllers()` on purpose. T: explain why every route returned an empty 404. A: compared the Endpoints Explorer, recognised no endpoints were mapped. R: I know a body-less 404 usually means "no endpoint matched", not "resource not found".
- **L02 — Consistent, leak-free error handling.** S: lab API returned mixed error shapes, and an unhandled exception could expose internals. T: one error contract for clients and support. A: added an `IExceptionHandler` writing RFC 9457 ProblemDetails with `traceId`, a correlation-ID middleware that validates client-supplied IDs (log-injection guard), and verified a simulated failure never leaked server names. R: every 4xx/5xx has the same shape and can be matched to a server log line; documented in `docs/api-guidelines.md`.
- **L02 — Reproducing an overbooking race.** S: an in-memory singleton store handled enrolments with a capacity rule. T: prove why the lock mattered. A: removed it, added artificial latency, fired 10 parallel requests at a capacity-2 course; ~10 succeeded; restored the lock → exactly 2 × 201, 8 × 409. R: I can explain check-then-act races and how the same problem is solved across servers with a database concurrency token (planned in L04).

---

## KEY IDEAS CHEAT SHEET
_(Grows by 3–6 lines per lesson, incl. at least one trade-off, one failure mode and one production rule. L20 condenses it to one printable page.)_

- .NET 10 is the current LTS (Nov 2025 → Nov 2028); C# 14 and ASP.NET Core 10 ship with it.
- Standards before products: if you understand OAuth 2.0, OIDC and SAML 2.0, then Keycloak, IdentityServer, Ping and Entra are configuration differences.
- Authentication proves who you are; authorisation decides what you may do — and every object access must be authorised (BOLA).
- Licences shape choices: MediatR 12.5.0 and FluentAssertions 7.x pinned; Duende IdentityServer free for dev/test, licensed in production; Keycloak is Apache 2.0 but only the latest release gets security fixes.
- For every technology, be ready to say what it costs, how it fails, and how you'd run it in production.
- SDK builds, runtime runs: C# → IL (DLL) → JIT → machine code, hosted in Kestrel; production images carry only the runtime.
- Configuration is layered (appsettings → appsettings.{Env} → User Secrets → env vars → command line); the last source wins.
- Trade-off: controllers (structure, filters, conventions) vs minimal APIs (less ceremony) — same routing and auth underneath.
- Failure mode: 404 with an empty body on every route → no endpoint matched (`MapControllers()` missing, non-public controller, wrong route).
- Production rule: LTS only, SDK pinned, HTTPS always, nothing sensitive in URLs or in a public repo — ever.
- Pipeline order is behaviour: correlation → exception handler → HTTPS → routing → CORS → authentication → authorisation → endpoints.
- Lifetimes flow downward (singleton → scoped → transient); captive dependency = singleton holding a scoped object; singletons must be thread-safe and check-then-act must be atomic.
- REST: GET safe; PUT/DELETE idempotent (same state, status may differ); POST neither — retry-safe only with an idempotency key.
- Trade-off: offset paging (simple, jump to page N) vs keyset (stable and fast on large tables); URL versioning (visible) vs header versioning (clean URLs).
- Failure mode: stack trace or server names in a 500 → information disclosure; fix with `IExceptionHandler` + generic ProblemDetails + `traceId` to find the real error in logs.
- Production rule: one RFC 9457 error shape everywhere; 404 (not 403) for other people's objects; server-side max page size; `ValidateOnStart`; ids in logs, never personal data.
