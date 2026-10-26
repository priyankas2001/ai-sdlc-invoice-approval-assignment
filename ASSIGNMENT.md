# AI in SDLC Assignment

## 1. Feature Overview

Implemented role-based invoice approval rules in the Invoice Approval Demo App.

- Managers may approve invoices up to 50,000.
- Directors may approve invoices up to 500,000.
- Invoices above 500,000 require two distinct approvers, with the second approver being a Director.
- Rejected approval attempts must provide a clear reason and leave invoice state unchanged.

## 2. Unit Test Results

**Latest verified run:** 26 October 2026

Command:

```powershell
dotnet test .\backend\InvoiceApp.sln --nologo
```

Result:

- Passed: 25
- Failed: 0
- Skipped: 0
- Total: 25
- Target framework: .NET 8 (`net8.0`)

## 3. Playwright End-to-End Tests

**Latest verified run:** 26 October 2026

Command, run from the `e2e` directory:

```powershell
npx playwright test --project=chromium --reporter=list
```

Result: **2 passed, 0 failed.**

Scenarios verified:

1. **Manager over-limit rejection:** A Manager attempts to approve an invoice of 50,001. The test verifies that an error is displayed and the invoice remains pending without approval decision fields being changed.
2. **High-value dual approval:** Two distinct Directors approve an invoice of 500,001. The test verifies that the first approval leaves the invoice pending, while the second Director's approval completes the workflow and records the distinct approvers.

## 4. Mutation Testing

- **Tool:** Stryker.NET 4.13.0
- **Target project:** `InvoiceApp.Domain`
- **Test project:** `InvoiceApp.Tests`

### Initial run

- Mutants tested: 81
- Killed: 64
- Survived: 17
- Skipped: 55 (28 NoCoverage, 27 Ignored)
- Timeouts: 0
- Mutation score: 58.72%

### Run after additional tests

- Unit tests passed: 24
- Mutants tested: 83
- Killed: 66
- Survived: 17
- Skipped: 53 (26 NoCoverage, 27 Ignored)
- Timeouts: 0
- Mutation score: 60.55%

### Comparison

The measured mutation score increased by 1.83 percentage points, from 58.72% to 60.55%. The later run killed two more mutants, while 17 mutants survived in both runs. The total mutant population and skipped-mutant counts differed between runs, so the scores should be interpreted as results from two separate executions.

The surviving mutants still require review to determine whether they represent meaningful test gaps or equivalent/unimportant mutations. No claim is made that all survivors have been eliminated.

## 5. AI-Assisted Development Evidence

AI assistance was used for requirements guidance, OpenSpec planning, implementation and test design, evidence documentation, and independent review interactions.

Evidence available in the repository includes:

- `AGENTS.md` — repository guidance for role-based invoice approval and the development workflow.
- `openspec/changes/role-based-invoice-approval/` — feature proposal, design, tasks, and specification.
- `.github/prompts/` and `.github/skills/` — OpenSpec workflow prompts and skills.
- `.mcp.json` — Playwright MCP configuration.
- `docs/evidence/AI-INTERACTION-LOG.md` — recorded AI-assisted tasks, outcomes, and artifact references.

Two separate Copilot Chat review interactions were reported during development. They are recorded as review interactions, not as autonomous agents or sub-agents.

**Outstanding evidence:** Actual use of multiple independent agents or sub-agents has not been verified. A hook-generated interaction log has not been verified either. The presence of configuration files alone does not prove execution. These requirements remain pending until supported by actual evidence.

## 6. Pull Request Review

**Pull Request URL:** https://github.com/priyankas2001/ai-sdlc-invoice-approval-assignment/pull/1

**Review verdict:** Request changes (initial review; final re-review pending)

### PR-01 — Documentation drift

- **Severity:** Medium
- **Location:** `AGENTS.md`, Conventions section
- **Finding:** The existing guidance stated that approvals were restricted to Approver or Admin, conflicting with the implemented Manager and Director policy.
- **Verdict:** Fixed — the approval guidance was updated to match the feature's role model.
- **Verification:** Confirm the current `AGENTS.md` wording and diff before final sign-off.

### PR-02 — Incomplete assignment evidence

- **Severity:** High
- **Location:** `ASSIGNMENT.md`
- **Finding:** Required evidence was incomplete, including the AI interaction log, review record, pull request URL, and demo video URL.
- **Verdict:** Deferred — the evidence package is still being completed. Reassess this finding after all required artifacts and links exist.

### PR-Q1 — First approver for high-value invoices

- **Severity:** Medium
- **Location:** `openspec/changes/role-based-invoice-approval/design.md`
- **Finding:** The original requirement does not explicitly state which role may provide the first approval for invoices above 500,000.
- **Verdict:** Deferred — confirm the business rule before final sign-off. The current implementation assumes a Director can initiate the dual-approval workflow.

### Review scope and reported test results

The reviewer reported inspecting the approval service, invoice model, API authorization/session flow, unit tests, Playwright test, OpenSpec artifacts, agent guidance, and repository configuration.

The reviewer reported:
- Unit tests: 24 passed, 0 failed.
- Playwright E2E: 1 passed.
- Mutation testing: not run by the reviewer; separate Stryker results are recorded in the Mutation Testing section.

These results are attributed to the review output and should be supported by the corresponding terminal evidence where available.

### Final review status

The initial review verdict was **Request changes**. Re-review the branch after addressing the findings and record the actual final verdict before submission.

## 7. Commits

The following commits are present in the feature branch history, newest first:

| Commit | Message |
|---|---|
| `8bbaf1e` | `test: cover high-value dual approval in Playwright` |
| `9839fcd` | `test: strengthen high-value approval state assertions` |
| `310c638` | `docs: update verified approval implementation tasks` |
| `11d110c` | `docs: clean assignment evidence report` |
| `4df4dd5` | `docs: fix approval role guidance` |
| `3c07007` | `docs: add AI interaction evidence log` |
| `f1eaf10` | `docs: align agent guidance and record review findings` |
| `7d94176` | `test: strengthen approval validation and document evidence` |
| `e81adea` | `test: cover manager over-limit rejection in Playwright` |
| `fd2a81c` | `feat: require dual approval for high-value invoices` |
| `fc12cc7` | `feat: enforce manager and director approval limits` |

## 8. Demo Video

**Video link:** Add the final 8–10 minute demo video URL here after uploading it.

The video should demonstrate the implementation, unit tests, Playwright E2E test, mutation-testing run, and relevant results.