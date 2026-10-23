# AI in SDLC Assignment

## 1. Feature Overview

Implemented role-based invoice approval rules in the Invoice Approval Demo App.

- Managers may approve invoices up to 50,000.
- Directors may approve invoices up to 500,000.
- Invoices above 500,000 require two distinct approvers, with the second approver being a Director.
- Rejected approval attempts must provide a clear reason and leave invoice state unchanged.

## 2. Unit Test Results

The approval service unit tests passed during implementation. Record the final test count and date from the latest `dotnet test` run before submission.

## 3. Playwright End-to-End Test

Added an E2E test covering a Manager attempting to approve a 50,001 invoice. The test verifies that an error is shown and the invoice remains pending without approval decision fields being changed.

Record the latest E2E test result before submission.

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

Document the actual agents, sub-agents, skills, hooks, and AI interactions used during the work. Include available logs and the purpose of each interaction. Do not claim tools or capabilities were used unless there is evidence.

## 6. Pull Request Review

Record the actual findings from the agent review. For every finding, include one verdict:
- **Fixed:** describe the change and supporting evidence.
- **Dismissed:** explain why the finding is not valid.
- **Deferred:** explain why it remains unresolved.

Add the PR URL and review summary after the review is completed.

## 7. Commits

List the actual commit hashes and messages from `git log --oneline`.

## 8. Demo Video

**Video link:** Add the final 8–10 minute demo video URL here after uploading it.

The video should demonstrate the implementation, unit tests, Playwright E2E test, mutation-testing run, and relevant results.