# CURRICULUM — CampusGate: secure .NET 10, authentication middleware, OAuth 2.0 / OIDC / SAML 2.0, Keycloak & DevOps — from zero to interview-ready in 5 days

**20 lessons · ~2 hours each · 4 lessons a day · 5 days · 1 project that grows every lesson**

Core: .NET 10 (LTS) · C# 14 · ASP.NET Core 10 (controllers, REST) · Clean Architecture · MediatR & CQRS · FluentValidation · Razor Pages · YARP · Serilog + Seq
**Security & identity:** web security · cryptography for identity · ASP.NET Core authentication & authorisation middleware · **OAuth 2.0** · **OpenID Connect** · **SAML 2.0** · JWT · **Keycloak** (admin, Admin REST API, themes, **Java SPI plugins**) · **Duende IdentityServer** · **Ping Identity** · Microsoft Entra ID · OWASP
**DevOps:** Git · pull requests · branch protection · GitHub Actions CI · Dependabot · secret scanning · CodeQL · Docker
Testing: xUnit · FluentAssertions 7.x · NSubstitute · Bogus · `WebApplicationFactory` · Testcontainers (Keycloak) · JUnit 5 (SPIs)

---

## 0. How this course works

### Who it's for
You know some programming but **nothing about security, identity or DevOps**. In 5 days you build, troubleshoot and **explain in an interview** a .NET platform secured by Keycloak, with your own authentication middleware library, partner integrations over OAuth 2.0, a legacy app on SAML 2.0, a brokered IdentityServer, Keycloak plugins you wrote, and a production-grade Git workflow.

### The job this course targets
A senior .NET developer in a specialist team that owns **authentication middleware** for a large European energy group. Every lesson maps to a line of the job description (§4).

### The depth stack (every 🟢 concept)

| Layer | What it answers | Marker |
|---|---|---|
| 1. Orientation | What is it? Where does it fit? | 🧭 |
| 2. Why | Which problem does it solve? (analogy) | 💡 |
| 3. Definition | Plain words, then technical (+ RFC/spec) | 🟦 |
| 4. The basics | How do I use it? | 📘 |
| 5. Under the hood | What happens at runtime — for protocols, a 📜 raw HTTP trace | ⚙️ |
| 6. Trade-offs | Gain, cost, when not, alternative | ⚖️ |
| 7. Failure modes | Symptom (exact error) → cause → notice → fix | 💣 |
| 8. Production practice | What a senior engineer insists on | ⬛ |

Then 🟩 example · 🟥 mistake · 🟪 say it out loud · 🎤 interview angle · 🟨 in short.

**Priority levels:** 🟢 Core (full stack, max 3–4 per lesson) · 🔵 Important (compact) · 🟣 Advanced (pointers → `DEEPER`).

### Five parts per lesson (~2 hours)

| Part | Time | What you do |
|---|---|---|
| 1. Understand | 35 min | Concepts, 📜 traces, 🎬 animations. Explain each ⚙️ out loud. |
| 2. Build | 45 min | Add the feature (Visual Studio, Keycloak console, IntelliJ, GitHub). |
| 3. Test | 20 min | Write and run the lesson's tests; one 💥 break-it. |
| 4. Practice | 12 min | Quiz, explain-it, **interview drill** — out loud. |
| 5. Review | 8 min | Summary, glossary, merge the PR, replace `02_CONTINUITY.md` with the full updated file. |

**Between lessons:** 10 minutes away from the screen. Lunch after the second lesson. **Evenings:** 20 minutes max — handout, weak spots, or `MOCK INTERVIEW <today's topic>`.

### Git workflow from L03
Every lesson after L03: **feature branch → small commits → PR → CI green → squash-merge → Sync** the repo in Project knowledge. By the interview you'll have ~18 merged PRs — real practice, not theory.

### Tests at the end of every lesson (no TDD)
Part 2 builds and runs the feature. **Part 3** writes every test on the lesson's 🧪 list. Security tests are first-class: every protected endpoint is tested for 401/403/404 and wrong issuer, audience and lifetime.

### A note on expectations
Four lessons a day is intense and Days 3–4 are the hardest. If a day overruns, send `BEHIND` or move the last lesson to the next morning. The interview depends most on **L08–L15 and L18–L20**: protect that time.

---

## 1. The project: CampusGate

### 1.1 The story (fictional)
**Northwind University** is a fictional European university with several faculties. **CampusGate** is its identity and access platform for students, teachers, staff and partner institutions:
- **Students** log in to the **student portal** to see their **courses** and **enrolments**, enrol in courses and view their **grades** (each student has an 8-digit **student number**).
- **Teachers** record and correct grades for the courses they teach in their **faculty**.
- **Registrar staff** look up any student record (every lookup is **audited**).
- **Partner universities** (exchange programme) fetch transcripts of exchange students in their faculty **machine-to-machine** with OAuth 2.0 client credentials.
- The **legacy tuition & fees application** signs staff in with **SAML 2.0**.
- **Meridian Medical School**, an affiliated school, has its own identity provider (**Duende IdentityServer**); its teachers log in through **Keycloak brokering**.
- The specialist team (you) maintains **`CampusGate.Auth`**, the shared authentication middleware library every CampusGate service uses.

**All data, institutions, students and teachers are fictional.**

### 1.2 Architecture

```
 Browser ─cookie─► CampusGate.Portal (Razor Pages BFF: OIDC code+PKCE, tokens server-side, YARP)
                        │ Bearer (user token)                     │ OIDC
                        ▼                                         ▼
                  CampusGate.Api (Clean Architecture) ◄── trusts ── Keycloak (realm "campusgate", PostgreSQL)
                        ▲   uses CampusGate.Auth                     ├─ brokers ► Meridian IdentityServer (OIDC)
 Partner univ ─client credentials─┘                                 ├─ SAML IdP ► CampusGate.Tuition (SAML SP)
 Legacy attendance gateway ─X-Api-Key─► (custom handler, L15)       └─ SPIs: campusgate-claims mapper, account-status authenticator
```

### 1.3 Clean Architecture

```
 Api / Portal / Tuition hosts   ← composition roots, controllers, Razor Pages
 CampusGate.Auth                 ← shared auth middleware library (schemes, handlers, policies, claims transformation)
   Infrastructure               ← repositories (in-memory adapters), Keycloak Admin client, token clients
     Application                ← use cases (MediatR), validators, behaviors, ports: ICurrentUser, IAuditLog, IIdentityProvisioning
       Domain                   ← Student, Course, Enrolment, Grade, StudentNumber, CourseCode, access rules
```
Dependencies point inward. Swaps proven in the course: dev token issuer → Keycloak (L12) · single issuer → multiple issuers via policy scheme (L15, L19).

🏛️ **Persistence note:** this course is about identity, so repositories are **thread-safe in-memory adapters** behind ports in Infrastructure. Say this honestly in interviews: "Persistence is an adapter; in production it would be EF Core on SQL Server or PostgreSQL." (Never the EF Core InMemory provider.)

### 1.4 The identity model (built from L11)

| Item | Value |
|---|---|
| Realm | `campusgate` |
| Realm roles | `student`, `teacher`, `registrar`, `admin`, `auditor` |
| Groups | `/students`, `/teachers/science`, `/teachers/arts`, `/registrar` |
| User attributes → claims | `student_id`, `faculty` (built-in mappers in L11, custom SPI mapper in L18) |
| Clients | `campusgate-portal` (confidential, code + PKCE, BFF) · `campusgate-api` (audience only) · `partner-aurora` (client credentials) · `campusgate-tuition` (SAML SP) · `campusgate-provisioner` (service account, Admin API) |
| Client scopes | `courses:read`, `enrolments:write`, `grades:read`, `grades:write`, `partner.transcripts:read` |
| Identity providers | `meridian-medical` (OIDC → Duende IdentityServer, L19) |
| Customisations | login theme (L17) · `campusgate-claims` protocol mapper + `account-status-check` authenticator (L18) |

### 1.5 Business & access rules (fictional)
- **Student number:** 8 digits, starts with `2`, last digit is a **Luhn (mod-10) check digit**. **Course code:** 2–4 capital letters + 3 digits (e.g., `CS101`).
- **Enrolments:** a student can't enrol twice in the same course; a course can't exceed its **capacity** (→ 409); enrolment closes on the course's enrolment deadline.
- **Grades:** value 0–20; only for enrolled students; once **published** a grade changes only through a **correction** by a teacher with a reason ≥ 10 chars; exam date not in the future and not more than 30 days in the past.
- **Access:** a student sees only their own enrolments and grades · a teacher grades only courses in their `faculty` that they teach · registrar may read any student, **every read audited** · a partner university may read transcripts only for its own faculty's exchange students and only with scope `partner.transcripts:read` · auditors read the audit log only · Keycloak admins are not CampusGate users.

### 1.6 Final solution structure

```
CampusGate (.slnx)
├─ src
│  ├─ CampusGate.Domain · CampusGate.Application · CampusGate.Infrastructure · CampusGate.Api
│  ├─ CampusGate.Contracts            # request/response DTOs
│  ├─ CampusGate.Auth                 # shared auth middleware library (L08 → L15), packable as NuGet
│  ├─ CampusGate.ServiceDefaults      # Serilog, correlation IDs, health checks, security headers
│  ├─ CampusGate.Portal               # Razor Pages BFF (L13)
│  └─ CampusGate.Tuition             # legacy-style Razor Pages app, SAML 2.0 SP (L16)
├─ identity
│  ├─ docker-compose.yml             # Keycloak + PostgreSQL + Seq (L11)
│  ├─ keycloak/realm-campusgate.json  # realm as code
│  ├─ keycloak/themes/campusgate      # login theme (L17)
│  ├─ keycloak/spi/campusgate-keycloak-extensions   # Java/Maven SPIs (L18)
│  ├─ keycloak/Dockerfile            # custom image with theme + SPIs (L18)
│  └─ Meridian.IdentityServer        # Duende IdentityServer, affiliated school IdP (L19)
├─ tests
│  ├─ CampusGate.Testing              # Bogus fakers, TestTokens, KeycloakFixture
│  ├─ CampusGate.Domain.Tests · .Application.Tests · .Api.IntegrationTests · .Auth.Tests
│  ├─ CampusGate.Security.Tests       # web security & OWASP suite (L05 →)
│  └─ CampusGate.ArchitectureTests
├─ tools
│  ├─ CampusGate.TokenInspector       # local JWT/SAML decoder + PKCE helper (L07)
│  ├─ CampusGate.PartnerUniversity.Client  # partner console app (L14)
│  └─ Playground.VulnerableApp       # localhost-only attack lab (L06)
├─ .github                           # ci.yml, security.yml, PR template, dependabot.yml
└─ docs                              # api-guidelines, ADRs, oauth/oidc/saml notes, runbooks, threat notes
```

### 1.7 How the project grows

| Lesson | What we add |
|---|---|
| **Day 1 — .NET foundations** | |
| L01 | Solution + first API (`CoursesController`), pushed to GitHub |
| L02 | Pipeline, DI, config, validation, ProblemDetails, logging, health + REST API design |
| L03 | Git & DevOps: branches, PRs, branch protection, GitHub Actions CI, Dependabot, secret scanning |
| L04 | Clean Architecture, domain (`StudentNumber`, `Course`, `Grade`), MediatR, FluentValidation, behaviors, CQRS, tests |
| **Day 2 — Security foundations** | |
| L05 | Web security I: HTTP, TLS, certificates, origins, cookies, CORS, HSTS |
| L06 | Web security II: OWASP Top 10, XSS, CSRF, open redirect, SSRF, security headers |
| L07 | Cryptography for identity: hash, HMAC, signatures, JWT, JWK/JWKS, key rotation, TokenInspector |
| L08 | ASP.NET Core authN/authZ middleware: schemes, JWT validation, policies, resource checks, BOLA — `CampusGate.Auth` born |
| **Day 3 — Protocols & Keycloak** | |
| L09 | OAuth 2.0 by hand: auth code + PKCE, client credentials, refresh, revocation, introspection |
| L10 | OpenID Connect: ID token, discovery, nonce, the .NET OIDC handler, logout |
| L11 | Keycloak fundamentals: Docker + PostgreSQL, realm `campusgate`, clients, scopes, mappers, policies, realm as code |
| L12 | Securing the API with Keycloak: issuer, audience, JWKS, claims transformation, scopes + roles |
| **Day 4 — Integrations & middleware** | |
| L13 | Student portal as BFF: OIDC login, server-side tokens, refresh, back-channel logout, antiforgery |
| L14 | Service-to-service & partner integrations: client credentials, token caching, token exchange, `private_key_jwt`, onboarding |
| L15 | Building & maintaining auth middleware: custom handler, policy scheme, dynamic policies, consistent errors, versioned library |
| L16 | SAML 2.0: assertions, bindings, metadata, .NET SP, Keycloak as SAML IdP |
| **Day 5 — Customisation, platforms, interview** | |
| L17 | Keycloak customisation I: Java/Maven for C# devs, themes, Admin REST API, config as code |
| L18 | Keycloak SPI plugins: protocol mapper + authenticator, JUnit, custom image built in CI |
| L19 | Duende IdentityServer, federation & brokering; Ping Identity & Entra ID compared; migrating IdPs |
| L20 | Troubleshooting auth flows + production identity checklist + DevSecOps recap + **full mock interview** |

### 1.8 Schedule

