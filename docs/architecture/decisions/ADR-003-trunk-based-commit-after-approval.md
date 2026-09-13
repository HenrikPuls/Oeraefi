# ADR-003: Trunk-Based Development, Commits Only After Explicit Approval

Status: Accepted (backfilled from arc42 decision table; decided pre-M0)

## Context

Solo developer with fully agentic (LLM-generated) implementation. Unreviewed autonomous commits would make it impossible to keep an overview of what entered the history.

## Decision

Single `main` branch, direct commits (no feature branches by default). The agent implements, verifies, and presents a summary; only an explicit user approval triggers `git add`/`commit`/`push`. Conventional Commits format.

## Consequences

- Every history entry is user-approved content.
- Work-in-progress lives in the working tree between sessions; `docs/status/CURRENT.md` carries the cross-session state.
- No PR/review ceremony for a solo project; approval happens in-session.
