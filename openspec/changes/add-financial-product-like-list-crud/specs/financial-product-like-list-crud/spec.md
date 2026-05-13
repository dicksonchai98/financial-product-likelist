## ADDED Requirements

### Requirement: User SHALL create like-list entries for financial products
The system SHALL allow an authenticated user to create a like-list entry with product name, price, fee rate, debit account, and order quantity.

#### Scenario: Create like-list entry successfully
- **WHEN** an authenticated user submits valid product and order data
- **THEN** the system creates the like-list entry tied to that user's `UserID`

#### Scenario: Reject invalid create payload
- **WHEN** a user submits missing or invalid required fields (e.g., non-positive quantity or price)
- **THEN** the system rejects the request and returns validation errors

### Requirement: System SHALL return like-list query results with required fields
The system SHALL provide a like-list query view that returns product name, debit account, total amount, total fee, and user contact email for the authenticated user.

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

### Requirement: System SHALL recompute monetary totals on the server side
The system SHALL recompute `TotalAmount` and `TotalFee` on the server side for create and update operations, and SHALL NOT trust client-provided totals.

#### Scenario: Recompute totals on create
- **WHEN** create request is processed
- **THEN** `TotalAmount` MUST equal `Price * OrderQty` and `TotalFee` MUST equal `TotalAmount * FeeRate`

#### Scenario: Recompute totals on update
- **WHEN** update request is processed
- **THEN** `TotalAmount` MUST equal `Price * OrderQty` and `TotalFee` MUST equal `TotalAmount * FeeRate`

### Requirement: System SHALL use stored procedures for all database operations
The system SHALL execute create, query, update, and delete database operations through stored procedures.

#### Scenario: CRUD operations invoke stored procedures
- **WHEN** any like-list CRUD operation is executed
- **THEN** the database access path uses stored procedures instead of inline SQL

### Requirement: System SHALL apply transactions for multi-table writes
The system SHALL wrap multi-table write operations in a single transaction when product and like-list data are modified together.

#### Scenario: Commit successful multi-table operation
- **WHEN** all statements in a multi-table operation succeed
- **THEN** the system commits the transaction

#### Scenario: Rollback failed multi-table operation
- **WHEN** any statement in a multi-table operation fails
- **THEN** the system rolls back the transaction and leaves no partial writes

### Requirement: System SHALL enforce SQL injection and XSS protections
The system SHALL prevent SQL injection via parameterized stored procedure calls and SHALL prevent XSS by encoded rendering in the UI.

#### Scenario: SQL injection payload is neutralized
- **WHEN** user input includes SQL meta-characters
- **THEN** the system treats input as data and does not execute injected SQL

#### Scenario: XSS payload is rendered safely
- **WHEN** user-provided content is displayed in the UI
- **THEN** the system outputs encoded content and does not execute scripts

### Requirement: System SHALL provide responsive UI using Bootstrap
The system SHALL implement like-list pages with Bootstrap so that create, query, update, and delete flows remain usable on desktop and mobile viewports.

#### Scenario: Like-list pages render responsively
- **WHEN** a user opens like-list pages on a mobile-sized viewport
- **THEN** form controls and result tables remain readable and operable without layout breakage

### Requirement: System SHALL provide database scripts in the DB folder
The system SHALL provide database `DDL` and `DML` scripts for required base tables and sample data under the project `DB` folder.

#### Scenario: DB scripts are present for setup
- **WHEN** a developer prepares the database from project artifacts
- **THEN** required `DDL` and `DML` scripts are available under `DB` and cover User, Product, and LikeList structures and sample data
