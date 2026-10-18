# CampusGate Git workflow

- Model: GitHub Flow. `main` is always releasable and protected (ruleset `protect-main`).
- Branches: `feature/lxx-<topic>`, `fix/<topic>`; short-lived; deleted after merge.
- Commits: Conventional Commits — `type(scope): summary` (feat, fix, ci, chore, docs, test, refactor).
- PRs: use the template, answer "Security impact", keep them small, CI check `build` must be green.
- Merge: squash only. Never force-push `main`; rebase only branches nobody else uses.

## If a secret is committed (even for a second)
1. Revoke / rotate it at the source immediately — assume it is already copied.
2. Check the provider's access logs for misuse.
3. Remove it from history (`git filter-repo`), force-push the cleaned branch, ask collaborators to re-clone.
4. Contact GitHub Support to purge cached views; close the related PRs.
5. Add a scanner pattern or a prefix so this key format is detected next time.