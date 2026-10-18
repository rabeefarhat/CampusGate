# CampusGate API guidelines (v1)

## Resources and URLs
- Base path: `/api/v1`. Breaking changes require `/api/v2`; `v1` keeps working until its announced sunset date.
- Plural nouns, lowercase: `/courses`, `/courses/{code}/enrolments`. No verbs in URLs.
- Non-CRUD actions are sub-resources created with POST: `POST /grades/{id}/corrections`.
- No personal data or secrets in URLs or query strings (they are logged everywhere).

## Methods
| Method | Safe | Idempotent | Use |
|---|---|---|---|
| GET | yes | yes | Read. Never changes state. |
| PUT | no | yes | Create or replace a resource whose id the client knows (course code). |
| DELETE | no | yes | Remove. Second call returns 404, state is unchanged. |
| POST | no | no | Create when the server picks the id (enrolments); non-CRUD actions. |

## Status codes
- 200 OK · 201 Created (+ `Location`) · 204 No Content
- 400 invalid input (ValidationProblemDetails) · 415 wrong media type
- 401 not authenticated · 403 authenticated but role not allowed
- 404 not found **or** not yours (we never confirm other people's objects exist)
- 409 conflicts with current state (`already_enrolled`, `course_full`) · 429 rate limited
- 500 our bug: generic message only

## Errors
- Every error body is RFC 9457 ProblemDetails (`application/problem+json`) with `type`, `title`, `status`, `traceId`, `correlationId` and, for business errors, a stable `errorCode`.
- Never return stack traces, SQL, hostnames or exception messages to clients.

## Paging
- `?page=1&pageSize=20` (offset paging). Default page size 20, server maximum 100 (larger values are capped, not rejected).
- Responses: `{ items, page, pageSize, totalCount, totalPages }`.

## Requests
- Bind request DTOs only, never storage/domain types (prevents over-posting). Unknown JSON properties are ignored.
- Ids in the URL win; bodies never carry the resource id.
- All input validated server-side with allow-list formats (course code `^[A-Za-z]{2,4}[0-9]{3}$`, student number `^2[0-9]{7}$` + Luhn from L04).

## Tracing and logging
- Every response carries `X-Correlation-Id`. Clients may send one (max 64 chars, `[A-Za-z0-9-]`); otherwise the server generates it.
- Logs use message templates with ids only. Never log personal data, tokens, cookies or secrets.

## Documentation
- Every action declares `[ProducesResponseType]` for each status it can return. The OpenAPI document (`/openapi/v1.json`, Development) is reviewed in PRs.