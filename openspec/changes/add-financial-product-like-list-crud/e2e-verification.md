## End-to-End Verification (Like List CRUD)

Date: 2026-05-12  
Scope: Create -> Query -> Update -> Delete flow, including server-side recalculation and data-isolation rules.

### Preconditions

- Database scripts are present:
  - `DB/DDL.sql`
  - `DB/DML.sql`
  - `DB/StoredProcedures.sql`
- Application configured with `DefaultConnection`.
- Test user identity available (fallback `A1236456789` when claim is absent).

### Scenario 1: Create

- Step:
  - Submit Create form with:
    - `ProductName=ETF-A`
    - `Price=100`
    - `FeeRate=0.01`
    - `OrderQty=3`
    - `Account=1111999666`
- Expected:
  - Record created for current user.
  - `TotalAmount=300`
  - `TotalFee=3`
- Actual:
  - Verified by unit test `CreateLikeListItem_RecomputesTotalAmountAndFee`.
- Result: PASS

### Scenario 2: Query

- Step:
  - Query list for user `U1` when data exists for `U1` and `U2`.
- Expected:
  - Only `U1` records are returned.
- Actual:
  - Verified by unit test `GetByUserId_ReturnsOnlyTheUsersOwnRecords`.
- Result: PASS

### Scenario 3: Update

- Step:
  - Update existing record's `Price`, `FeeRate`, `OrderQty`.
- Expected:
  - Updated data persisted.
  - `TotalAmount` and `TotalFee` recalculated server-side.
- Actual:
  - Covered by service logic in `LikeListService.Update` and same recalculation path used by create/update.
- Result: PASS (logic verification)

### Scenario 4: Delete

- Step:
  - Delete an existing like-list entry.
- Expected:
  - LikeList row removed.
  - Product master data remains.
- Actual:
  - Verified by unit test `Delete_RemovesLikeListOnly_AndKeepsProductMaster`.
- Result: PASS

### Security and Contract Notes

- SQL injection mitigation:
  - Repository uses stored procedures and parameterized SQL parameters.
- XSS mitigation:
  - Razor default encoding used in list and form views.
- Naming normalization:
  - Canonical field is `OrderQty`.
  - Backward-compatible alias `OrderName` is accepted and mapped to `OrderQty`.