| Day | 09:00–11:00 | 11:15–13:15 | 14:15–16:15 | 16:30–18:30 |
|---|---|---|---|---|
| **1** | L01 First API | L02 Fundamentals + REST | L03 Git & DevOps | L04 Clean Architecture + MediatR |
| **2** | L05 Web security I | L06 Web security II | L07 Cryptography | L08 ASP.NET Core auth |
| **3** | L09 OAuth 2.0 | L10 OIDC | L11 Keycloak | L12 Securing the API |
| **4** | L13 Portal BFF | L14 Service-to-service | L15 Auth middleware | L16 SAML 2.0 |
| **5** | L17 Keycloak customisation | L18 Keycloak SPIs | L19 IdentityServer & platforms | L20 Troubleshooting + mock interview |

### Why this order
- **Git and CI on day 1 (L03)** so every later change is a reviewed PR.
- **Web security → crypto → ASP.NET Core auth → OAuth → OIDC → Keycloak:** each step only uses words defined in the step before. You first issue your own JWT (L08) so you understand what a token server does — and why production systems don't do that.
- **Standards before products:** OAuth/OIDC/SAML first, then Keycloak, then IdentityServer and Ping.
- **Middleware (L15) after real tokens and partners exist**, so the library solves real problems.
- **Customisation (L17–L18) after you know Keycloak well enough to know what *not* to customise.**
- **Troubleshooting (L20) last**, using your own broken flows.

---

## 2. Before Lesson 1 — setup

1. **Visual Studio 2026** with **ASP.NET and web development** (✓ installed). Check **Individual components**: .NET 10 Runtime (LTS), Git for Windows.
2. **GitHub account** (✓).
3. **Docker Desktop** (✓) — Engine running, ≥ 4 GB RAM in **Settings > Resources**. Needed from **L09**.
4. **A second browser profile** for logging in as two users — from L13.
5. **SAML-tracer** browser extension (verify) — L16.
6. **JDK 21** (Eclipse Temurin MSI, "Set JAVA_HOME" ✓ — verify the version the pinned Keycloak needs) and **IntelliJ IDEA** free edition (verify current packaging) — needed for **L17**. Install any evening before Day 5.
7. In **Claude**: Web search, Artifacts, Code execution and file creation on.

---

## 3. Lessons

# DAY 1 — .NET FOUNDATIONS

## L01 — Start the project: Visual Studio 2026, .NET 10 & your first API
**Day 1 · Slot 1**
**📍 Start:** nothing. **🏁 End:** a `CampusGate` solution with a running API; `CoursesController` returns fictional courses; tested from a `.http` file; pushed to GitHub; repo connected to the Claude Project.

### Concepts
🟢 **Core**
- .NET vs C# vs ASP.NET Core; SDK vs runtime; `net10.0`; **LTS** (to Nov 2028). ⚙️ C# → IL → JIT → the host starts **Kestrel**.
- Solution (`.slnx`) vs project; `Program.cs`, `appsettings.json`, `Properties/launchSettings.json`.
- **HTTP**: method, URL, status code, headers, body. 📜 One raw request/response, line by line.
- **Controllers & actions** ⚙️: `AddControllers()` discovers, `MapControllers()` maps routes; a new controller instance per request.

🔵 **Important**
- C# for today (class vs record, properties, `List<T>`, `var`, attributes, `=>`, string interpolation). F5 vs Ctrl+F5, breakpoints. `.http` files, Endpoints Explorer, OpenAPI at `/openapi/v1.json`. Controllers vs minimal APIs. **HTTPS dev certificate** (preview of L05).

🟣 C# 14 highlights · `dotnet run app.cs` · Scalar UI.

### ⚖️ 💣 ⬛
- ⚖️ Controllers (structure, filters, conventions) vs minimal APIs (less ceremony); LTS vs STS.
- 💣 Controller not found → 404 with empty body. 💣 Port in use / untrusted dev certificate. 💣 Different SDKs on different machines (`global.json`, L03).
- ⬛ Only LTS in production · pin the SDK · never disable HTTPS "to make it work".

### 🎬 Visuals
- 🎬 Pressing F5: build → Kestrel → request → routing → controller → JSON.

### 🛠️ Build
1. **Blank Solution** `CampusGate` (`.slnx`) with location `D:\` and **Place solution and project in the same directory** unchecked, so the solution is `D:\CampusGate\CampusGate.slnx`.
2. **ASP.NET Core Web API** `CampusGate.Api` in `src/` (.NET 10.0 LTS, Authentication **None**, HTTPS ✓, OpenAPI ✓, **Use controllers ✓**).
3. Send the sample request from the `.http` file; set a breakpoint; inspect.
4. `CoursesController`: `GET api/courses` (3 fictional courses: code, title, faculty, capacity) and `GET api/courses/{code}` (404 if unknown).
5. **Git > Create Git Repository** → push to the **public** GitHub repo `https://github.com/rabeefarhat/CampusGate` (explain public vs private: in a public repo anything committed, including history, is readable by anyone — so secrets must never be committed; secret scanning + push protection are free for public repos and turned on in L03).
6. Connect the repo: Project knowledge > **+** > **GitHub** → `CampusGate` → select `src/`, `.gitignore`, `CampusGate.slnx`.

### 🧪 Tests (Part 3)
Manual test plan in `CampusGate.Api.http` (unit tests start in L04): list → 200 with 3 items; known code → 200; unknown → 404; malformed code → explain 404 vs 400 (fixed in L02). 💥 Remove `MapControllers()` → every request 404 → restore.

### 🏁 Commit `feat: create CampusGate API with CoursesController`

### 🎤 Explain-it & interview
1. SDK vs runtime? 2. What happens when a request reaches your controller? 3. Why LTS? 4. Controllers vs minimal APIs? 5. 🎤 *"Walk me through an HTTP request from the browser to your code."*

---

## L02 — ASP.NET Core fundamentals + REST API design done right
**Day 1 · Slot 2**
**📍 Start:** hard-coded list. **🏁 End:** an injected store, `POST` enrolments with automatic validation, options + `ValidateOnStart`, ProblemDetails via `IExceptionHandler`, structured `ILogger`, a **correlation ID** middleware, `/health`; routes under `api/v1`, pagination, correct status codes, idempotent `PUT`/`DELETE`, a reviewed OpenAPI document and `docs/api-guidelines.md`.

### Concepts
🟢 **Core**
- **Middleware pipeline** (💡 airport checkpoints) ⚙️ — each middleware wraps the next; order is behaviour. *This is exactly where authentication and authorisation will plug in (L08) — remember the picture.*
- **DI & lifetimes** ⚙️ (singleton, scoped, transient); captive dependency.
- **REST** (💡 a library catalogue: everything is a resource with an address): resources, nouns in URLs, methods as verbs; **safe vs idempotent**; actions that aren't CRUD (`POST /grades/{id}/corrections`).
- **Status codes as a contract**: 200/201/204, 400/401/403/404/409/415/422/429, 500 — and the **security meaning of 401 vs 403 vs 404**. **ProblemDetails** (RFC 9457).

🔵 **Important**
- Controller request flow (routing → binding → validation → filters → action → result). Model validation with DataAnnotations → automatic 400. Configuration layers & options pattern. `ILogger` message templates (IDs only, never personal data). `IExceptionHandler` — **never leak stack traces**. **Correlation IDs** (`X-Correlation-Id`, `traceId` in ProblemDetails). Pagination (offset vs keyset), max page size. Versioning (URL `api/v1`). **API security basics**: no personal data or secrets in URLs (they land in logs); over-posting & DTOs. `async`/`await`, `CancellationToken`.

