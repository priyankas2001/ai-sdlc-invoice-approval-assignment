# Spec Delta

## Purpose

This capability defines the server-side authorization rules for invoice approvals so only `Manager` and `Director` roles may approve, high-value invoices remain pending until a second distinct `Director` approval is recorded, and invalid approval attempts leave invoice state unchanged.

## ADDED Requirements

### Requirement: Approval roles are explicit and limited
The system SHALL grant approval permissions only to the `Manager` and `Director` roles by default. Existing `Approver` and `Admin` roles SHALL NOT automatically inherit approval permissions unless a product decision explicitly reassigns them, and unrelated admin functions SHALL remain available without granting approval authority.

#### Scenario: `Manager` has approval permission by default
- **WHEN** a user with role `Manager` attempts to approve an invoice within the Manager limit
- **THEN** the approval is allowed if the invoice amount is within the authorized threshold and no higher-value dual-approval rule applies

#### Scenario: `Director` has approval permission by default
- **WHEN** a user with role `Director` attempts to approve an invoice within the Director limit
- **THEN** the approval is allowed if the invoice amount is within the authorized threshold and no higher-value dual-approval rule applies

#### Scenario: legacy roles do not gain approval rights implicitly
- **WHEN** a user with role `Approver` or `Admin` attempts to approve an invoice without an explicit approval-role assignment
- **THEN** the system rejects the attempt with a clear authorization error and the invoice remains unchanged

### Requirement: Manager single-approval limit
The system SHALL allow a `Manager` to approve an invoice only when the invoice amount is less than or equal to $50,000.

#### Scenario: Manager approves at the threshold
- **WHEN** a `Manager` attempts to approve an invoice with amount $50,000
- **THEN** the approval succeeds and the invoice is marked approved with the `Manager` recorded as the approving user

#### Scenario: Manager approval is denied just above the threshold
- **WHEN** a `Manager` attempts to approve an invoice with amount $50,001
- **THEN** the attempt fails with a clear reason and the invoice remains in its prior status with no approval record changes

### Requirement: Director single-approval limit
The system SHALL allow a `Director` to approve an invoice only when the invoice amount is less than or equal to $500,000.

#### Scenario: Director approves at the threshold
- **WHEN** a `Director` attempts to approve an invoice with amount $500,000
- **THEN** the approval succeeds and the invoice is marked approved with the `Director` recorded as the approving user

#### Scenario: Director approval is denied just above the threshold
- **WHEN** a `Director` attempts to approve an invoice with amount $500,001
- **THEN** the attempt fails with a clear reason and the invoice remains unchanged until the valid second approval requirement is satisfied

### Requirement: High-value invoices require a second distinct Director approval
The assignment SHALL require two distinct approvers for any invoice above $500,000, and the second approver SHALL be a `Director`. The invoice SHALL remain pending after the first valid approval until the second, distinct `Director` approval is recorded; the assignment does not explicitly define the first approver role above $500,000, so this is a documented planning assumption rather than an added business rule.

#### Scenario: Large invoice is not fully approved after the first valid approval
- **WHEN** a valid first approver approves an invoice with amount greater than $500,000
- **THEN** the invoice remains pending and must still receive a second distinct `Director` approval before the invoice is marked fully approved

#### Scenario: Large invoice requires dual approval and succeeds with a second Director
- **WHEN** a valid first approver and then a different `Director` approve the same invoice above $500,000
- **THEN** the invoice becomes fully approved only after the second valid `Director` approval is recorded

#### Scenario: Same user cannot count as both approvers
- **WHEN** the same authenticated user attempts to serve as both approvers on the same invoice
- **THEN** the system rejects the second approval and the invoice remains unchanged

#### Scenario: Non-Director cannot complete the second approval
- **WHEN** a `Manager` or another non-`Director` user attempts to approve the second required approval on an invoice above $500,000
- **THEN** the system rejects the attempt with a clear reason and no invoice state is mutated

#### Scenario: Invalid approval order is rejected
- **WHEN** a second approval for a large invoice is attempted before a valid first approval exists or the first approval was not valid for the amount and role
- **THEN** the system rejects the attempt and preserves the invoice and all approval records

### Requirement: Rejected approval attempts preserve invoice state
The system SHALL reject unauthorized or invalid approval attempts without modifying the invoice's status, decision metadata, or recorded approver values.

#### Scenario: Unauthorized role is rejected
- **WHEN** a user without the required role attempts to approve an invoice
- **THEN** the system returns a clear permission error and the invoice remains in its original pending state

#### Scenario: Duplicate approver is rejected without mutation
- **WHEN** a second approval attempt reuses the same user already recorded for the invoice
- **THEN** the system rejects the request and preserves the invoice's original state

#### Scenario: Rejected validation failure leaves approval records unchanged
- **WHEN** an approval attempt violates the approval threshold, role, duplicate-user, or order rules
- **THEN** the system returns a clear reason and the invoice status, decision fields, and approver records remain unchanged

### Requirement: Successful two-person approval is recorded and final
The system SHALL record both approvers distinctly for high-value invoices and SHALL only mark the invoice fully approved after the valid second approval is recorded.

#### Scenario: Successful two-person approval records both approvers in order
- **WHEN** a valid first approver and a distinct `Director` complete the required approvals for an invoice above $500,000
- **THEN** the system records each approver distinctly and marks the invoice approved only after the second valid Director approval is accepted
