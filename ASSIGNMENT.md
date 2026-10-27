# AI in SDLC Assignment

## 1. Feature Overview

Implemented role-based invoice approval rules in the Invoice Approval Demo App.

- Managers may approve invoices up to 50,000.
- Directors may approve invoices up to 500,000.
- Invoices above 500,000 require two distinct approvers, with the second approver being a Director.
- Rejected approval attempts must provide a clear reason and leave invoice state unchanged.

## 2. Unit Test Results

**Latest verified run:** 27 October 2026

Command:

```powershell
dotnet test .\backend\InvoiceApp.sln --configuration Release --no-build --nologo
```

Result:

- Passed: 25
- Failed: 0
- Skipped: 0
- Total: 25
- Target framework: .NET 8 (`net8.0`)

## 3. Playwright End-to-End Tests

**Latest verified run:** 27 October 2026

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

### Initial review

**Verdict:** Request changes.

#### PR-01 — Documentation drift

- **Severity:** Medium
- **Finding:** The previous `AGENTS.md` guidance restricted approvals to Approver/Admin, conflicting with the implemented Manager/Director rules.
- **Disposition:** Fixed.
- **Evidence:** The current `AGENTS.md` specifies Manager and Director approval limits. The guidance was updated in commit `4df4dd5`.

#### PR-02 - Incomplete assignment evidence

- **Severity:** High
- **Finding:** The assignment evidence package was incomplete.
- **Disposition:** Still open.
- **Evidence:** The report and AI interaction log have been updated, but the demo video has not yet been uploaded. Actual agent/sub-agent and hook execution remains unverified.

#### PR-Q1 - First approver for high-value invoices

- **Severity:** Medium
- **Finding:** The requirement does not explicitly specify who may provide the first approval for invoices above 500,000.
- **Disposition:** Still open; requires product clarification.
- **Evidence:** `openspec/changes/role-based-invoice-approval/design.md` documents the assumption that a Director initiates the dual-approval workflow.

### Fresh review

**Review date:** 26 October 2026
**Verdict:** Needs clarification.

The reviewer found no blocking defect against the five explicit approval rules in the reviewed domain logic and tests. However, the first-approver policy for invoices above 500,000 remains ambiguous, and the evidence package is not yet complete.

#### Findings

1. Medium - First approver policy remains ambiguous.
   The design and implementation assume that a Director can initiate the dual-approval workflow. Product confirmation is required. Update the specification, design, and tests to match the confirmed rule.

2. Medium - Assignment evidence remains incomplete.
   The demo video has not yet been uploaded, and actual multi-agent/sub-agent and hook execution has not been verified. Record only evidence of activities that actually occurred.

3. **No additional code defect identified against the explicit rules.**
   The reviewer reported that the current domain checks and state-preservation assertions align with those rules. This does not resolve the outstanding product clarification.

### Test evidence

**Latest verified terminal results from the assignment workflow:**

- Unit tests: 25 passed, 0 failed, 0 skipped.
- Full Chromium Playwright suite: 2 passed, 0 failed.
- Stryker.NET mutation score: 60.55% in the later recorded run.

**Tests reported by the fresh reviewer:**

- Unit tests: 24 passed, 0 failed.
- Focused high-value dual-approval Playwright test: 1 passed, 0 failed.

These are separate runs with different test scopes or points in the workflow; the results are recorded as reported rather than merged.

### Final review status

The latest review verdict is **Needs clarification**. Final sign-off remains pending until the first-approver policy is confirmed and the required assignment evidence is complete.

## 7. Commits

The following commits are present in the feature branch history, newest first:

| Commit | Message |
|---|---|
| `037434d` | `docs: record independent invoice approval review` |
| `8fda2bc` | `docs: update assignment evidence and test results` |
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

Video link: Pending - the required 8-10 minute demo video has not yet been recorded and uploaded.

The video should demonstrate the implementation, unit tests, Playwright E2E test, mutation-testing run, and relevant results.