### ⚖️ 💣 ⬛
- ⚖️ Offset vs keyset pagination. ⚖️ URL vs header versioning. ⚖️ Filters vs middleware.
- 💣 Captive dependency. 💣 Middleware out of order. 💣 Over-posting (a client sets a field it shouldn't — a **security** bug). 💣 `GET` that changes state. 💣 Unbounded list. 💣 Stack trace in a 500 response.
- ⬛ Bind DTOs only · ProblemDetails without internals outside Development · fail fast on bad config · version from day one · max page size enforced server-side · correlation ID on every response.

### 🎬 Visuals
- 🎬 Request through the pipeline, exception caught on the way back → ProblemDetails. 🎬 Lifetimes across two requests. Diagram: CampusGate resource tree (courses → enrolments → grades).

### 🛠️ Build
1. `CampusGate.Contracts` (class library) with DTOs; `ICourseStore` + thread-safe in-memory store.
2. Routes under `api/v1`; `GET api/v1/courses?page=&pageSize=` → `PagedResponse<T>` (max 100).
3. `POST api/v1/courses/{code}/enrolments` → 201 `CreatedAtAction`; `PUT`/`DELETE` on a course (idempotent).
4. `Grading` options (`MaxPastDays = 30`, `MaxGrade = 20`) with `ValidateOnStart`.
5. `IExceptionHandler` + ProblemDetails with `errorCode` and `traceId`; `CorrelationIdMiddleware`; `MapHealthChecks("/health")`.
6. `[ProducesResponseType]` on actions; review `/openapi/v1.json`. Write `docs/api-guidelines.md`.

### 🧪 Tests (Part 3)
Manual `.http` plan + `docs/api-checklist.md`: valid POST → 201 + `Location`; missing field → 400; extra `id` in body ignored; pageSize 1000 → capped; `PUT` twice → same result; `DELETE` twice → 204 then 404 (explain); thrown exception → 500 without stack trace, with `traceId`; every response has `X-Correlation-Id`; bad config → startup failure. 💥 Swap the thread-safe store for a plain `List<T>` singleton → explain the race → restore.

### 🏁 Commit `feat: pipeline, DI, validation, ProblemDetails, REST v1 conventions`

### 🎤 Explain-it & interview
1. Middleware and order? 2. Lifetimes and captive dependency? 3. Safe vs idempotent? 4. 401 vs 403 vs 404? 5. 🎤 *"Where in the pipeline would you put a check that every request is authenticated, and why there?"* 6. 🎤 *"How would you version an API used by partner companies without breaking them?"*

---

## L03 — Git & DevOps foundations: branches, pull requests, CI & supply-chain hygiene
**Day 1 · Slot 3**
**📍 Start:** commits straight to `main`. **🏁 End:** protected `main`; `.gitignore`, `.gitattributes`, `global.json`; PR template; Conventional Commits; GitHub Actions CI building on every PR with `-warnaserror`; Dependabot; **secret scanning + push protection** enabled; one practised merge conflict.

### Concepts
🟢 **Core**
- **Git model** (💡 a photo album: each commit is a snapshot pointing to its parent) ⚙️: working tree → staging → commit → push; `HEAD`; branches as movable labels; remotes.
- **Branches, merge, squash, rebase**; resolving conflicts in Visual Studio's merge editor.
- **Pull requests, code review & branch protection** (require PR, require status checks, no force-push); what a security-minded reviewer looks for.
- **CI with GitHub Actions** (💡 an automatic inspector on every change) ⚙️: workflow (YAML) → trigger → job → runner (fresh VM) → steps → logs → status check blocks the merge.

🔵 **Important**
- `.gitignore` (never `bin/`, `obj/`, `*.user`, secrets). **A secret committed once stays in history** → rotate first, then clean. Conventional Commits. Trunk-based vs GitHub Flow vs GitFlow. `global.json`. **Dependabot** (alerts + version updates). **Secret scanning & push protection**. Least-privilege `permissions:` in workflows; pin actions (by version now, by SHA in L20). Tags & releases.

🟣 Interactive rebase · `git bisect` · signed commits.

### ⚖️ 💣 ⬛
- ⚖️ Merge (true history) vs squash (clean main) vs rebase (linear, rewrites history). ⚖️ Long-lived branches vs trunk-based.
- 💣 Secret pushed → leaked even after deletion. 💣 Force-push overwrites a colleague's work. 💣 "Works locally, fails in CI" (SDK, OS, paths). 💣 Huge PR nobody really reviews. 💣 `permissions: write-all` in a workflow.
- ⬛ Protect `main` · every change via PR with green CI · small PRs · secrets never in Git, push protection on · pin the SDK · automate dependency updates · least-privilege workflow tokens.

### 🎬 Visuals
- 🎬 Commit graph: branch, commits, PR, merge vs squash vs rebase. 🎬 A CI run: push → runner → restore → build → ✅/❌ blocks the merge.

### 🛠️ Build (Visual Studio Git windows + GitHub website)
1. `.gitignore` (Visual Studio template), `.gitattributes`, `global.json` (SDK 10.0.x, `rollForward: latestFeature`).
2. `.github/pull_request_template.md` (what / why / how tested / **security impact**).
3. `.github/workflows/ci.yml`: on PR and push to `main` → checkout → setup .NET from `global.json` → restore → build `-warnaserror` → test (empty until L04); `permissions: contents: read`.
4. GitHub: protect `main` (Settings > Branches or Rulesets — verify labels): require PR, require `ci` check, block force-push. Settings > Code security: Dependabot alerts + `dependabot.yml` (NuGet, GitHub Actions), **secret scanning + push protection**.
5. Practise: `feature/l03-ci` → commits → PR → watch CI → squash-merge. Create and resolve a deliberate merge conflict.

### 🧪 Tests (Part 3)
CI is the test: a PR with a compile error is blocked; a warning is blocked (`-warnaserror`); direct push to `main` refused. 💥 Try to push a fake-but-realistic-looking secret pattern on a branch → see push protection (or, if not detected, see it in history after deleting it) → discuss rotation and history rewriting → close the branch.

### 🏁 PR `ci: branch protection, CI workflow, Dependabot, secret scanning`

### 🎤 Explain-it & interview
1. What is a commit, really? 2. Merge vs rebase vs squash? 3. Why protect `main`? 4. What does CI do and what are runners? 5. 🎤 *"A developer pushed an API key to GitHub. What do you do, in order?"* 6. 🎤 *"What makes a GitHub Actions workflow secure?"*

---

## L04 — Clean Architecture, the domain, MediatR, FluentValidation, behaviors & CQRS
**Day 1 · Slot 4**
**📍 Start:** one API project with logic in controllers. **🏁 End:** Domain / Application / Infrastructure / Api projects; `StudentNumber` and `CourseCode` value objects (Luhn check), `Course` aggregate with the enrolment and grading rules; MediatR **12.5.0** commands and queries; FluentValidation; logging + validation behaviors; thin controllers; an **`ICurrentUser` port** (fixed ⚠️ DEV ONLY user until L08); tests (xUnit, FluentAssertions 7.x, NSubstitute, Bogus) and **architecture tests**, all running in CI.

### Concepts
🟢 **Core**
- **Clean Architecture** (💡 an onion) ⚙️: the dependency rule, enforced by project references; ports & adapters; composition root.
- **Domain modelling for security**: entity vs value object, invariants in factory methods ("parse, don't validate"), **input validation is a security control** (allow-list formats like `StudentNumber`).
- **MediatR, CQRS & pipeline behaviors** ⚙️: one handler per use case (load → domain → save); commands change state, queries read; behaviors wrap every request (logging → validation → [authorisation slot for L08] → handler).
- **Testing toolkit**: AAA, `[Fact]`/`[Theory]`, FluentAssertions, NSubstitute for ports only, Bogus with a fixed seed, architecture tests (`NetArchTest.Rules` or `TngTech.ArchUnitNET` — verify).

🔵 **Important**
- FluentValidation vs DataAnnotations vs domain rules (three layers). **`ICurrentUser`**: Application asks "who is acting?" without knowing about tokens or `HttpContext`. Commands as an allow-list (anti mass-assignment). Licences: MediatR 12.5.0 and FluentAssertions 7.x pinned. Typed IDs. `TimeProvider`/`FakeTimeProvider` for "not in the future".

### 🧩 DDD notes
- Value object (`StudentNumber`), aggregate (`Course` owns its enrolments and grades, so "never over capacity" and "no double enrolment" are enforced in one place, even when two students enrol at the same moment).

### ⚖️ 💣 ⬛
- ⚖️ Clean Architecture vs CRUD ceremony. ⚖️ MediatR (uniform pipeline) vs indirection. ⚖️ Exceptions vs Result types.
- 💣 Domain referencing infrastructure. 💣 Fat controllers. 💣 Behaviors registered in the wrong order (validation after the handler). 💣 `DateTime.Now` in the domain. 💣 Over-mocking.
- ⬛ No `HttpContext` in Application · validate every command · state changes only through named domain methods · architecture rules as tests in CI · never mock what you own.

### 🎬 Visuals
- 🎬 The onion; an outward arrow turns red. 🎬 Request → Logging → Validation → Handler; an invalid command stopped.

### 🛠️ Build
1. `CampusGate.Domain`, `.Application`, `.Infrastructure`; references; `AddApplication()` / `AddInfrastructure()`.
2. Domain: `StudentNumber`, `CourseCode`, `Course` (`Enrol`, `RecordGrade`, `PublishGrades`, `CorrectGrade`), `Enrolment`, `Grade`, `DomainException` → 409 via the exception handler.
3. MediatR 12.5.0 + FluentValidation; use cases `CreateCourse`, `EnrolStudent`, `RecordGrade`, `CorrectGrade`, `GetCourse`, `ListCourses`; `LoggingBehavior`, `ValidationBehavior`.
4. `ICurrentUser` (Application) + `DevCurrentUser` (Infrastructure, ⚠️ DEV ONLY).
5. Thin controllers sending commands/queries.
6. Test projects + `CampusGate.Testing` (`StudentNumberFaker`, fixed seed); `docs/adr/ADR-001-clean-architecture.md`; `ci.yml` runs `dotnet test`.

### 🧪 Tests (Part 3)
`StudentNumber` theory (valid, 7/9 digits, letters, wrong check digit, wrong prefix, equality) · 1,000 Bogus student numbers all valid · enrolment rules (double enrolment fails; capacity reached → fails; after deadline fails) · grading rules (21 fails; published grade changed without correction fails; teacher correction with reason passes; future exam date fails; boundary 30 days ± 1 tick) · validators per rule · handler with NSubstitute store · behavior order · architecture: Domain references nothing, Application has no ASP.NET Core / Infrastructure reference. 💥 Add a Domain → Infrastructure reference → architecture test red locally **and in the PR** → remove.

### 🏁 PR `feat: Clean Architecture, domain, MediatR use cases, tests`

### 🎤 Explain-it & interview
1. The dependency rule? 2. Value object vs entity? 3. What do behaviors give you? 4. CQRS in one sentence — and when is it overkill? 5. 🎤 *"Why shouldn't your application layer read `HttpContext.User` directly?"* 6. 🎤 *"Where should an authorisation rule like 'teachers only grade courses in their own faculty' live?"*

---

# DAY 2 — SECURITY FOUNDATIONS

> From here on the course assumes **zero prior security knowledge**: every term gets a 📖 box, every protocol a 📜 raw HTTP trace, every failure its exact error message.

## L05 — Web security I: HTTP, TLS & certificates, origins, cookies, CORS
**Day 2 · Slot 1**
**📍 Start:** HTTPS you've never looked at. **🏁 End:** you can read the TLS connection, the certificate chain, every cookie attribute and every CORS header; HSTS and HTTPS redirection configured correctly; `CampusGate.Security.Tests` created; `docs/security/web-security-basics.md` in your own words.

### Concepts
🟢 **Core**
- **Threats** (💡 a letter through the post): eavesdropping, tampering, impersonation → confidentiality, integrity, authenticity.
- **HTTPS/TLS** ⚙️: handshake (client hello → certificate → key exchange → encrypted channel); TLS 1.2 vs 1.3; what TLS does **not** do (it doesn't prove who the *user* is).
- **Certificates & chain of trust** (💡 a passport signed by a government your country recognises): subject, SAN, issuer, validity, root/intermediate CAs, Windows certificate store, the ASP.NET Core dev certificate.
- **Origins & same-origin policy** ⚙️: scheme + host + port; the browser blocks *reading* cross-origin responses, not *sending* requests (root of CSRF).
- **Cookies** ⚙️: `Set-Cookie`, `Domain`, `Path`, `Expires`, **`Secure`, `HttpOnly`, `SameSite` (Strict/Lax/None)**, `__Host-` prefix.

🔵 **Important**
- **CORS** ⚙️: simple vs preflight (`OPTIONS`), allow headers; CORS *relaxes* the same-origin policy — it never adds protection. **HSTS** & HTTPS redirection. Reverse proxies terminating TLS and `X-Forwarded-Proto` (it matters again in L13, L20). `localhost` vs `127.0.0.1` are different origins.

🟣 Certificate transparency · OCSP · cipher suites · HTTP/2 & 3.

### ⚖️ 💣 ⬛
- ⚖️ `SameSite=Strict` vs `Lax` vs `None` (needed for some federated flows — remember for OIDC/SAML).
- 💣 Expired/hostname-mismatched certificate → `NET::ERR_CERT_*` / `The remote certificate is invalid according to the validation procedure`. 💣 Disabling certificate validation in `HttpClient` → man-in-the-middle. 💣 `AllowAnyOrigin` + credentials. 💣 Cookie without `Secure`. 💣 HSTS on a dev domain locks you out.
- ⬛ HTTPS everywhere, HSTS in production · never disable certificate validation · cookies `Secure; HttpOnly; SameSite=Lax` or stricter · explicit CORS allow-list · forward proxy headers deliberately.

### 🎬 Visuals
- 🎬 TLS handshake. 🎬 Chain of trust. 🎬 Same-origin policy: request sent, response blocked. 🎬 CORS preflight.

### 🛠️ Build
1. Browser dev tools tour (Network with *Preserve log*, Security/certificate viewer, Application > Cookies). 📜 Read one full HTTPS exchange.
2. Windows **Manage user certificates** → find the ASP.NET Core dev certificate; explain trust.
3. API: `UseHttpsRedirection`, `UseHsts` (non-Development only); a named CORS policy for one test origin.
4. A test endpoint that sets a cookie with and without attributes → inspect in dev tools; `document.cookie` can't read `HttpOnly`.
5. `docs/security/web-security-basics.md`.

### 🧪 Tests (Part 3)
`CampusGate.Security.Tests` with `WebApplicationFactory`: HTTP → redirect to HTTPS; Production environment sends `Strict-Transport-Security`; preflight from allowed origin → `Access-Control-Allow-Origin`, from another → none; cookie is `HttpOnly; Secure; SameSite=Lax`. 💥 `AllowAnyOrigin().AllowCredentials()` → see the exception ASP.NET Core throws → restore.

### 🏁 PR `feat(security): HTTPS, HSTS, cookie defaults, CORS policy`

### 🎤 Explain-it & interview
1. What does TLS protect, and what doesn't it? 2. How does the browser trust a certificate? 3. What's an origin? 4. `HttpOnly`, `Secure`, `SameSite`? 5. 🎤 *"CORS is configured — does that protect our API from other websites?"* 6. 🎤 *"A service throws 'The remote certificate is invalid'. What do you check?"*

---

## L06 — Web security II: attacks & defences (OWASP Top 10)
**Day 2 · Slot 2**
**📍 Start:** you know the browser's rules. **🏁 End:** you've attacked and fixed a localhost-only `Playground.VulnerableApp` (XSS, CSRF, open redirect, SSRF); CampusGate has a `SecurityHeadersMiddleware` (CSP, `frame-ancestors`, `nosniff`, `Referrer-Policy`) in `CampusGate.ServiceDefaults`, a local-redirect helper for `returnUrl`, an outbound-URL allow-list handler, and `docs/security/owasp-map.md`.

### Concepts
🟢 **Core**
- **OWASP Top 10 & API Security Top 10** (verify current editions) — CampusGate's map.
- **XSS** (💡 a note slipped into your letter that the reader obeys): stored, reflected, DOM; Razor encodes by default, `@Html.Raw` breaks it; **CSP** as a second wall; why XSS is fatal for tokens in the browser (→ BFF in L13).
- **CSRF** ⚙️: the browser sends cookies automatically; defences: `SameSite`, **antiforgery tokens**, no state change on `GET`; why bearer-token APIs aren't CSRF-prone but cookie-based apps are.
- **Broken access control & injection**: BOLA/IDOR preview (L08), mass assignment (L02), SQL injection (parameters — concept).

🔵 **Important**
- **Open redirect** (`returnUrl` abuse — critical for login flows: `LocalRedirect`, `Url.IsLocalUrl`). **SSRF** (cloud metadata `169.254.169.254`; allow-list outbound hosts). Clickjacking. Security headers. Vulnerable dependencies (Dependabot from L03).

🟣 Subresource Integrity · Trusted Types · login rate limiting (L15).

### ⚖️ 💣 ⬛
- ⚖️ Strict CSP (strong) vs breaking third-party scripts.
- 💣 `@Html.Raw(userInput)` → stored XSS. 💣 State-changing `GET` → CSRF via an `<img>` tag. 💣 `Redirect(returnUrl)` → phishing through your own login page. 💣 "Fetch this URL" endpoint → reads cloud metadata.
- ⬛ Encode output, never trust input · CSP + security headers on every response · antiforgery on every cookie-authenticated state change · only local redirects after login · allow-list outbound URLs · keep dependencies patched.

### 🎬 Visuals
- 🎬 Stored XSS stealing a token from `localStorage` vs failing against an `HttpOnly` cookie. 🎬 CSRF with and without `SameSite`/antiforgery. 🎬 Open redirect after login.

### 🛠️ Build
1. `tools/Playground.VulnerableApp` (Razor Pages, localhost only, labelled): attack and fix XSS, CSRF, open redirect, SSRF.
2. `CampusGate.ServiceDefaults` class library: `SecurityHeadersMiddleware`, `AddCampusGateDefaults()` (correlation ID moves here too).
3. `SafeRedirect` helper; `AllowListOutboundHandler` (`DelegatingHandler`) for later partner calls.
4. `docs/security/owasp-map.md`.

### 🧪 Tests (Part 3)
Every API response has the security headers and CSP; Playground fixed version: `returnUrl=https://evil.example` → redirected to `/`; POST without antiforgery → 400; outbound handler refuses `http://169.254.169.254`; `<script>` input rendered as text. 💥 Swap `LocalRedirect` for `Redirect` → open-redirect test red → restore.

### 🏁 PR `feat(security): security headers, CSP, redirect & SSRF guards`

### 🎤 Explain-it & interview
1. XSS types and defences? 2. Why are cookie apps CSRF-prone and bearer APIs not? 3. What does CSP do? 4. What is SSRF? 5. 🎤 *"Why is an open redirect on a login page dangerous?"* 6. 🎤 *"Name three OWASP risks and how your app defends against each."*

---

## L07 — Cryptography for identity: hashing, HMAC, signatures, JWT, keys & certificates
**Day 2 · Slot 3**
**📍 Start:** you know TLS uses certificates. **🏁 End:** a `Playground.Crypto` console (SHA-256, HMAC, RSA/ECDSA sign/verify, self-signed certificate, JWK export); `tools/CampusGate.TokenInspector` decodes JWTs and SAML responses **locally**, checks signatures against a JWKS URL, and generates PKCE verifier/challenge pairs; Data Protection explained; `docs/security/crypto-cheatsheet.md`.

### Concepts
🟢 **Core**
- **Encoding vs hashing vs encryption vs signing** (💡 translating vs fingerprinting vs locking vs sealing with wax). Base64url is **encoding — anyone can read it**.
- **Hashes & HMAC** ⚙️: SHA-256 properties; HMAC = hash + shared secret (HS256 JWTs).
- **Symmetric vs asymmetric** ⚙️: shared key vs key pairs; **sign with the private key, verify with the public key** — the basis of RS256/ES256 tokens and SAML signatures.
- **JWT anatomy & JWK/JWKS** ⚙️: header (`alg`, `kid`, `typ`), payload (claims), signature; JWKS publishes public keys by `kid`; **key rotation** (publish new key, sign with it, keep the old until tokens expire).

🔵 **Important**
- X.509 certificates for signing (SAML, `private_key_jwt`). **Password hashing** (PBKDF2/bcrypt/Argon2 — slow and salted; `PasswordHasher`). Secure randomness (`RandomNumberGenerator`) for `state`, `nonce`, PKCE. **ASP.NET Core Data Protection** (key ring; why cookies break when it isn't shared across instances — again in L13). Constant-time comparison. Never invent your own crypto.

🟣 AEAD (AES-GCM) · elliptic curves intuition · post-quantum awareness · JWE.

### ⚖️ 💣 ⬛
- ⚖️ HS256 (simple, but every verifier can *forge*) vs RS256/ES256 (only the issuer signs; anyone verifies — right for many services).
- 💣 Plain SHA-256 for passwords → cracked offline. 💣 Signing key in `appsettings.json` → anyone with the repo forges tokens. 💣 `alg: none` / algorithm confusion when validation isn't strict. 💣 `kid` not found after rotation → `IDX10503` / `IDX10500`. 💣 Data Protection keys not persisted → users logged out on every deploy.
- ⬛ Asymmetric signing for tokens · pin allowed algorithms · rotate keys with overlap · keys in a vault/HSM · framework crypto only · persist and protect Data Protection keys · decode tokens locally, never online.

### 🎬 Visuals
- 🎬 Sign with private key → verify with public key; tampered payload fails. 🎬 Key rotation timeline with two keys in the JWKS. 🎬 Salted password hashing vs rainbow table.

### 🛠️ Build
1. `tools/Playground.Crypto` console: each primitive, step by step.
2. `tools/CampusGate.TokenInspector` console: decode JWT (header/payload pretty-printed, `exp`/`iat`/`nbf` as UTC + "expires in"), optional signature check against a JWKS URL, decode a Base64 SAML response to XML, generate a PKCE pair. ⚠️ For fictional/lab tokens only.
3. `docs/security/crypto-cheatsheet.md`.

### 🧪 Tests (Part 3)
HMAC changes when one byte changes; signature fails for a modified payload and for the wrong public key; TokenInspector parses a known JWT and reports `exp` correctly; PKCE S256 helper matches the **RFC 7636 Appendix B test vector**; password hasher: same password → different hashes, both verify; Data Protection round trip. 💥 Validate an RS256 token while accepting any algorithm → show why `ValidAlgorithms` matters → restore.

### 🏁 PR `feat(tools): crypto playground and TokenInspector`

### 🎤 Explain-it & interview
1. Hashing vs encryption vs signing? 2. Is a JWT encrypted? 3. HS256 vs RS256 for microservices? 4. How does key rotation work with JWKS? 5. 🎤 *"How should passwords be stored?"* 6. 🎤 *"What's in an X.509 certificate and how is it validated?"*

---

## L08 — Authentication & authorisation in ASP.NET Core: the middleware, JWT, policies & BOLA
**Day 2 · Slot 4**
**📍 Start:** everyone can do everything; `ICurrentUser` is fake. **🏁 End:** **`CampusGate.Auth`** (class library) is born: JWT bearer validation with explicit parameters, `HttpCurrentUser`, policies `Student`, `Teacher`, `Registrar`, a **fallback policy**, resource-based authorisation for courses and grades, filtered list queries, an `AuthorizationBehavior` in Application; tokens come from a **temporary dev issuer you build yourself** (⚠️ DEV ONLY) so you understand what a token server does; a BOLA test suite.

### Concepts
🟢 **Core**
- **Authentication vs authorisation** (💡 badge at the entrance vs each door). Claims, `ClaimsIdentity`, `ClaimsPrincipal`.
- **The ASP.NET Core auth middleware** ⚙️: `AddAuthentication` registers **schemes** → each scheme has a **handler** → `UseAuthentication` runs the default handler → `HttpContext.User` → `UseAuthorization` evaluates endpoint metadata → **challenge (401)** vs **forbid (403)**. Order and why.
- **JWT bearer validation** ⚙️: signature (key/`kid`) → issuer → audience → lifetime (+ clock skew); `TokenValidationParameters`; claim-type mapping surprises (`MapInboundClaims = false`).
- **Policies & resource-based authorisation**: requirements + handlers, fallback policy, `IAuthorizationService.AuthorizeAsync(user, resource, requirement)`; **BOLA** (OWASP API #1).

🔵 **Important**
- Role vs policy vs resource-based. Filter lists by user *and* check single resources; 404 vs 403 for other people's data. `HttpCurrentUser` (adapter). `AuthorizationBehavior` for commands marked `IRequireRole`. Why the dev issuer is ⚠️ DEV ONLY and what a real authorization server adds (L09). Designing `CampusGate.Auth` as a library: one `AddCampusGateAuthentication(configuration)` extension, options validated on start.

### ⚖️ 💣 ⬛
- ⚖️ Self-contained JWT (no lookup) vs revocation → short lifetimes. ⚖️ 404 (hides existence) vs 403 (clearer).
- 💣 `UseAuthorization` before `UseAuthentication` → everyone anonymous. 💣 New controller without `[Authorize]` (fallback policy prevents it). 💣 `IDX10223` lifetime expired / clock skew. 💣 Role claim not found due to claim mapping → 403 for everyone. 💣 Trusting a student ID from the request body.
- ⬛ Fallback policy = authenticated user · least privilege · authorise every object access · validate issuer/audience/lifetime/signature with pinned algorithms · never trust client-sent identity · auth configuration lives in one shared library.

### 🎬 Visuals
- 🎬 One request through authN → authZ → resource check (200/401/403/404). 🎬 JWT validation check by check. 🎬 BOLA attack with and without the check.

### 🛠️ Build
1. `CampusGate.Auth` library: `AddCampusGateAuthentication()` with JwtBearer (10.0.x), `ValidAlgorithms = [RS256]`, `MapInboundClaims = false`, `ClockSkew` 30 s; options + `ValidateOnStart`.
2. `DevTokenController` in the Api (Development only — filter returns 404 elsewhere) signing RS256 with a dev key from **User Secrets**; dev JWKS endpoint.
3. `HttpCurrentUser : ICurrentUser` (`StudentId`, `Faculty`, roles).
4. Policies; fallback policy; `GradeAuthorizationHandler` (student owns it / teacher of that course in that faculty / registrar); filtered list query; `AuthorizationBehavior`.
5. `.http` file: get a dev token, call endpoints as different users.

### 🧪 Tests (Part 3)
`CampusGate.Testing/TestTokens` (signs in-process). No token → 401 · expired → 401 · wrong issuer → 401 · wrong audience → 401 · `alg: none` → 401 · student A reads student B's grades → 404 · enrolment list never contains other students' rows (Bogus 3 × 20) · science teacher grades an arts course → 403 · dev token endpoint → 404 in Production · reflection test: every action is authorised or explicitly `[AllowAnonymous]` from an allow-list. 💥 Remove the resource check → BOLA test red → restore.

### 🏁 PR `feat(auth): CampusGate.Auth, JWT validation, policies, BOLA tests`

### 🎤 Explain-it & interview
1. Schemes, handlers, challenge, forbid? 2. What does JWT bearer validate, in order? 3. Role vs policy vs resource-based? 4. BOLA and how you test it? 5. 🎤 *"Why shouldn't every API issue its own tokens?"* 6. 🎤 *"Users get 403 though their token has the role. What do you check?"*

---

# DAY 3 — PROTOCOLS & KEYCLOAK

## L09 — OAuth 2.0 from first principles
**Day 3 · Slot 1**
**📍 Start:** your own dev issuer. **🏁 End:** you perform **authorization code + PKCE** and **client credentials** **by hand** with `.http` files and a browser against a throwaway Keycloak (`start-dev`, ⚠️ DEV ONLY, realm `oauth-lab`); every parameter read and decoded with TokenInspector; refresh, revoke, introspect done; `docs/oauth/flows.http` and `docs/oauth/glossary.md`.

### Concepts
🟢 **Core**
- **The problem OAuth solves** (💡 a valet key): delegated, limited access without sharing passwords. **OAuth is authorisation, not authentication.**
- **Roles & endpoints**: resource owner, client, authorization server, resource server; authorize, token, metadata (RFC 8414), revocation (RFC 7009), introspection (RFC 7662).
- **Authorization code + PKCE** 📜 (RFC 6749 + 7636): front channel (`response_type`, `client_id`, `redirect_uri`, `scope`, `state`, `code_challenge`, `code_challenge_method`) → login → redirect with `code` & `state` → back channel `POST /token` with `code_verifier` → tokens. Why `state`, why PKCE, why the code is single-use and short-lived.
- **Client credentials** 📜: machine-to-machine; confidential clients; client authentication (secret, `private_key_jwt`, mTLS).

🔵 **Important**
- Access tokens (JWT profile RFC 9068 vs opaque + introspection), **refresh tokens & rotation**, scopes vs roles, **audience**. Public vs confidential clients. **Deprecated:** implicit and password grants — why. **OAuth 2.1** & **RFC 9700**. Device flow (RFC 8628). Token exchange (RFC 8693) and DPoP (RFC 9449) — preview of L14.

🟣 PAR (RFC 9126) · RAR · FAPI 2.0.

### ⚖️ 💣 ⬛
- ⚖️ JWT access tokens (fast local validation, hard to revoke) vs opaque + introspection (revocable, network call). ⚖️ Short vs long access token lifetimes.
- 💣 `invalid_grant` (code reused/expired, wrong `redirect_uri`, bad `code_verifier`). 💣 `invalid_client`. 💣 `Invalid parameter: redirect_uri` (exact match!). 💣 No `state` check → login CSRF. 💣 Tokens in URLs (implicit) → leaked via history/referrer/logs.
- ⬛ Code + PKCE for every client · exact redirect URIs · short-lived access tokens, rotated refresh tokens · least-privilege scopes · strong client authentication · never implicit or password grants.

### 🎬 Visuals
- 🎬 Auth code + PKCE (front vs back channel coloured) → a stolen-code attack failing because of PKCE. 🎬 Client credentials. 🎬 Refresh rotation and reuse detection.

### 🛠️ Build
1. Docker Desktop: `quay.io/keycloak/keycloak:<pinned version>` with `start-dev` (⚠️ DEV ONLY); realm `oauth-lab`, one public client, one confidential client, one fictional user (click paths given; Keycloak properly in L11).
2. `docs/oauth/flows.http`: discovery → build the authorize URL (PKCE pair from TokenInspector) → log in → copy `code` → token request → userinfo → refresh → revoke → introspect. Client credentials request.
3. Decode and annotate every token (`iss`, `sub`, `aud`, `azp`, `scope`, `exp`, `iat`, `jti`).
4. `docs/oauth/glossary.md` in your own words.

### 🧪 Tests (Part 3)
`CampusGate.Security.Tests/OAuthLabTests` with **Testcontainers Keycloak** (verify package) importing the exported lab realm: client credentials returns a token with expected `scope`; wrong secret → `invalid_client`; unsupported grant → `unsupported_grant_type`/`unauthorized_client`; token endpoint error bodies match RFC 6749 §5.2; the browser part stays a written manual checklist. 💥 Reuse an authorization code by hand → `invalid_grant` → explain.

### 🏁 PR `docs(oauth): hand-run OAuth 2.0 flows and lab tests`

### 🎤 Explain-it & interview
1. The four roles? 2. Walk through auth code + PKCE with every parameter. 3. Why is implicit deprecated? 4. JWT vs opaque access tokens? 5. 🎤 *"Is OAuth an authentication protocol?"* 6. 🎤 *"What does PKCE protect against, and why do confidential clients use it too?"* 7. 🎤 *"How would you revoke access for a stolen JWT?"*

---

## L10 — OpenID Connect: identity on top of OAuth 2.0
**Day 3 · Slot 2**
**📍 Start:** OAuth by hand. **🏁 End:** `tools/Playground.OidcWeb` (Razor Pages) logs in with ASP.NET Core's **OpenID Connect handler** against `oauth-lab`: cookie session, visible `state`/`nonce` handling, claims page, userinfo, **RP-initiated logout**; handler events logged (tokens redacted); `docs/oidc/id-vs-access-token.md`.

### Concepts
🟢 **Core**
- **OIDC = OAuth 2.0 + identity**: `openid` scope; **ID token** (who logged in — for the client) vs **access token** (what the client may do — for the API). Never send ID tokens to APIs.
- **ID token validation** ⚙️: signature, `iss`, `aud` = client ID, `exp`, **`nonce`**, `azp`, `at_hash`, `auth_time`, `acr`/`amr`.
- **Discovery & JWKS** 📜: `/.well-known/openid-configuration`, `jwks_uri`, key caching and refresh on unknown `kid`.
- **The ASP.NET Core OIDC handler** ⚙️: challenge → redirect → `/signin-oidc` → code redemption → claims → **cookie scheme signs the user in**; `SaveTokens`; events (`OnRedirectToIdentityProvider`, `OnTokenValidated`, `OnRemoteFailure`).

🔵 **Important**
- Standard scopes & claims, userinfo. `prompt`, `max_age`, `login_hint`, `acr_values` (step-up/MFA). **Logout:** RP-initiated (`end_session_endpoint`, `id_token_hint`, `post_logout_redirect_uri`), front-channel, **back-channel** (best for BFFs). Session lifetime vs token lifetime.

### ⚖️ 💣 ⬛
- ⚖️ Claims in the ID token (fast, bigger cookie) vs userinfo (extra call, fresh). ⚖️ Front- vs back-channel logout.
- 💣 "Correlation failed" (state cookie lost: `SameSite`, HTTP vs HTTPS, multiple instances without shared Data Protection). 💣 `IDX21323` nonce invalid. 💣 Cookie too large → HTTP 431 / "Request Header Fields Too Large". 💣 Logout clears only the local cookie → silent re-login. 💣 ID token sent to an API.
- ⬛ Code + PKCE for OIDC login · never disable `state`/`nonce` validation · keep cookies small · implement IdP logout · correct `SameSite`/HTTPS · shared Data Protection across instances.

### 🎬 Visuals
- 🎬 OIDC login through the .NET handler. 🎬 RP-initiated vs back-channel logout.

### 🛠️ Build
1. `Playground.OidcWeb`: `AddCookie` + `AddOpenIdConnect` (`Authority`, `ResponseType = code`, `UsePkce = true`, `MapInboundClaims = false`, scopes, `SaveTokens`), a claims page, logout.
2. Log handler events (redacting tokens) with Serilog.
3. Read the discovery document; watch the JWKS fetch.
4. `docs/oidc/id-vs-access-token.md`.

### 🧪 Tests (Part 3)
Unit: the options factory sets code flow, PKCE, `MapInboundClaims = false`, required scopes, `RequireHttpsMetadata = true` outside Development. Integration (Keycloak fixture): discovery `issuer` matches the configured authority **exactly**; JWKS contains the `kid` of a freshly issued token. Manual checklist: login, claims page, logout clears the IdP session. 💥 Force `SameSite=Strict` on the correlation cookie → "Correlation failed" → explain → restore.

### 🏁 PR `feat(oidc): OIDC playground login and logout`

### 🎤 Explain-it & interview
1. OAuth vs OIDC? 2. ID vs access token — who consumes each? 3. What's the nonce for? 4. How does the handler get signing keys? 5. 🎤 *"Users see 'Correlation failed' after login. Walk me through your diagnosis."* 6. 🎤 *"How do you log a user out everywhere?"*

---

## L11 — Keycloak fundamentals
**Day 3 · Slot 3**
**📍 Start:** a throwaway lab realm. **🏁 End:** `identity/docker-compose.yml` with Keycloak (pinned) + **PostgreSQL** + Seq; realm `campusgate` from §1.4 (roles, groups, fictional users with `student_id`/`faculty`, clients, client scopes, audience mapper); password policy, brute-force detection, OTP for admins; token & session lifetimes chosen and documented; events on; realm **exported to `identity/keycloak/realm-campusgate.json`** (no secrets) and imported on start.

### Concepts
🟢 **Core**
- **What Keycloak is** (💡 the company's badge office): open-source (Apache 2.0) IAM server — OIDC/OAuth 2.0 and SAML 2.0 provider, user store, identity broker, user federation. **Realms**; master realm only for administration.
- **Clients** ⚙️: OIDC vs SAML; **client authentication** on/off (confidential vs public); flows allowed (standard flow, direct access grants **off**, service accounts); valid redirect URIs, web origins, PKCE method.
- **Users, groups, roles, client scopes & protocol mappers**: realm vs client roles, composites, groups as role carriers; what ends up in a token (`realm_access.roles`, audience mapper → `aud: campusgate-api`, user-attribute mappers).
- **Authentication flows & policies**: browser flow, required actions, password policy, **brute-force detection**, OTP/WebAuthn, SSO session idle/max, token lifespans, refresh-token rotation ("revoke refresh token").

🔵 **Important**
- **Dev vs production mode** ⚙️ (`start-dev` vs `start --optimized`; PostgreSQL; HTTPS; `KC_HOSTNAME`; proxy headers; build-time vs runtime options; health/metrics on the management port). **Events** (login + admin) — your audit trail for L20. **Realm export/import** as config as code (secrets not exported by default — verify). **Upgrade cadence:** no LTS. Organizations & Authorization Services — awareness.

### ⚖️ 💣 ⬛
- ⚖️ Realm per tenant vs one realm with Organizations. ⚖️ Realm roles vs client roles. ⚖️ Longer sessions vs exposure.
- 💣 Token `iss = http://localhost:8080/realms/campusgate` but the API in Docker expects `http://keycloak:8080/...` → `IDX10205`. 💣 Direct access grants left on → password grant usable. 💣 Wildcard redirect URI → code theft. 💣 Admin console public with a weak admin password. 💣 `start-dev` in production.
- ⬛ Pin an exact version and patch promptly · PostgreSQL, never the dev database · fixed public hostname · exact redirect URIs · disable unused grants · brute-force protection + MFA for privileged users · admin console restricted · events on and shipped to logs · realm config versioned without secrets.

### 🎬 Visuals
- Diagram: realm → clients → scopes → mappers → token. 🎬 Inside Keycloak during a login (flow → session → token with mappers).

### 🛠️ Build (🔑 console + 🐳 Docker Desktop)
1. `identity/docker-compose.yml`: `postgres`, `keycloak` (`start-dev` locally but **with PostgreSQL**, hostname set, volume), `seq` — every line explained.
2. Realm `campusgate`: roles, groups, 6 fictional users (2 students, 2 teachers science/arts, 1 registrar, 1 admin with OTP required).
3. Clients `campusgate-portal`, `campusgate-api`, `partner-aurora`; client scopes; audience mapper; attribute mappers for `student_id`, `faculty`.
4. Password policy, brute-force detection, lifespans (every number justified in `docs/identity/keycloak-settings.md`), refresh rotation, events + admin events.
5. **Realm settings > Action > Partial export** (verify label) → review for secrets → commit; import on start (`--import-realm`).

### 🧪 Tests (Part 3)
`KeycloakFixture` in `CampusGate.Testing` (Testcontainers, imports the realm): discovery `issuer` correct; partner token has `aud: campusgate-api`, only its scopes, no user claims; direct access grant on `campusgate-portal` → `unauthorized_client`; a user token (via a test-only direct-grant client existing **only in a test realm variant** — ⚠️ labelled) contains `realm_access.roles`, `student_id`; repeated wrong passwords → temporarily locked. 💥 Remove the audience mapper → `aud` test red → restore.

### 🏁 PR `feat(identity): Keycloak realm as code with PostgreSQL`

### 🎤 Explain-it & interview
1. Realm, client, client scope, mapper? 2. Confidential vs public client in Keycloak? 3. How does a role get into a token? 4. Dev vs production mode? 5. 🎤 *"Our API rejects Keycloak tokens with an issuer error only in Docker. Why?"* 6. 🎤 *"How do you manage Keycloak configuration across dev, test and prod?"*

---

## L12 — Securing the API with Keycloak
**Day 3 · Slot 4**
**📍 Start:** the API trusts your dev issuer. **🏁 End:** the API trusts **Keycloak** through configuration only (`Authority`, `Audience`); `KeycloakRolesClaimsTransformation` in `CampusGate.Auth` flattens `realm_access.roles`; policies combine **scopes and roles**; auth failures logged as structured security events (never the token) to Seq; all L08 security tests now run against real Keycloak tokens; the dev issuer is **deleted**. *Domain and Application didn't change.*

### Concepts
🟢 **Core**
- **Resource server configuration** ⚙️: `Authority` → discovery → JWKS (cached, refreshed on unknown `kid`); `Audience`/`ValidAudiences`; `ValidIssuer` must match **exactly** (scheme, host, port, path, trailing slash).
- **Claims transformation** ⚙️: Keycloak nests roles in `realm_access.roles` / `resource_access.<client>.roles`; `IClaimsTransformation` flattens them; runs on every authentication — cheap and idempotent.
- **Scopes vs roles in policies**: scope = what the *client* may do on the user's behalf; role = what the *user* may do; writes require both (`grades:write` **and** `teacher`).

🔵 **Important**
- Container networking: public vs internal issuer URL (`KC_HOSTNAME`, backchannel options — verify names; `MetadataAddress` vs `Authority`). `RequireHttpsMetadata` (⚠️ off only in Development). Lifetimes vs `ClockSkew`. `JwtBearerEvents` for security logging. `IdentityModelEventSource.ShowPII` — Development only.

### ⚖️ 💣 ⬛
- ⚖️ Local JWT validation (fast, offline) vs introspection (revocation, network). ⚖️ Roles in tokens (stateless) vs permissions looked up in CampusGate (fresh, needs caching).
- 💣 `IDX10205` issuer mismatch · `IDX10214` audience invalid · `IDX10500`/`IDX10503` signing key not found · `IDX10223` expired · roles missing (no claims transformation) → everyone 403.
- ⬛ Trust via configuration, not code · always validate audience · map claims explicitly · short `ClockSkew` · structured security logs without tokens · never call Keycloak per request.

### 🎬 Visuals
- 🎬 First request after startup: API fetches discovery + JWKS, caches; Keycloak rotates keys → unknown `kid` → refresh → success.

### 🛠️ Build
1. `Authentication` config section (`Authority`, `Audience`, `RequireHttpsMetadata`); `CampusGate.Auth` reads it.
2. `KeycloakRolesClaimsTransformation` + a pure, unit-testable parser.
3. Policies with `ScopeRequirement` + role; `HttpCurrentUser` reads `student_id`/`faculty`.
4. `JwtBearerEvents.OnAuthenticationFailed`/`OnChallenge`/`OnForbidden` → Serilog security events (`SecurityEvent`, reason, `client_id`, correlation ID) → Seq.
5. Delete the dev issuer; `.http` files fetch tokens from Keycloak.

### 🧪 Tests (Part 3)
All L08 security tests rewired to `KeycloakFixture` tokens · token from another realm → 401 · token for another audience → 401 · role `teacher` without `grades:write` → 403 · claims transformation: nested roles → role claims, idempotent when run twice · issuer with/without trailing slash documented. 💥 Remove the claims transformation → role tests 403 → restore.

### 🏁 PR `feat(auth): trust Keycloak; remove dev token issuer`

### 🎤 Explain-it & interview
1. How does the API get Keycloak's public keys? 2. What does the claims transformation do? 3. Scope vs role? 4. Four `IDX` errors and their causes? 5. 🎤 *"How would you migrate an API from one identity provider to another with no downtime?"* (preview of L15/L19)

---

# DAY 4 — INTEGRATIONS & MIDDLEWARE

## L13 — The student portal as a BFF: OIDC login, sessions, refresh & logout
**Day 4 · Slot 1**
**📍 Start:** no portal. **🏁 End:** `CampusGate.Portal` (Razor Pages) is a **Backend-for-Frontend**: OIDC code + PKCE with a confidential client; the browser holds only a `__Host-` `HttpOnly; Secure; SameSite=Lax` cookie; tokens kept **server-side** (`ITicketStore`); YARP proxies `/api/*` and attaches the access token; refresh before expiry; RP-initiated + **back-channel logout**; antiforgery on state changes; persisted Data Protection keys.

### Concepts
🟢 **Core**
- **Where tokens should live** ⚙️: SPA with tokens in the browser (exposed to XSS) vs **BFF** (tokens server-side, browser has only a session cookie) — current recommendation for browser apps (IETF *OAuth 2.0 for Browser-Based Apps* — verify status).
- **OIDC handler + cookie + YARP** ⚙️: login → session cookie → `/api/*` → YARP transform adds `Authorization: Bearer` → API.
- **Refresh & session lifetime**: refresh in `CookieAuthenticationEvents.OnValidatePrincipal`, rotation, sign-out when refresh fails.
- **Logout done right**: local cookie + Keycloak session (RP-initiated) + back-channel logout invalidating the server-side session.

🔵 **Important**
- Server-side ticket store (small cookies). Data Protection key persistence and sharing across instances. Antiforgery for the BFF. `X-Forwarded-*` behind proxies (wrong scheme in `redirect_uri`). Duende BFF library as an alternative (licence — verify). Step-up with `acr_values` (optional).

### ⚖️ 💣 ⬛
- ⚖️ BFF (strongest browser security; needs a server and shared session) vs pure SPA (simpler hosting; XSS exposure).
- 💣 Cookie too big → 431. 💣 No refresh → users bounced after 5 minutes. 💣 Two instances without shared Data Protection → "Correlation failed"/random logouts. 💣 Logout leaves the Keycloak session → silent re-login. 💣 Proxy without antiforgery → CSRF.
- ⬛ BFF for sensitive browser apps · tokens never reach JavaScript · server-side ticket store · refresh with rotation · back-channel logout · antiforgery on every cookie-authenticated mutation · shared, protected Data Protection keys.

### 🎬 Visuals
- 🎬 BFF login → cookie → `/api/courses` → host adds bearer → API. 🎬 Token refresh in the host. 🎬 Same XSS attack: SPA loses tokens; BFF attacker can only act while the page is open.

### 🛠️ Build
1. `CampusGate.Portal` (Razor Pages, .NET 10) + `CampusGate.ServiceDefaults`.
2. Cookie + OIDC (client secret from **User Secrets**), `MapInboundClaims = false`, scopes, `SaveTokens` into an in-memory `ITicketStore` (distributed cache in production — say so).
3. YARP route `/api/{**catch-all}` → API with a transform adding the access token; API 401 → re-challenge.
4. Refresh logic; sign-out page (RP-initiated); back-channel logout endpoint registered in the Keycloak client.
5. Pages: my courses, enrol in a course (antiforgery), my grades, profile (claims shown).

### 🧪 Tests (Part 3)
Integration (portal + Keycloak fixture + echo backend): unauthenticated `/api/*` → 401 (no redirect for API calls); cookie flags correct; no response to the browser contains `access_token`/`refresh_token`/`eyJ`; YARP adds `Authorization`; an expired access token is refreshed; back-channel logout invalidates the session; POST without antiforgery → 400. 💥 Expose the access token in a page "for convenience" → the "no token in browser responses" test red → restore.

### 🏁 PR `feat(portal): Razor Pages BFF with OIDC, refresh and logout`

### 🎤 Explain-it & interview
1. BFF vs tokens in the browser? 2. How does the host get tokens to the API? 3. How do you refresh in a BFF? 4. RP-initiated vs back-channel logout? 5. 🎤 *"Design authentication for a browser app that shows students their grades."* 6. 🎤 *"After deploying a second instance, users get randomly logged out. Why?"*

---

## L14 — Service-to-service & partner integrations across the organisation
**Day 4 · Slot 2**
**📍 Start:** only humans call the API. **🏁 End:** `tools/CampusGate.PartnerUniversity.Client` calls `api/v1/partner/transcripts` with client credentials and scope `partner.transcripts:read`, restricted to its faculty (a hard-coded client claim); a cached `ClientCredentialsTokenHandler` (`DelegatingHandler`) in `CampusGate.Auth`; **token exchange** demonstrated (verify Keycloak's standard token exchange support); `private_key_jwt` client authentication for the partner; introspection on one high-risk endpoint; `docs/integrations/onboarding-guide.md`.

### Concepts
🟢 **Core**
- **Machine-to-machine with client credentials** ⚙️: one client per calling system, scopes per capability, audience per API, tokens cached until shortly before expiry.
- **Delegation across services**: forwarding the user token vs **token exchange** (RFC 8693) vs client credentials (loses the user) — choose per call; `sub`, `azp`, `act` claims.
- **Integrating across a large organisation**: an integration contract (OpenAPI + scopes + rate limits + error model), onboarding checklist, credential lifecycle (issue, rotate, revoke), environments, support via correlation IDs.

🔵 **Important**
- Client authentication: shared secret vs **`private_key_jwt`** (RFC 7523 signed assertion) vs **mTLS** (RFC 8705) vs DPoP (RFC 9449). Introspection, cached briefly. API keys: when acceptable (→ custom handler in L15). Webhooks with HMAC signatures. `Duende.AccessTokenManagement` as a library alternative (verify licence).

### ⚖️ 💣 ⬛
- ⚖️ Forward vs exchange vs client credentials. ⚖️ Shared secret (easy) vs `private_key_jwt`/mTLS (no shared secret, more setup).
- 💣 New token per call → Keycloak overload. 💣 One "god client" shared by many systems → no revocation granularity, no audit. 💣 Forwarded user token accepted by every API (audience too broad). 💣 Partner secret in a public repo.
- ⬛ One client per integration · narrowest scopes + dedicated audience · cache tokens · prefer asymmetric client auth · rotate credentials on a schedule · log `client_id`/`azp` on every request · document the contract.

### 🎬 Visuals
- 🎬 Partner → Keycloak (client credentials) → API (scope + faculty check). 🎬 Forward vs exchange vs client credentials, showing `aud`/`sub`/`act`.

### 🛠️ Build
1. Keycloak: `partner-aurora` with **Signed JWT** client authentication (verify label), scope `partner.transcripts:read`, hard-coded claim `faculty=science`.
2. API: `PartnerTranscriptsController` with `PartnerTranscripts` policy (client token only — no user — plus scope); faculty filter.
3. `ClientCredentialsTokenHandler` (cached, `TimeProvider`) in `CampusGate.Auth`.
4. `PartnerUniversity.Client` console signing its client assertion with a key from User Secrets.
5. Token exchange demo endpoint (optional path); introspection on `DELETE api/v1/courses/{code}`.
6. `docs/integrations/onboarding-guide.md`.

### 🧪 Tests (Part 3)
Partner token reads its faculty's transcripts, gets 404/empty for others, 403 on student endpoints; a user token gets 403 on the partner endpoint; token handler requests **one** token for 50 calls and a new one after expiry (`FakeTimeProvider`); exchanged token has the expected `aud` and still identifies the user; introspection says inactive after revocation → 401. 💥 Disable caching → count token requests → red → restore.

### 🏁 PR `feat(integration): partner API, client credentials, token exchange`

### 🎤 Explain-it & interview
1. How does a background service authenticate to an API? 2. Forward vs exchange vs client credentials? 3. `private_key_jwt` vs secret vs mTLS? 4. When introspection? 5. 🎤 *"Another department wants to call our API. Walk me through onboarding them securely."* 6. 🎤 *"A partner's client secret leaked. What's your plan?"*

---

## L15 — Building & maintaining authentication middleware
**Day 4 · Slot 3**
**📍 Start:** you use built-in handlers. **🏁 End:** `CampusGate.Auth` is a **versioned internal library**: a custom **API-key `AuthenticationHandler`** for a legacy attendance card-reader gateway, a **policy scheme** selecting Keycloak / a second issuer / API key, a `[RequireScope]` attribute with a dynamic `IAuthorizationPolicyProvider`, a uniform 401/403 ProblemDetails `IAuthorizationMiddlewareResultHandler`, a `SecurityContextMiddleware` enriching logs, rate limiting on partner endpoints, auth-failure metrics, `docs/auth-library/README.md` + `CHANGELOG.md`, and NuGet packing in CI.

### Concepts
🟢 **Core**
- **Writing an `AuthenticationHandler<TOptions>`** ⚙️: `HandleAuthenticateAsync` (NoResult / Fail / Success), `HandleChallengeAsync`, `HandleForbiddenAsync`; options validation; hashed keys; constant-time comparison.
- **Multiple schemes & policy schemes** ⚙️: `AddPolicyScheme` + `ForwardDefaultSelector` (peek at the token's `iss`), per-endpoint schemes; dual issuers for zero-downtime IdP migration.
- **Custom authorisation**: `IAuthorizationRequirement` + `AuthorizationHandler<T, TResource>`; `IAuthorizationPolicyProvider`; `IAuthorizationMiddlewareResultHandler`.
- **Middleware vs filter vs handler** — which concern goes where; **maintaining shared middleware**: semantic versioning, changelog, backwards compatibility, performance (no per-request I/O without cache), observability (security events + metrics), test matrix.

🔵 **Important**
- `JwtBearerEvents` as extension points; `IClaimsTransformation` pitfalls (per request, may run twice). Rate limiting (`AddRateLimiter`, partition by `client_id`). `System.Diagnostics.Metrics` counter for auth failures by reason. Packaging (`dotnet pack`, GitHub Packages — concept).

### ⚖️ 💣 ⬛
- ⚖️ Custom scheme (fits legacy) vs standard protocols (always preferred — API keys only with written justification). ⚖️ Dynamic policies (flexible) vs explicit (discoverable).
- 💣 Handler returns `Fail` instead of `NoResult` when its header is absent → breaks other schemes. 💣 Selector throws on malformed tokens → 500 instead of 401. 💣 Keys compared with `==` or stored in plain text. 💣 Claims transformation hitting a database per request. 💣 Middleware placed before `UseAuthentication` → no user.
- ⬛ Standard protocols first · `NoResult` when not applicable · hash and rotate API keys · consistent 401/403 · metrics for auth failures · treat auth middleware as a versioned library with tests, docs and a changelog.

### 🎬 Visuals
- 🎬 Policy scheme: request → selector peeks `iss` → forwards to Keycloak / second issuer / ApiKey handler. 🎬 Requirement handlers voting (succeed / fail / neither).

### 🛠️ Build
1. `ApiKeyAuthenticationHandler` (`X-Api-Key`, SHA-256-hashed keys in config, `client_name` claim) for `legacy-attendance-gateway` on one attendance ingestion endpoint.
2. Policy scheme `CampusGate` with selector (Bearer + `iss` → `Keycloak` scheme or a placeholder `Meridian` scheme for L19; `X-Api-Key` → ApiKey).
3. `[RequireScope]` + `ScopePolicyProvider` + `ScopeAuthorizationHandler`.
4. `ProblemDetailsAuthorizationResultHandler` (401/403 with `traceId`).
5. `SecurityContextMiddleware` (pushes `ClientId`, `UserId`, `AuthScheme` into Serilog `LogContext`).
6. Rate limiter on partner endpoints; auth-failure counter.
7. README, CHANGELOG (SemVer), `<PackageId>`/`<Version>` in the `.csproj`, CI packs the library as an artifact.

### 🧪 Tests (Part 3)
`CampusGate.Auth.Tests` with `TestServer`: missing header → `NoResult`; wrong key → 401 ProblemDetails; correct key → principal with `client_name`; `CryptographicOperations.FixedTimeEquals` used; selector: Keycloak token → Keycloak scheme, unknown issuer → 401 (not 500), garbage → 401; missing scope → 403 with `traceId`; logs carry `ClientId`; 11th partner call → 429. 💥 Make the API-key handler return `Fail` when the header is missing → Keycloak requests start failing → restore.

### 🏁 PR `feat(auth): API-key handler, policy scheme, dynamic policies, library packaging`

### 🎤 Explain-it & interview
1. `NoResult` vs `Fail`? 2. Two identity providers in one API? 3. When is a custom handler justified? 4. Middleware vs filter vs authorisation handler? 5. 🎤 *"Walk me through how you'd build and maintain shared auth middleware used by twenty services."* 6. 🎤 *"After a change to our auth library, three services return 500 instead of 401. How do you investigate?"*

---

## L16 — SAML 2.0: assertions, bindings, metadata & federation
**Day 4 · Slot 4**
**📍 Start:** OIDC everywhere. **🏁 End:** `CampusGate.Tuition` (Razor Pages, legacy style) signs staff in with **SAML 2.0** via `Sustainsys.Saml2.AspNetCore2` (verify version) against Keycloak as **SAML IdP**; metadata exchanged; signed assertions required; SP-initiated SSO and Single Logout; a full login captured with SAML-tracer and annotated with TokenInspector; `docs/saml/saml-cheatsheet.md`; brokering a SAML IdP into OIDC explained (and optionally built).

### Concepts
🟢 **Core**
- **SAML 2.0** (💡 a sealed, notarised letter of introduction): **IdP**, **SP**, **assertion** (subject/NameID, conditions — audience & time window, authentication statement, attributes), **XML signature**, optional encryption.
- **Bindings & profiles** 📜: SP-initiated Web SSO — `AuthnRequest` via **HTTP-Redirect** (deflated, Base64, query string) → login → `Response` via **HTTP-POST** (auto-submitted form to the **ACS** URL) with `RelayState`; IdP-initiated SSO and its risks.
- **Metadata** ⚙️: entity IDs, ACS URLs, signing certificates, NameID formats; **certificate rollover**.
- **SAML vs OIDC**: XML vs JSON, browser POST vs redirects + back channel, enterprise legacy vs modern apps/APIs; **brokering** one into the other.

🔵 **Important**
- NameID formats and account linking. Single Logout and why it's fragile. `InResponseTo` and replay protection. **XML Signature Wrapping** — always use a maintained library. `SameSite=None; Secure` for the cross-site POST back.

### ⚖️ 💣 ⬛
- ⚖️ SAML (enterprise IdPs everywhere) vs OIDC (simpler, API-friendly). ⚖️ IdP-initiated (convenient) vs SP-initiated (protected by `InResponseTo`).
- 💣 Signature invalid after IdP certificate rotation (stale metadata). 💣 `NotBefore`/`NotOnOrAfter` clock skew. 💣 Audience/entity ID mismatch (trailing slash!). 💣 ACS URL mismatch behind a proxy (http vs https). 💣 Unsigned assertions accepted → impersonation. 💣 `SameSite=Lax` cookie → POST binding loses state.
- ⬛ Require signed assertions · validate audience, recipient, time window, `InResponseTo` · automate metadata refresh, plan rollover · prefer SP-initiated · prefer OIDC for new apps and bridge SAML at the IdP · never parse SAML by hand.

### 🎬 Visuals
- 🎬 SP-initiated SSO: Redirect binding → login → POST binding → ACS → cookie. 🎬 Keycloak brokering a SAML IdP into OIDC. 🎬 Certificate rollover with two certificates in metadata.

### 🛠️ Build
1. Keycloak: SAML client `campusgate-tuition` (entity ID, ACS URL, signatures on, NameID persistent, role mapper); download IdP metadata.
2. `CampusGate.Tuition` + Sustainsys: SP entity ID, IdP from metadata URL, SP signing certificate (from L07), cookie `SameSite=None; Secure`; `/invoices` page requiring role `registrar`.
3. SAML-tracer: capture one login; decode with TokenInspector; annotate every element.
4. Single Logout configured and tested.
5. `docs/saml/saml-cheatsheet.md`; optional: a second realm acting as SAML IdP brokered into `campusgate`.

### 🧪 Tests (Part 3)
Integration (Keycloak fixture + Tuition via `WebApplicationFactory`): `/invoices` unauthenticated → 302 to Keycloak with an `AuthnRequest` containing the right `Issuer` and ACS URL (decode and assert); SP metadata publishes the expected entity ID and certificate; **tampered** SAML response → rejected; expired assertion (signed with a test key trusted only in tests) → rejected; non-registrar user → 403. 💥 Stop requiring signed assertions → tampered-response test red → restore.

### 🏁 PR `feat(saml): SAML 2.0 SP for Tuition with Keycloak IdP`

### 🎤 Explain-it & interview
1. IdP, SP, assertion, ACS, entity ID? 2. Redirect vs POST binding? 3. What's in metadata and why it matters for rollover? 4. SAML vs OIDC — when each? 5. 🎤 *"A partner's SAML login started failing this morning with 'signature invalid'. What do you check first?"* 6. 🎤 *"How would you let a SAML-only enterprise IdP log in to an OIDC application?"*

---

# DAY 5 — CUSTOMISATION, PLATFORMS & INTERVIEW

## L17 — Customising Keycloak I: Java for C# developers, themes, Admin REST API & config as code
**Day 5 · Slot 1**
**📍 Start:** Keycloak configured by hand. **🏁 End:** enough **Java & Maven** (IntelliJ) to write SPIs; a CampusGate **login theme**; a typed **Keycloak Admin REST API** client in C# (`Infrastructure/Identity/KeycloakAdminClient`) behind an `IIdentityProvisioning` port, provisioning teachers idempotently with a least-privilege service account; a CI job proving `realm-campusgate.json` imports cleanly.

### Concepts
🟢 **Core**
- **Java & Maven for C# devs** (💡 a sister language): JDK vs JRE (≈ SDK vs runtime), packages vs namespaces, interfaces/annotations, `pom.xml` (≈ `.csproj` + NuGet), lifecycle (`compile`, `test`, `package`, `verify`), JUnit 5 (≈ xUnit), Mockito (≈ NSubstitute).
- **Keycloak extension options**: configuration → theme → Admin REST API → SPI. Rule: **configure first, theme second, SPI last**.
- **Themes** ⚙️: types (login, account, email, admin), FreeMarker templates, `theme.properties`, messages/i18n, parent theme, cache off in dev.
- **Admin REST API** ⚙️: service-account client with **least-privilege realm-management roles** (`manage-users`, not `realm-admin`); token via client credentials; users, groups, role mappings; idempotent provisioning.

🔵 **Important**
- **Config as code**: realm export/import vs `keycloak-config-cli` vs Terraform provider (verify) — pros/cons; no secrets in Git; per-environment values. Template upgrade risk — keep overrides minimal.

### ⚖️ 💣 ⬛
- ⚖️ Custom theme (branding) vs upgrade cost. ⚖️ Provisioning via Admin API vs user federation.
- 💣 Overriding whole templates → broken login after upgrade. 💣 Admin client with `realm-admin` → compromised app takes over the realm. 💣 Non-idempotent provisioning → duplicate users. 💣 Theme cache on in dev → "my change doesn't show".
- ⬛ Minimal theme overrides, diffed on every upgrade · least-privilege admin service account · idempotent provisioning with retries · config as code reviewed in PRs · admin events audited.

### 🎬 Visuals
- Diagram: extension pyramid. 🎬 Teacher added → command handler → `KeycloakAdminClient` → user + group + attributes.

### 🛠️ Build (☕ IntelliJ + 🔑 console + Visual Studio)
1. IntelliJ: Maven project `java-primer` — a class, an interface, a JUnit test; side-by-side with C#.
2. `identity/keycloak/themes/campusgate/login`: `theme.properties` (parent — verify for the pinned version), CSS, logo, messages (EN/NL/FR), one small template override; mounted into the container.
3. Client `campusgate-provisioner` (service account; `manage-users`, `view-users` only).
4. `KeycloakAdminClient` (typed `HttpClient` + `ClientCredentialsTokenHandler` from L14); `RegisterTeacher` use case searches by attribute first, then creates.
5. CI job: start Keycloak container, import realm, smoke-check discovery.

### 🧪 Tests (Part 3)
JUnit test in the primer (IntelliJ green). C# against the Keycloak fixture: creates a user with attributes and group; second call doesn't duplicate; provisioner **cannot** create a client or change realm settings (403); login page HTML contains the CampusGate message. 💥 Give the provisioner `realm-admin` → "cannot change realm" test red → restore.

### 🏁 PR `feat(identity): login theme, admin provisioning, realm import CI`

### 🎤 Explain-it & interview
1. Keycloak's extension options and when to use each? 2. Theme inheritance? 3. Roles for an Admin API client? 4. Config-as-code options? 5. 🎤 *"How do you keep Keycloak configuration consistent across environments?"* 6. 🎤 *"Maven vs NuGet/MSBuild — explain to a .NET colleague."*

---

## L18 — Customising Keycloak II: SPI plugins
**Day 5 · Slot 2**
**📍 Start:** Java basics, a theme, the Admin API. **🏁 End:** `identity/keycloak/spi/campusgate-keycloak-extensions` (Maven) with a **protocol mapper** `campusgate-claims` (replaces the built-in attribute mappers) and an **authenticator** `account-status-check` (blocks login when the CampusGate API says the student account is suspended, e.g. unpaid tuition; 1 s timeout, short cache, **fail closed**), plus an optional **event listener**; JUnit + Testcontainers tests; a **multi-stage Dockerfile** building a custom Keycloak image in CI; CampusGate's .NET tests now run against that image.

### Concepts
🟢 **Core**
- **The SPI model** ⚙️: Service Provider Interface = an interface Keycloak calls; a **`ProviderFactory`** (created once: `init`/`postInit`/`create`/`close`, `getId()`) creates **`Provider`** instances per session; registration in `META-INF/services/<factory-interface>`; `KeycloakSession` ≈ a per-request scoped container.
- **Protocol mapper SPI**: `AbstractOIDCProtocolMapper` + `OIDCAccessTokenMapper`/`OIDCIDTokenMapper`/`UserInfoTokenMapper`; configurable properties in the console.
- **Authenticator SPI**: `AuthenticatorFactory`/`Authenticator` — `authenticate`, `action`, `requiresUser`, `configuredFor`; added to a **copied browser flow**; `context.success()` / `failure()` / `challenge()` with a themed error page.

🔵 **Important**
- Packaging ⚙️: JAR in `/opt/keycloak/providers`, **`kc.sh build`**, multi-stage Dockerfile (Maven → Keycloak builder → runtime). Dependencies `provided`, version = pinned Keycloak. **Public vs internal/private SPIs** (can change between versions — verify). Event listener SPI (send *after* commit, never block login). User Storage SPI & Required Action SPI — when to choose them. External calls from SPIs: timeouts, caching, fail-open vs fail-closed.

### ⚖️ 💣 ⬛
- ⚖️ SPI (powerful, in-process) vs out-of-process (Admin API, events, claims added by the API) — upgrade cost. ⚖️ Fail-closed (secure) vs fail-open (available).
- 💣 Slow HTTP call in an authenticator → every login hangs, worker threads exhausted. 💣 Provider built against another Keycloak version → `NoSuchMethodError` at startup. 💣 Event listener throwing → login errors. 💣 Mapper puts personal data in tokens. 💣 Forgot `kc.sh build` → provider not loaded in optimised mode.
- ⬛ Customise only what configuration can't · pin Keycloak version in `pom.xml` = image version · timeouts + caching on every external call · decide fail-open/closed explicitly and document it · no personal data in tokens · integration-test providers in a real Keycloak · rebuild and re-test on every upgrade.

### 🎬 Visuals
- 🎬 Keycloak startup: scan providers → factories `init` → per-request providers via `KeycloakSession`. 🎬 Login with `account-status-check`: password → authenticator → API call → success or "account suspended" page.

### 🛠️ Build (☕ IntelliJ + 🐳 Docker Desktop)
1. Maven project with `keycloak-server-spi`, `keycloak-server-spi-private`, `keycloak-services` as `provided` (verify artifacts for the pinned version), JUnit 5, Mockito.
2. `CampusGateClaimsProtocolMapper` (`student_id`, `faculty` with configurable claim names).
3. `AccountStatusAuthenticator` + factory; API endpoint `api/v1/accounts/{studentId}/status` protected by a client-credentials token; copied browser flow using it.
4. Optional `CampusGateAuditEventListener` (minimal event, async, bounded).
5. `identity/keycloak/Dockerfile` (multi-stage) → `campusgate/keycloak:<version>`; compose uses it.
6. CI job: `mvn verify` + image build.

### 🧪 Tests (Part 3)
JUnit: mapper logic; authenticator decision function with Mockito. Integration (Java Testcontainers Keycloak — verify artifact — loading the JAR): token contains `student_id`/`faculty`; suspended account login fails, active succeeds. .NET: the full suite still passes against the custom image. 💥 Remove the authenticator timeout and stub a 10-second response → login test times out → restore.

### 🏁 PR `feat(keycloak): claims mapper and account-status authenticator SPIs`

### 🎤 Explain-it & interview
1. SPI, ProviderFactory, Provider? 2. How is a provider deployed? 3. Mapper vs authenticator vs event listener? 4. Fail-open vs fail-closed? 5. 🎤 *"Tell me about a Keycloak extension you built and the decisions behind it."* 6. 🎤 *"After upgrading Keycloak, it won't start: `NoSuchMethodError` in your provider. What happened and how do you prevent it?"*

---

## L19 — Duende IdentityServer, federation & brokering; Ping Identity & Entra ID compared
**Day 5 · Slot 3**
**📍 Start:** Keycloak is the only identity provider. **🏁 End:** `identity/Meridian.IdentityServer` (Duende IdentityServer + ASP.NET Core Identity, seeded fictional Meridian teachers) issues OIDC tokens; Keycloak **brokers** it (`meridian-medical`) so Meridian teachers log in to the portal with their own credentials; the API also accepts Meridian **client tokens directly** on one endpoint via the policy scheme from L15; `docs/identity/platform-comparison.md` maps Keycloak ↔ IdentityServer ↔ PingFederate/PingOne ↔ Entra ID; `docs/identity/migration-playbook.md`.

### Concepts
🟢 **Core**
- **IdentityServer as a framework** (💡 Keycloak is a ready-built office; IdentityServer is a kit to build your own): an ASP.NET Core app you host and code — clients, API scopes, API resources, identity resources, signing keys, persisted grants; extension points `IProfileService`, `IExtensionGrantValidator`, `ICustomTokenRequestValidator`, events, your own login UI.
- **Identity brokering & federation** ⚙️: Keycloak as broker — first broker login flow, account linking, allow-listed claim mappers, `kc_idp_hint`; trust boundaries (what you accept from a partner and what you re-check).
- **Ping Identity & Entra ID** (concepts — verify product names): PingFederate (SP/IdP connections, OAuth clients, access token managers, adapters, policies), PingOne (environments, applications, populations, sign-on policies), DaVinci; Entra ID (tenants, app registrations, enterprise apps, `scp` vs `roles`, managed identities). Concept map to Keycloak.
- **Choosing & migrating identity platforms** ⚙️: build vs self-host vs commercial vs SaaS; zero-downtime migration (dual issuers, user migration incl. password hashes or lazy migration, stable subject IDs, client-by-client cut-over, rollback).

🔵 **Important**
- Licensing: Duende free for development/testing, production licence required (verify Community Edition terms). OpenIddict as open-source alternative. SCIM (RFC 7643/7644) for provisioning. Claims differences between platforms → isolate in claims transformation.

### ⚖️ 💣 ⬛
- ⚖️ IdentityServer (full .NET control, you own patches and UI) vs Keycloak (product, admin UI, Java SPIs) vs Ping (commercial support) vs Entra (cloud, Microsoft ecosystem). ⚖️ Brokering (one issuer) vs direct trust (fewer hops, more issuers).
- 💣 Partner sends `role=admin` → blindly mapped → privilege escalation. 💣 Account linking by unverified email → takeover. 💣 Signing key not persisted → tokens invalid after restart. 💣 `sub` changes after migration → users lose their data. 💣 App hard-coded to one IdP's claim names.
- ⬛ Map only allow-listed partner claims · link by stable verified identifiers · per-partner audience/scopes · monitor partner metadata/keys · code to standards and isolate product specifics · staged, reversible migrations.

### 🎬 Visuals
- 🎬 Meridian teacher → portal → Keycloak (`kc_idp_hint=meridian-medical`) → IdentityServer login → back → first-broker-login → CampusGate token. 🎬 Staged migration with dual issuers. Table: one concept across four platforms.

### 🛠️ Build
1. `Meridian.IdentityServer` from the Duende templates (verify template name), ASP.NET Core Identity (SQLite or PostgreSQL container — verify store options), seeded users.
2. Clients `keycloak-broker` (code + PKCE) and `meridian-integration` (client credentials); `IProfileService` adding `meridian_staff_id`, `faculty`.
3. Keycloak: Identity Provider **OpenID Connect v1.0** `meridian-medical`, allow-listed mappers (hard-coded role `teacher` only), first-broker-login reviewed.
4. API: `Meridian` JWT scheme in the policy scheme; `api/v1/partner/meridian/grades` accepting Meridian client tokens with scope `meridian.grades`.
5. Portal: "Sign in with Meridian Medical School" (`kc_idp_hint`).
6. `platform-comparison.md`, `migration-playbook.md`, `docs/adr/ADR-00x-federation.md`.

### 🧪 Tests (Part 3)
IdentityServer: token for `meridian-integration`; `IProfileService` adds expected claims only. API: Meridian token accepted on its endpoint, rejected elsewhere; Keycloak token rejected on the Meridian-only endpoint; parameterised portability tests (Keycloak, IdentityServer) give the same 401/403/200 matrix; startup fails if two schemes share an issuer. Broker: Meridian user gets only `teacher`; an injected `role=admin` is **not** mapped. 💥 Map all partner claims to roles → "admin ignored" test red → restore.

### 🏁 PR `feat(identity): Meridian IdentityServer, brokering, platform comparison`

### 🎤 Explain-it & interview
1. IdentityServer vs Keycloak — how would you choose? 2. Three IdentityServer extension points? 3. Map Keycloak concepts to PingFederate/PingOne. 4. Account linking risks? 5. 🎤 *"An affiliated school wants its teachers to use our apps with their own logins. Design it."* 6. 🎤 *"We use PingFederate. You know Keycloak. How quickly can you be productive and what's different?"* 7. 🎤 *"Plan a zero-downtime IdP migration."*

---

## L20 — Troubleshooting auth flows, production checklist & full mock interview
**Day 5 · Slot 4 (allow 2.5–3 hours)**
**📍 Start:** everything works. **🏁 End:** `docs/runbooks/auth-troubleshooting.md` (symptom → evidence → cause → fix for ~20 failures); 8 drills diagnosed in your own CampusGate; startup guards against unsafe production settings; workflow actions pinned by SHA + CodeQL added; the production identity checklist applied; a scored mock interview and your 2-minute honest project pitch.

| Part | Time | What Claude delivers |
|---|---|---|
| A. Method & toolbelt | 20 min | 🟢 The method (reproduce → locate the failing hop → collect evidence → compare with the 📜 expected trace → one change → verify → write it down). Toolbelt: dev tools (Preserve log), **HAR files (contain tokens — sanitise, never share raw)**, SAML-tracer, TokenInspector, Keycloak login/admin events, server logs, ASP.NET Core auth logs at Debug, `ShowPII` (⚠️ local only), Seq by correlation ID, `.http` to discovery/JWKS, clock checks. 🎬 Decision tree "where does the flow stop?". ⚖️/💣/⬛ as usual. |
| B. Drills | 45 min | 8 toggles that break CampusGate on purpose: wrong redirect URI · wrong client secret · issuer hostname mismatch · audience mapper removed · clock skew · `SameSite=Strict` correlation cookie · proxy without forwarded headers · Data Protection not shared between two portal instances. For each: symptom → evidence → fix → runbook row. Regression tests for the automatable ones; startup validation rejecting `ShowPII`, `RequireHttpsMetadata=false`, `ValidateIssuer=false`, `ValidateAudience=false` outside Development. 💥 Enable `ShowPII` in Production → startup fails → restore. |
| C. Production & DevSecOps | 15 min | Apply Appendix I to CampusGate; record gaps as GitHub issues labelled `security`. Pin actions by SHA, add CodeQL (C# + Java) and dependency review to `security.yml`. PR `chore: troubleshooting runbook, startup guards, CodeQL`. |
| D. Mock interview | 50 min | Four rounds in interviewer style: (1) protocols — OAuth/OIDC/SAML deep dive; (2) Keycloak — config, SPIs, production; (3) coding — write an `AuthenticationHandler` / authorisation handler on paper; (4) system design — "SSO and secure partner integrations for a large organisation with Keycloak, a legacy SAML app, an affiliated IdP and many internal APIs". Plus 4 troubleshooting stories (`invalid_grant` after an IdP upgrade, SAML cert rollover, 401s after key rotation, 403 despite the role). ⛔ Model answers, scoring rubric, red flags. |
| E. Your story & cheat sheet | 20 min | README for CampusGate (architecture + identity diagrams, how to run, security model, **what's lab-only**); 2-minute honest pitch; 5 STAR stories from your Story Bank; one printable cheat sheet; `MOCK INTERVIEW` suggestions for the evening before the interview. |

---

# APPENDICES

## Appendix A — Tool cheat sheets

**Visual Studio:** F5 / Ctrl+F5 / Shift+F5 · F9 breakpoint, F10 step over, F11 step into · Ctrl+Shift+B build · Test Explorer (Ctrl+R, A) · right-click project → **Manage NuGet Packages** (pick version) · **Manage User Secrets** · **View > Git Changes**, **Git > Manage Branches**, merge editor · `.http` files, **View > Other Windows > Endpoints Explorer** (verify labels).

**Browser dev tools (F12):** Network (Preserve log, Disable cache), Application > Cookies, certificate viewer, Console. **Never share raw HAR files.**

**Keycloak admin console:** Realm settings (General, Login, Keys, Tokens, Sessions, Events, Themes, Security defenses) · Clients · Client scopes · Realm roles · Groups · Users · Identity providers · Authentication (Flows, Required actions, Policies) · Events — verify labels for the pinned version.

**IntelliJ IDEA:** open `pom.xml` as project · Maven tool window (Lifecycle → `verify`, `package`) · gutter ▶ to run tests · Project SDK = JDK 21.

**GitHub:** Pull requests · Actions · Settings > Branches/Rulesets · Settings > Code security (Dependabot, secret scanning, push protection, code scanning).

## Appendix B — Docker recipes

| Service | Image : tag | Ports | Notes |
|---|---|---|---|
| Keycloak | `quay.io/keycloak/keycloak` : exact version (verify) → later `campusgate/keycloak` | 8080, management 9000 (verify) | with PostgreSQL; realm import |
| PostgreSQL | `postgres` : pinned major supported by Keycloak (verify) | internal only | volume |
| Seq | `datalust/seq` (verify licence env var) | 5341 / 80 | structured logs & security events |

## Appendix C — Packages

| When | Package | Version / note |
|---|---|---|
| L04 | `MediatR` | **12.5.0 pinned** (Apache 2.0) |
| L04 | `FluentValidation.DependencyInjectionExtensions` | latest |
| L04 | `FluentAssertions` | **7.x pinned** (v8+ commercial) |
| L04 | `NSubstitute`, `Bogus`, `NetArchTest.Rules` / `TngTech.ArchUnitNET.xUnit`, `Microsoft.Extensions.TimeProvider.Testing` | latest (verify) |
| L02/L12 | `Serilog.AspNetCore`, `Serilog.Sinks.Seq` | latest (verify) |
| L05 | `Microsoft.AspNetCore.Mvc.Testing` | 10.0.x |
| L08 | `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.x |
| L09 | `Testcontainers.Keycloak` (verify) | latest |
| L10/L13 | `Microsoft.AspNetCore.Authentication.OpenIdConnect`, `Yarp.ReverseProxy` | 10.0.x / latest |
| L16 | `Sustainsys.Saml2.AspNetCore2` (alt. `ITfoxtec.Identity.Saml2`) | verify .NET 10 support |
| L17–L18 | Maven: `keycloak-server-spi`, `keycloak-server-spi-private`, `keycloak-services` (provided), JUnit 5, Mockito, `testcontainers-keycloak` (dasniko — verify) | = pinned Keycloak |
| L19 | `Duende.IdentityServer`, `.AspNetIdentity` | dev/test free; production licence |

## Appendix D — Clean Architecture quick reference

| Layer | Contains | Never contains |
|---|---|---|
| Domain | Entities, value objects, business permission rules | Tokens, `HttpContext`, frameworks |
| Application | Use cases, validators, behaviors, ports (`ICurrentUser`, `IIdentityProvisioning`, `IAuditLog`) | Repositories' implementations, IdP clients, controllers |
| Infrastructure | In-memory repositories, `HttpCurrentUser`, `KeycloakAdminClient` | Business rules |
| CampusGate.Auth | Schemes, handlers, policies, claims transformations, token handlers | Business rules |
| Hosts (Api, Portal, Tuition) | Controllers/pages, middleware order, composition root | Business rules |

## Appendix E — Standards & RFC map (verify before quoting)

| Topic | Spec |
|---|---|
| OAuth 2.0 · bearer tokens | RFC 6749 · RFC 6750 |
| PKCE | RFC 7636 |
| JWT · JWS · JWK | RFC 7519 · RFC 7515 · RFC 7517 |
| JWT client assertions (`private_key_jwt`) | RFC 7523 |
| AS metadata | RFC 8414 |
| Revocation · introspection | RFC 7009 · RFC 7662 |
| Device grant | RFC 8628 |
| Token exchange | RFC 8693 |
| mTLS · DPoP | RFC 8705 · RFC 9449 |
| JWT access token profile | RFC 9068 |
| PAR | RFC 9126 |
| OAuth 2.0 Security BCP | RFC 9700 |
| OAuth 2.1 · browser-based apps | IETF drafts (verify status) |
| OIDC Core, Discovery, RP-Initiated / Back-Channel Logout | OpenID Foundation |
| SAML 2.0 | OASIS |
| SCIM | RFC 7643 · RFC 7644 |
| Problem Details | RFC 9457 |
| OWASP Top 10 · API Security Top 10 · ASVS | OWASP (verify editions) |

## Appendix F — Interview map (job line → lessons)

| Job requirement | Lessons |
|---|---|
| Authentication & authorisation middleware | L02, L08, L12, L15 |
| Secure C#/.NET applications & services | L02, L04, L05–L08, L13 |
| Authentication servers & identity platforms | L09–L11, L19 |
| Understanding & troubleshooting auth flows | L09, L10, L13, L16, L20 |
| OAuth2, OIDC, SAML2 solutions | L09, L10, L13, L14, L16 |
| Customising Keycloak incl. plugins | L11, L17, L18 |
| IdentityServer / Ping Identity | L19 |
| Secure integrations across the organisation | L14, L15, L19 |
| REST APIs | L01, L02 |
| Web security | L05, L06, L07 |
| Git & DevOps | L03 + every PR, L17–L18 CI, L20 |

## Appendix G — Production identity checklist (L11, L18, L20)

**Identity provider:** exact pinned version, patched promptly · PostgreSQL with tested backups · fixed public hostname, TLS, proxy headers · admin console isolated, admins with MFA · brute-force protection · password policy · events shipped to logging · signing keys rotated with overlap · HA plan · realm config as code without secrets · SPIs rebuilt and tested per upgrade.
**Clients:** code + PKCE only · exact redirect URIs · unused grants off · one client per app/integration · asymmetric client auth where possible · least-privilege scopes · secrets in a vault, rotated.
**Tokens:** short access-token lifetime · refresh rotation · audience per API · minimal claims, no personal data · issuer/audience/signature/lifetime validated with pinned algorithms.
**Applications:** BFF for browser apps · secure cookies · antiforgery · shared Data Protection keys · back-channel logout · fallback policy · resource-based checks · consistent 401/403 · structured security events without tokens.
**Operations:** alerts on login error spikes and 401/403 anomalies · runbooks: leaked client secret, compromised account, signing-key compromise, IdP outage, certificate rollover · troubleshooting guide maintained.

## Appendix H — Interview question bank (seed list — grows with `INTERVIEW`)
1. Walk me through authorization code + PKCE. 2. OAuth vs OIDC? 3. ID token vs access token? 4. How do you validate a JWT and what does each check stop? 5. Where should a browser app keep tokens? 6. Refresh rotation and reuse detection? 7. Client credentials vs token exchange? 8. SAML vs OIDC; how does the POST binding work? 9. What's in SAML metadata and why does it matter? 10. Troubleshoot "Correlation failed" / `invalid_grant` / `IDX10205`. 11. Keycloak: realm vs client vs client scope vs mapper. 12. Keycloak SPIs you built; deploying and upgrading them. 13. Keycloak in production — what must be configured? 14. IdentityServer vs Keycloak vs Ping. 15. Two IdPs in one API? 16. Write an `AuthenticationHandler`. 17. CSRF vs XSS in a cookie-based app. 18. What does CORS protect? 19. Securing integrations across a large organisation. 20. Design SSO for a group with legacy SAML apps and modern APIs. 21. A client secret leaked — now what? 22. Merge vs rebase; protecting main; a secret in history. 23. What's in your CI pipeline for security? 24. Keeping personal data out of logs. 25. What would you monitor on an identity platform?
