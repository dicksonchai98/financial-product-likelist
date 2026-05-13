## ADDED Requirements

### Requirement: System SHALL support user registration
The system SHALL allow a new user to register with required identity fields and credentials, and SHALL persist the user through stored procedures.

#### Scenario: Register with valid input
- **WHEN** a user submits valid `UserID`, `UserName`, `Email`, `Account`, and password
- **THEN** the system creates the user record and stores password as hash (not plaintext)

#### Scenario: Reject duplicated identity
- **WHEN** a user submits a `UserID` or unique login identity that already exists
- **THEN** the system rejects registration and returns a validation error

### Requirement: System SHALL support user login via cookie authentication
The system SHALL authenticate users with account credentials and establish an authenticated cookie session containing identity claims.

#### Scenario: Login succeeds with valid credentials
- **WHEN** a user submits valid credentials
- **THEN** the system signs in the user and issues an authentication cookie with `UserID`, `UserName`, and `Email` claims

#### Scenario: Login fails with invalid credentials
- **WHEN** a user submits invalid credentials
- **THEN** the system rejects login and does not issue an authentication cookie

### Requirement: System SHALL support logout and session termination
The system SHALL provide a logout action that invalidates the current authentication cookie.

#### Scenario: Logout clears authentication
- **WHEN** an authenticated user invokes logout
- **THEN** the system signs out the user and removes authenticated access to protected pages

### Requirement: Protected routes SHALL redirect unauthenticated users to login
The system SHALL redirect unauthenticated requests for protected pages to the login endpoint.

#### Scenario: Unauthenticated access to protected route
- **WHEN** an unauthenticated user requests `/LikeList`
- **THEN** the system redirects to `/Account/Login` with a `ReturnUrl` for the original target

### Requirement: Login flow SHALL support safe local return URL redirect
The system SHALL redirect users to local `ReturnUrl` after successful login and SHALL prevent open redirect to external URLs.

#### Scenario: Local return URL redirect
- **WHEN** login succeeds and `ReturnUrl` is a local path
- **THEN** the system redirects the user to that local path

#### Scenario: External return URL blocked
- **WHEN** login succeeds and `ReturnUrl` is an external URL
- **THEN** the system ignores that value and redirects to a safe default page

### Requirement: User authentication data access SHALL use stored procedures
The system SHALL perform user registration and login lookup through stored procedures with parameterized inputs.

#### Scenario: Register and login operations use stored procedures
- **WHEN** registration or login data is accessed
- **THEN** the repository invokes stored procedures and does not execute inline SQL
