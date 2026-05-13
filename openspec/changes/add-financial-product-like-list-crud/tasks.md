## 1. Domain Model and Validation

- [x] 1.1 Define/adjust `User`, `Product`, and `LikeList` models and view-models with required fields (`ProductName`, `Price`, `FeeRate`, `Account`, `OrderQty`, `TotalAmount`, `TotalFee`, `Email`)
- [x] 1.2 Implement server-side input validation for create/update payloads (required fields, numeric bounds, invalid values)
- [x] 1.3 Standardize naming mismatch handling (`OrderName` vs `OrderQty`) in models, mapping, and DB parameters

## 2. Stored Procedures and Repository Layer

- [x] 2.1 Create/update stored procedures for Like List CRUD (`Create`, `GetList`, `Update`, `Delete`)
- [x] 2.2 Implement repository methods that call stored procedures only (no inline SQL)
- [x] 2.3 Ensure query SP returns required list fields (product name, debit account, total amount, total fee, user email)

## 3. Service Layer Business Rules

- [x] 3.1 Implement service logic to recompute `TotalAmount = Price * OrderQty` and `TotalFee = TotalAmount * FeeRate` on create/update
- [x] 3.2 Enforce user data isolation by always scoping read/update/delete operations with `UserID`
- [x] 3.3 Implement delete behavior to remove LikeList entry without deleting Product master data

## 4. Transaction and Consistency

- [x] 4.1 Wrap multi-table create flows (`Product` + `LikeList`) in a single transaction with commit/rollback
- [x] 4.2 Wrap multi-table update flows (`Product` + `LikeList`) in a single transaction with commit/rollback
- [x] 4.3 Add failure-path handling to guarantee rollback and no partial writes

## 5. MVC UI and Security

- [x] 5.1 Implement/adjust Like List MVC pages and actions for create/query/update/delete flows
- [x] 5.2 Apply Bootstrap responsive layout to like-list pages and forms for mobile and desktop usability
- [x] 5.3 Apply anti-injection and anti-XSS controls (parameterized SP calls, encoded rendering, avoid unsafe raw output)

## 6. Database Artifacts and Verification

- [x] 6.1 Provide/update `DB/DDL.sql` for User, Product, LikeList schema
- [x] 6.2 Provide/update `DB/DML.sql` with representative sample data for required tables
- [x] 6.3 Verify end-to-end scenarios for create/query/update/delete and document expected outputs
