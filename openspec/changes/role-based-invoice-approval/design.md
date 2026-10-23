# Design

## Context

The repo currently has a simple approval model with a single `Approver`-class gate at the controller and a single `DecidedBy` field on each invoice. The domain service accepts any valid pending invoice and records one approver without any business authorization or amount checks. This is the current baseline for the change described in the proposal.

## Goals / Non-Goals

**Goals:**
- Enforce `Manager` and `Director` approval limits in the domain service.
- Require a second, distinct `Director` approval for invoices above $500,000 before the invoice becomes fully approved.
- Ensure invalid approval attempts fail with a clear reason and leave invoice state and approval records unchanged.
- Keep the existing request/response and session-auth architecture while tightening server-side permissions.
- Preserve unrelated admin-only behavior such as invoice delete actions while restricting approval authority to the new roles.

**Non-Goals:**
- Replacing the in-memory repository model or introducing a new persistence store.
- Allowing approval logic to live in controllers or browser code.
- Broad renaming or refactoring of unrelated invoice or auth behavior.

## Decisions

- Domain-first rule enforcement: approval eligibility will be calculated inside `ApprovalService`, not in the controller or browser. This keeps the invariant authoritative and testable independent of UI state.
- Explicit role authorization: only `Manager` and `Director` receive approval permissions by default. The existing `Approver` and `Admin` roles are not implicitly treated as approval roles and remain separate unless a product decision reassigns them. This preserves unrelated admin functionality while avoiding conflicting access semantics.
- Narrow interpretation for invoices above $500,000: the assignment requires a second distinct approver, and the second approver must be a `Director`. It does not explicitly specify the first approver’s role for that case. To stay within the assignment, the implementation will not invent a new policy; it will treat the first approval as a valid, otherwise eligible approver while leaving the invoice pending until a second distinct `Director` records approval. This is a documented planning assumption, not a new business requirement.
- Two-stage delivery: Stage 1 covers single-approver threshold logic and unit tests; Stage 2 adds the over-$500,000 dual-approver rule, duplicate-user checks, and second-approver `Director` constraint. This keeps validation focused and allows clean regression checks.
- Explicit state preservation: rejected validation failures will raise a domain exception before any mutation and will return an API error message that the controller layer can pass through without changing `Status`, `DecidedBy`, `DecidedAt`, or any approval records.
- Role configuration compatibility: demo accounts and tests will need a deliberate role matrix. Existing demo users should remain compatible, but new approval-capable demo accounts must be added explicitly as `Manager` and `Director` without silently granting `Approver` or `Admin` approval rights.

## Risks / Trade-offs

- [Existing legacy roles may be misread as current approvals] → Mitigation: reserve the new `Manager` and `Director` roles as explicit approval roles and do not implicitly assign `Approver` or `Admin` the same permissions without an explicit decision.
- [High-value approval state may be misunderstood if the invoice is marked approved too early] → Mitigation: keep the invoice pending after the first valid approval until the second, distinct `Director` approval is recorded, so there is no partial approval state that falsely represents final approval.
- [Invoice approval state can become inconsistent if validation happens after mutation] → Mitigation: validate eligibility before changing state and guard all mutation behind a single approval rule path.
- [UI/UX may give false confidence if browser logic is the only check] → Mitigation: keep UI feedback as convenience only and enforce the same checks in the server response path.

## Migration Plan

1. Add the new role model and demo-account configuration deliberately, without breaking existing non-approval flows.
2. Implement Stage 1: `Manager` and `Director` single-approver boundaries plus the unit test suite.
3. Implement Stage 2: over-$500,000 dual approval logic, duplicate-user checks, and second-approver `Director` requirement.
4. Add Playwright coverage for a rejected high-value approval through the UI and validate that the reason is visible and the invoice remains unchanged.
5. Run the project’s .NET test suite and Playwright tests before review, and record the results in the PR description without claiming success without evidence.

## Open Questions

- The requirement does not specify which role may provide the first approval on an invoice above $500,000. The implementation will not invent an additional rule; instead, the current planning assumption is that the first approval must come from an otherwise eligible, distinct approver and that the invoice remains pending until a second distinct `Director` approves it. This must be confirmed by product review before implementation.
- Should the legacy `Approver` and `Admin` roles continue to exist without approval privileges, or should they be intentionally mapped to `Manager`/`Director` during a migration window?
- For demo accounts and existing end-to-end tests, which usernames should be seeded as `Manager` and `Director` so the authorization model remains testable without changing unrelated functionality?
- Should an over-limit approval attempt return a single generic error or a specific reason such as “invoice exceeds Director limit; a second distinct Director approval is required”? The implementation should keep the message actionable but the exact wording is a product decision.

## Verification / Acceptance Criteria

- `dotnet test backend/InvoiceApp.sln` passes for the approval logic and regression tests.
- `cd e2e && npm install && npx playwright install chromium && npm test` passes for the rejected over-limit flow.
- `Manager` approval succeeds at $50,000 and fails just over the limit.
- `Director` approval succeeds at $500,000 and fails just over the limit.
- Invoice amounts above $500,000 require two distinct approvers, with the second approver being a `Director`.
- The first valid approval on a high-value invoice does not mark the invoice as fully approved; it remains pending until the second valid Director approval is recorded.
- Duplicate approvers, unauthorized roles, invalid approval sequences, and rejected approval attempts leave all invoice fields unchanged.
- The approval permission is enforced by the server and is not dependent on a client-side check alone.
