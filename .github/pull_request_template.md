## What
<!-- One or two sentences: what changes. -->

## Why
<!-- The problem, ticket or lesson (e.g. L03). -->

## How tested
- [ ] Builds locally with `-warnaserror`
- [ ] Tests added/updated and green (from L04)
- [ ] Manual check (`.http` file / browser / Keycloak console) described below

## Security impact
<!-- Answer every line. "None" is a valid answer, but say why. -->
- Authentication / authorisation changed?
- New input accepted from users or other systems? How is it validated?
- Secrets, tokens or personal data touched, logged or returned?
- New or updated dependencies?

## Checklist
- [ ] Small, focused PR (one concern)
- [ ] Conventional Commit title (`feat:`, `fix:`, `ci:`, `chore:`, `docs:`, `test:`, `refactor:`)
- [ ] No secrets, no `bin/`/`obj/`, no personal data in the diff