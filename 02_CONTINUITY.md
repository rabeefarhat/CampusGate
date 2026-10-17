# 02 — CONTINUITY: progress & project state

> Claude reads this, and the synced GitHub repository, at the start of every chat. At the end of every lesson Claude outputs this **whole file**, updated — replace it in Project knowledge after filling in the `__` fields.

## CURRENT STATE

**Last completed lesson:** none — fresh start
**Next lesson:** L01 — Start the project: Visual Studio 2026, .NET 10 & your first API
**Pace:** 4 lessons a day (~2 hours each), 20 lessons over 5 days
**Today:** Day 1 — L01, L02, L03, L04
**Behind schedule?** no
**Interview:** right after Day 5 — _fill in the date_
**Target role:** Senior .NET Developer, authentication middleware team, European energy group (OAuth2/OIDC/SAML2, Keycloak incl. plugins, IdentityServer/Ping, web security, REST, Git & DevOps)

### Project state (CampusGate — university identity & access platform)
- **Last project step completed:** none
- **Physical location:** `D:\CampusGate\` (solution file `D:\CampusGate\CampusGate.slnx`)
- **Solution tree:** _(created in L01 — written as an indented list, never inside a ``` code block)_
- **What works:** nothing yet
- **Differences from the lessons / known issues:** none
- **Ports:** API HTTPS _fill in_ · Portal _fill in (L13)_ · Tuition _fill in (L16)_ · Keycloak _fill in (L09/L11)_ · Seq _fill in (L11)_ · Meridian IdentityServer _fill in (L19)_
- **GitHub repository URL:** _fill in after L01_
- **Repository in Project knowledge:** _not yet connected — add after L01, sync after every merged PR_
- **Last PR merged and synced:** _none_

### Code facts (only what the repo can't show)
- _(empty — names chosen differently from the curriculum, skipped or extra steps, and why)_

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
| User Secrets keys in use (names only) | _(L08 →)_ |

### Working agreements
1. One chat = one lesson: `START Lxx` or `START Lxx PARTS`; five parts (Understand · Build · Test · Practice · Review).
2. **No TDD.** Build and run first; all tests in Part 3; one test per business and security rule; one 💥 break-it per lesson.
3. **Zero prior knowledge of security, identity and DevOps** — every term defined, every protocol traced in raw HTTP, exact error messages shown.
4. Every 🟢 concept ends with ⬛ production best practices and an 🎤 interview angle.
5. Standards before products; current best practice only (code + PKCE, no implicit/password grants, BFF, RFC 9700); dev shortcuts labelled ⚠️ DEV ONLY.
6. Clean Architecture (Domain ← Application ← Infrastructure ← hosts); `CampusGate.Auth` is the shared auth library.
7. Controllers only; REST conventions from L02; DTOs only.
8. From L03: feature branch → small commits → PR → green CI → squash-merge → Sync.
9. Testing toolkit: xUnit · FluentAssertions **7.x** · NSubstitute (ports only) · Bogus (fixed seed) · Testcontainers (Keycloak) · JUnit for SPIs.
10. Never log or share tokens, cookies, secrets or personal data; decode tokens only with TokenInspector.

### Project steps
- Day 1: [ ] L01 First API · [ ] L02 Fundamentals + REST · [ ] L03 Git & DevOps · [ ] L04 Clean Architecture + MediatR
- Day 2: [ ] L05 Web security I · [ ] L06 Web security II · [ ] L07 Cryptography · [ ] L08 ASP.NET Core auth
- Day 3: [ ] L09 OAuth 2.0 · [ ] L10 OIDC · [ ] L11 Keycloak · [ ] L12 Securing the API
- Day 4: [ ] L13 Portal BFF · [ ] L14 Service-to-service · [ ] L15 Auth middleware · [ ] L16 SAML 2.0
- Day 5: [ ] L17 Keycloak customisation · [ ] L18 Keycloak SPIs · [ ] L19 IdentityServer & platforms · [ ] L20 Troubleshooting + mock interview

### Ubiquitous language (source of truth: docs/ubiquitous-language.md)
- _(starts in L04: student number, course, course code, enrolment, capacity, grade, published grade, correction, faculty…)_

### Environment
- Visual Studio 2026 version: _fill in_ · workload: ASP.NET and web development ✓
- .NET SDK: _fill in (10.0.x)_
- GitHub account: ✓
- Docker Desktop: ✓ (Engine running, ≥ 4 GB RAM — confirm before L09)
- Second browser profile: _yes/no_ (L13) · SAML-tracer extension: _yes/no_ (L16)
- JDK 21: _not yet — install before Day 5_ · IntelliJ IDEA: _not yet — install before Day 5_

### Confidence & mock scores
- Day 2 end self-rating (security basics /5): _?_ · Day 4 end (protocols /5): _?_ · L20 mock interview: _?_

### Weak spots (max 15)
- _(none yet)_

### Things that confused me
- _(your own words)_

### Skipped / deferred items
- _(none)_

### Decisions (ADRs, one line each)
- _(none yet — ADR-001 Clean Architecture comes in L04)_

### "Explain CampusGate in 2 minutes" versions
- _(starts after L08; identity version after L13; final version in L20)_

## LESSON LOG
| Lesson | Date | Time spent | Key takeaways (short) | Self-score (avg /5) | Open questions |
|---|---|---|---|---|---|

---

## INTERVIEW STORY BANK
_(1–2 STAR-style talking points per lesson: Situation · Task · Action · Result — honest, lab-scale, no inflated claims.)_

- _(starts in L01)_

---

## KEY IDEAS CHEAT SHEET
_(Grows by 3–6 lines per lesson, incl. at least one trade-off, one failure mode and one production rule. L20 condenses it to one printable page.)_

- .NET 10 is the current LTS (Nov 2025 → Nov 2028); C# 14 and ASP.NET Core 10 ship with it.
- Standards before products: if you understand OAuth 2.0, OIDC and SAML 2.0, then Keycloak, IdentityServer, Ping and Entra are configuration differences.
- Authentication proves who you are; authorisation decides what you may do — and every object access must be authorised (BOLA).
- Licences shape choices: MediatR 12.5.0 and FluentAssertions 7.x pinned; Duende IdentityServer free for dev/test, licensed in production; Keycloak is Apache 2.0 but only the latest release gets security fixes.
- For every technology, be ready to say what it costs, how it fails, and how you'd run it in production.
