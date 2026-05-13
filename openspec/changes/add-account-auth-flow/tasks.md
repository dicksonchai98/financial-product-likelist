## 1. Account MVC and ViewModels

- [ ] 1.1 Create `AccountController` with `Login`, `Register`, and `Logout` actions
- [ ] 1.2 Add `LoginViewModel` and `RegisterViewModel` with validation attributes
- [ ] 1.3 Create Razor views for `Views/Account/Login.cshtml` and `Views/Account/Register.cshtml`

## 2. Authentication Service and Cookie Flow

- [ ] 2.1 Implement `AuthService` for register/login credential verification
- [ ] 2.2 Implement password hashing and verification utility (hash + salt)
- [ ] 2.3 Wire cookie sign-in/sign-out with claims (`UserID`, `UserName`, `Email`) and local `ReturnUrl` handling

## 3. User Repository and Stored Procedures

- [ ] 3.1 Implement `UserRepository` to call `SP_User_Register` and `SP_User_Login` via parameterized stored procedure calls
- [ ] 3.2 Update `DB/StoredProcedures.sql` with account-related procedures and required parameters
- [ ] 3.3 Update `DB/DDL.sql` / `DB/DML.sql` for user auth fields and development test users

## 4. LikeList Integration

- [ ] 4.1 Configure cookie authentication options (`LoginPath`, `LogoutPath`, `AccessDeniedPath`) in `Program.cs`
- [ ] 4.2 Ensure unauthenticated `/LikeList` requests redirect to login with `ReturnUrl`
- [ ] 4.3 Ensure successful login redirects back to local `ReturnUrl` or safe default `/LikeList`

## 5. Security and Validation

- [ ] 5.1 Reject duplicate user identity on registration with clear validation message
- [ ] 5.2 Ensure no plaintext password persistence and verify hashing behavior
- [ ] 5.3 Prevent open redirect by allowing only local return URLs

## 6. Verification

- [ ] 6.1 Add/update automated tests for register, login success/failure, logout, and return-url behavior
- [ ] 6.2 Validate Docker startup still initializes DB scripts including user auth procedures
- [ ] 6.3 Perform E2E check: unauthenticated `/LikeList` -> login -> redirected back -> CRUD access granted
