# API checklist — manual regression plan (L02)

Run with the https profile; requests are in `src/CampusGate.Api/CampusGate.Api.http`.
Becomes automated integration tests in L04/L05.

| # | Rule | Request | Expected |
|---|---|---|---|
| 1 | Paging defaults | #1 | 200, page 1, pageSize 20, totalCount 3 |
| 2 | Max page size enforced server-side | #2 | 200, pageSize 100 |
| 3 | Invalid paging rejected | #3 | 400 ValidationProblemDetails |
| 4 | Course lookup is case-insensitive | #4 | 200, code CS101 |
| 5 | Unknown course → ProblemDetails | #5 | 404 with traceId |
| 6 | Malformed course code rejected | #6 | 400 |
| 7 | PUT creates | #7 | 201 + Location |
| 8 | PUT idempotent | #8 | 200, same body |
| 9 | Missing title rejected | #9 | 400 |
| 10 | Non-JSON body rejected | #10 | 415 |
| 11 | Extra `id` ignored; correlation id echoed | #11 | 201, server-generated id, X-Correlation-Id: demo-123 |
| 12 | No double enrolment | #12 | 409 already_enrolled |
| 13 | Enrol until capacity | #13 | 201 |
| 14 | Capacity enforced | #14 | 409 course_full |
| 15 | Student number format | #15 | 400 |
| 16 | Delete | #16 | 204 |
| 17 | Delete idempotent | #17 | 404 (state unchanged) |
| 18 | Options bound | #18 | maxGrade 20, maxPastDays 30 |
| 19 | No internals in 500 | #19 | 500 generic ProblemDetails with traceId; no stack trace or server names |
| 20 | Invalid correlation id replaced | #20 | generated 32-hex id |
| 21 | Correlation id on every response | #1 #3 #5 #19 | header present |
| 22 | Health | #21 | 200 Healthy |
| 23 | Bad config fails at startup | MaxGrade = 0 | OptionsValidationException at start |
| 24 | Concurrent enrolments never exceed capacity | parallel script | exactly `capacity` × 201, rest 409 |