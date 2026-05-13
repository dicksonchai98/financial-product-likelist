## MODIFIED Requirements

### Requirement: User SHALL create like-list entries for financial products
The system SHALL allow an authenticated user to create a like-list entry with product name, price, fee rate, debit account, and order quantity. Authentication SHALL be established by `account-auth-flow` cookie login before access to create operations.

#### Scenario: Create like-list entry successfully
- **WHEN** an authenticated user submits valid product and order data
- **THEN** the system creates the like-list entry tied to that user's `UserID`

#### Scenario: Reject invalid create payload
- **WHEN** a user submits missing or invalid required fields (e.g., non-positive quantity or price)
- **THEN** the system rejects the request and returns validation errors

### Requirement: System SHALL return like-list query results with required fields
The system SHALL provide a like-list query view that returns product name, debit account, total amount, total fee, and user contact email for the authenticated user. Unauthenticated requests SHALL be redirected to login by account-auth-flow.

#### Scenario: Query own like-list data
- **WHEN** an authenticated user requests like-list data
- **THEN** the system returns only records belonging to that user's `UserID` with required display fields

### Requirement: User SHALL update like-list entries
The system SHALL allow an authenticated user to update product name, price, fee rate, debit account, and order quantity for an existing like-list entry owned by that user.

#### Scenario: Update own like-list entry successfully
- **WHEN** an authenticated user updates an entry they own with valid data
- **THEN** the system persists the update and recalculates derived amounts

#### Scenario: Prevent cross-user update
- **WHEN** a user attempts to update an entry owned by another user
- **THEN** the system denies the operation

### Requirement: User SHALL delete like-list entries
The system SHALL allow an authenticated user to delete a like-list entry that they own.

#### Scenario: Delete own like-list entry successfully
- **WHEN** an authenticated user requests deletion of an entry they own
- **THEN** the system removes that like-list entry

#### Scenario: Delete does not remove shared product master data
- **WHEN** a user deletes a like-list entry linked to a product master record
- **THEN** the system keeps the product master record intact
