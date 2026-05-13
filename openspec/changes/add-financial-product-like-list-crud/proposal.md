## Why

目前專案缺少一套可維護且可驗證的「金融商品喜好清單（Like List）」需求契約，導致後續實作容易在資料欄位、計算規則與安全要求上出現落差。這次變更要先把 CRUD 行為與必要技術約束明確化，讓開發可依同一份規格落地。

## What Changes

- 新增 Like List 金融商品資訊的完整 CRUD 規格（新增、查詢、修改、刪除）。
- 明確定義欄位與計算規則：商品名稱、價格、手續費率、扣款帳號、購買數量、總手續費、預計扣款總金額。
- 將資料庫存取方式約束為 Stored Procedure，並要求多表異動使用 Transaction。
- 納入安全需求：防止 SQL Injection 與 XSS。
- 明確列出 UI 路由與後端分層責任（MVC + service/repository）。

## Capabilities

### New Capabilities
- `financial-product-like-list-crud`: 定義使用者可對金融商品喜好清單執行新增、查詢、修改、刪除，並包含金額計算、資料隔離、資料庫與安全約束。

### Modified Capabilities
- None.

## Impact

- Affected code: `Controllers`, `Services`, `Repositories`, `Models`, `ViewModels`, `Views/LikeList`。
- Database: `DB/DDL.sql`, `DB/DML.sql`, `DB/StoredProcedures.sql` 需新增或調整對應物件。
- APIs/Behavior: Like List 相關頁面與提交流程會新增或調整。
- Dependencies/Architecture: 維持 ASP.NET Core MVC + SQL Server + Bootstrap；增加對 Transaction 與 Stored Procedure 一致性要求。
