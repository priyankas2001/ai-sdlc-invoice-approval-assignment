# Project conventions for AI agents

Invoice approval demo app: ASP.NET Core 8 Web API with a small static web UI.

## Layout

- `backend/src/InvoiceApp.Domain`: business rules, no web or storage code. `ApprovalService` owns the invoice lifecycle (Draft, PendingApproval, Approved, Rejected).
- `backend/src/InvoiceApp.Api`: controllers, sign-in (`Auth/`), in-memory repository, and the web UI under `wwwroot/`.
- `backend/tests/InvoiceApp.Tests`: xUnit tests, one test class per class under test, named `<ClassUnderTest>Tests`.
- `e2e`: Playwright end-to-end tests (`tests/`) and page objects (`pages/`).
- `docs/use-cases`: short briefs for features we may build.

## Conventions

- Controllers live in `Controllers/`, one per resource, route `api/[controller]`. Request bodies are `record` types declared inside the controller.
- Controllers inherit `ApiControllerBase` and use `Handle(...)` so service errors map to 404, 409 and 400.
- E- Every endpoint is behind sign-in. Restrict actions with `[RequireRole("...")]`: destructive actions are Admin only; invoice approvals are restricted to Manager and Director roles according to the role-based approval rules below.
- Business rules go in the Domain project, never in controllers.
- Register services in `Program.cs`. Repositories are interfaces in Domain with an in-memory implementation in Api.
- Every new rule or endpoint needs tests. Use `FakeInvoiceRepository` in tests.
- Use clear names; no one-letter variables or `flag` parameters.
- End-to-end tests use page objects and role-based locators (`getByRole`, `getByLabel`), never CSS or XPath selectors.

## Commands

- Run the app: `dotnet run --project backend/src/InvoiceApp.Api` then open http://localhost:5000
- Run the tests: `dotnet test backend/InvoiceApp.sln`
- Run end-to-end tests: `cd e2e && npm install && npx playwright install chromium && npm test`

## Demo accounts (not real credentials)

- `demo.approver` / `ChangeMe123!` (role Approver)
- `demo.admin` / `ChangeMe123!` (role Admin)

## Never

- Commit real credentials, tokens or customer data.
- Add business logic to `Program.cs` or controllers.


## Role-Based Invoice Approval Assignment

### Approval requirements

- A Manager may approve invoices with an amount up to and including 50,000.
- A Director may approve invoices with an amount up to and including 500,000.
- An invoice above 500,000 requires two distinct approvers.
- The second approver for an invoice above 500,000 must be a Director.
- The same authenticated user must never count as both approvers for one invoice.
- A rejected approval attempt must return a clear, actionable reason and leave all invoice state unchanged, including status, decision fields, and recorded approvers.

### Implementation expectations

- Keep business rules in `backend/src/InvoiceApp.Domain/ApprovalService.cs`, not in controllers or browser code.
- Preserve the existing ASP.NET Core API, authentication/session flow, in-memory repository, and invoice lifecycle unless a documented requirement requires a change.
- Enforce authorization on the server using the authenticated session's username and role. Never trust a role supplied by the client.
- Represent multiple approvers explicitly when required. Record enough information to distinguish the first and second approvers.
- Validate approval eligibility before mutating the invoice or persisting changes.
- Return clear domain errors that the existing API error-handling layer can translate into appropriate HTTP responses.
- Keep the existing `Draft`, `PendingApproval`, `Approved`, and `Rejected` lifecycle semantics intact.

### Required tests

- Add xUnit tests for the Manager and Director amount limits, including exact boundary values.
- Test amounts immediately above each applicable limit.
- Test that invoices above 500,000 cannot be approved by one approver alone.
- Test that a second distinct Director can complete the required approval.
- Test that the same username cannot count as both approvers.
- Test that a non-Director cannot complete the second approval for an invoice above 500,000.
- For rejected attempts, assert that invoice status, decision fields, and approver records remain unchanged.
- Review every AI-generated test for meaningful assertions, correct setup, and realistic failure detection.
- Add a Playwright end-to-end test for a rejected over-limit approval through the user interface, verifying the visible reason and unchanged invoice state.

### Project commands

- Run the application: `dotnet run --project backend/src/InvoiceApp.Api`
- Run unit tests: `dotnet test backend/InvoiceApp.sln`
- Run end-to-end tests: `cd e2e; npm install; npx playwright install chromium; npm test`
- Run a Release build: `dotnet build backend/InvoiceApp.sln --configuration Release`
- Run mutation testing using the assignment-compatible .NET 8 tool version: `dotnet-stryker` version `4.13.0`.

### Specification and implementation workflow

- Use a spec-driven tool such as Spec Kit, OpenSpec, or BMAD to generate the feature specification and task breakdown. Commit the generated artifacts.
- Implement the feature in at least two separate steps: first single-approver limits with tests, then the two-approver rule with tests.
- Make each implementation step its own meaningful commit describing what changed and why.
- Use multiple agents or sub-agents for appropriate independent tasks, such as specification review, test review, and implementation review. Keep final implementation and acceptance decisions under human review.
- Use appropriate available agent skills and hooks for structured output and AI interaction logging. Preserve existing repository configuration and document any new configuration.

### Review expectations

- Review the complete pull request diff, including tests, specification files, agent configuration, and documentation.
- Run the workshop review prompt against the pull request diff.
- Record every review finding in the pull request description with one verdict: `fixed`, `dismissed` with a reason, or `deferred` with a reason.
- Never claim a test, mutation score, review, or demo has succeeded without recorded evidence.
- Record the baseline and final mutation scores as actual tool output.
- Keep an AI interaction log containing relevant prompts, responses, agent/sub-agent activity, and hook-generated records. Do not log credentials or session tokens.
- Create `ASSIGNMENT.md` at the repository root containing the write-up, evidence, pull request link, and demo video link.

### General safety and quality

- Do not commit real credentials, tokens, or customer data.
- Do not introduce business logic into `Program.cs` or controllers.
- Follow existing naming, layering, testing, and Playwright page-object conventions.
- Investigate unexpected behavior rather than weakening tests or changing unrelated application behavior.