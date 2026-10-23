# Tasks

## 1. Stage 1: single-approver threshold and role checks

- [ ] 1.1 Define the approval policy for `Manager` and `Director` limits and confirm role compatibility assumptions for legacy `Approver` and `Admin` roles before code changes; verify that only `Manager` and `Director` receive approval permissions by default and that unrelated admin actions remain intact.
- [ ] 1.2 Update the domain approval rules in `ApprovalService` to enforce `Manager` approval up to and including $50,000 and `Director` approval up to and including $500,000; verify the service rejects attempts above the allowed limit and returns a clear reason.
- [ ] 1.3 Add unit tests covering exact thresholds, just-over-threshold amounts, unauthorized roles, and state preservation on rejected attempts; verify with `dotnet test backend/InvoiceApp.sln`.
- [ ] 1.4 Review authentication and demo-role configuration so `Manager` and `Director` are explicit, server-side roles without silently granting legacy `Approver` or `Admin` conflicting approval permissions; verify by reviewing the configured user matrix and API behavior.

## 2. Stage 2: dual-approver policy for invoices above $500,000

- [ ] 2.1 Extend the approval model to support a pending-first-approval workflow for amounts over $500,000: the assignment requires a second distinct approver and the second approver must be a `Director`; the first approval remains subject to the narrowest interpretation and must be treated as a valid, otherwise eligible approver while the invoice stays pending until the second `Director` approval is recorded.
- [ ] 2.2 Enforce that the second approver must be a different user and must be a `Director`, and reject duplicate approvers, invalid approval order, and non-`Director` second approvals; verify with unit tests and state-preservation assertions.
- [ ] 2.3 Add unit tests for duplicate approvers, invalid approval sequences, non-Director second approvals, successful two-person approval, and unchanged state after rejected high-value attempts; verify with `dotnet test backend/InvoiceApp.sln`.
- [ ] 2.4 Add a Playwright end-to-end test for a rejected over-limit approval through the UI, asserting the visible clear reason and unchanged invoice state; verify with `cd e2e && npm install && npx playwright install chromium && npm test`.

## 3. Review and acceptance gate

- [ ] 3.1 Document the required testing commands and review expectations in the PR or project docs; verify the commands match the repo standard (`dotnet test backend/InvoiceApp.sln` and `cd e2e && npm install && npx playwright install chromium && npm test`).
- [ ] 3.2 Confirm acceptance criteria with reviewers: thresholds are exact, invalid attempts fail without state mutation, server-side enforcement is in place, the two-person over-limit flow remains pending until the second Director approval, and the UI clearly exposes rejection reasons.
- [ ] 3.3 Record review findings and evidence in the PR description; do not claim test or review success without recorded evidence.

## Workflow follow-up

- Archive the change after review and verification requirements are satisfied.
- Re-run `openspec status --change role-based-invoice-approval` to confirm the planning artifacts are complete before implementation begins.

## Business decision pending review

- The assignment does not specify the role of the first approver on invoices above $500,000. This plan intentionally does not invent a new policy; the current assumption is that the first approval must come from an otherwise eligible, distinct approver and the invoice remains pending until a second distinct `Director` approves it.
