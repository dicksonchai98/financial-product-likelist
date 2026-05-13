## Why

目前 LikeList 已使用 `[Authorize]` 保護，但專案尚未提供可用的註冊、登入、登出流程，導致使用者會被導向不存在的 `/Account/Login`。這次變更要補齊帳號驗證流程，讓授權機制可實際運作並支援完整使用者操作。

## What Changes

- 新增 Account 驗證流程：註冊、登入、登出。
- 新增 Cookie Authentication 的 claims 建立與清除機制。
- 新增帳號相關 MVC 端點與頁面（`/Account/Login`、`/Account/Register`、`/Account/Logout`）。
- 新增 User 資料存取與驗證邏輯（Stored Procedure + parameterized calls）。
- 讓 LikeList 的 `[Authorize]` 流程可正確導向登入頁並於成功登入後回跳原頁。

## Capabilities

### New Capabilities
- `account-auth-flow`: 定義使用者註冊、登入、登出、Cookie claims/session 生命周期，以及未授權導向與回跳行為。

### Modified Capabilities
- `financial-product-like-list-crud`: 補充「需已登入且由 account-auth-flow 提供身份」的前置條件與導向行為。

## Impact

- Affected code: `Controllers/AccountController`, `Services/AuthService`, `Repositories/UserRepository`, `Models/User`, `ViewModels/Login/Register`, `Views/Account/*`。
- Database: `DB/StoredProcedures.sql` 需新增或調整 `SP_User_Register`, `SP_User_Login`；`DB/DML.sql` 需補測試帳號資料。
- APIs/Behavior: 未登入訪問 LikeList 導向登入頁；登入成功後建立 Cookie 並回跳來源頁。
- Security: 密碼需做 hash 儲存與驗證；持續使用 anti-forgery 與參數化查詢。
