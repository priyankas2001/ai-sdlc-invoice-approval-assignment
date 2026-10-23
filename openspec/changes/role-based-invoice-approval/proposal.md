# Proposal

## Why

The current invoice approval flow is role-agnostic and only supports a single approver decision. The assignment specifies the approval boundaries and the high-value approval requirements, but it does not explicitly name the first approver role for invoices above $500,000. To satisfy the stated requirements without inventing a new policy, this proposal distinguishes required behavior from planning assumptions and keeps the assumption clearly labeled.

## What Changes

- Add explicit approval roles for `Manager` and `Director` only. Existing `Approver` and `Admin` roles will not automatically inherit approval permissions.
- Preserve unrelated admin functionality in the application while keeping approval authority limited to the two new roles.
- Enforce Manager approval for amounts up to and including $50,000, and Director approval for amounts up to and including $500,000.
- Require two distinct approvers for invoices above $500,000, with the second approver required to be a `Director`.
- Keep high-value invoices pending after the first valid approval and only mark them approved after the valid second Director approval is recorded.
- Reject unauthorized, duplicate, out-of-order, or invalid approval attempts without mutating invoice or approval state.
- Add unit and Playwright coverage for boundary values, over-limit attempts, duplicate-user failures, and rejected high-value approval flows.

## Requirements vs. Assumptions

### Assignment requirements
- Manager may approve an invoice individually when the amount is less than or equal to $50,000.
- Director may approve an invoice individually when the amount is less than or equal to $500,000.
- Invoices above $500,000 require two distinct approvers.
- The second approver must be a `Director`.
- The same user must never count as both approvers.
- Unauthorized or invalid approval attempts must fail without changing invoice or approval state.
- Approval permissions must be enforced on the server side.

### Planning assumption (not an additional requirement)
- The assignment does not explicitly state which role may provide the first approval on an invoice above $500,000.
- To remain narrow and avoid inventing a new approval policy, this plan does not add a new rule beyond the assignment.
- The assumption for planning is: the first approval must be from a distinct, otherwise eligible approver; because a `Manager` is not eligible above $50,000, the least-assumption candidate in practice is a `Director`, but this must be reviewed before implementation.

## Capabilities

### New Capabilities
- `invoice-approval`: Role-aware invoice approval authorization, threshold logic, pending high-value approval workflow, duplicate-approver checks, and rejection behavior for invalid approval attempts.

### Modified Capabilities
- None.

## Impact

- Domain approval logic in `backend/src/InvoiceApp.Domain/ApprovalService.cs`.
- API authorization and sign-in flow in `backend/src/InvoiceApp.Api/Controllers/ApprovalsController.cs`, `backend/src/InvoiceApp.Api/Auth/*`, and `backend/src/InvoiceApp.Api/appsettings.json`.
- Demo account configuration for explicit approval roles while preserving admin-only functions.
- Browser flow in `backend/src/InvoiceApp.Api/wwwroot/app.js` for clear error messaging and high-value approval state handling.
- Unit tests in `backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs` and Playwright coverage in `e2e/tests/`.
