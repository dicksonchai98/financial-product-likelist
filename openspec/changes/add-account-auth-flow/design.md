## Context

目前系統已完成 LikeList CRUD，並在 `LikeListController` 上套用 `[Authorize]`。但專案尚未提供可操作的 `Account/Login`、`Account/Register`、`Account/Logout` 流程，導致未授權使用者雖會被導向登入路徑，實際上無法完成驗證與回跳。

系統目前部署形態為 Docker（app + sqlserver + nginx），資料層依既有規範使用 Stored Procedure，安全需求包含 SQL Injection、XSS 與 Cookie-based session。此次設計需在不破壞既有 LikeList 能力的前提下，補齊可運作的身份流程。

## Goals / Non-Goals

**Goals:**
- 建立可用的 Account 流程：Register / Login / Logout。
- 使用 Cookie Authentication 建立與清除身份狀態（claims 含 `UserID`, `UserName`, `Email`）。
- 將 LikeList 的授權前置條件從「假設已登入」改為「可透過 Account 流程取得登入狀態」。
- 持續使用 Stored Procedure 存取 User 資料，避免 inline SQL。
- 支援未登入導向登入頁，登入成功回跳 `ReturnUrl`。

**Non-Goals:**
- 不引入第三方 OAuth / OpenID Connect（Google/Microsoft login）。
- 不新增角色權限模型（RBAC）或細粒度授權政策。
- 不處理密碼重設、雙因子驗證、帳號鎖定等進階安全流程。
- 不重構既有 LikeList 業務規則（金額計算、交易邏輯保持不變）。

## Decisions

1. Authentication 機制採 ASP.NET Core Cookie Authentication
- Decision: 使用既有中介軟體，設定 LoginPath/LogoutPath/AccessDeniedPath。
- Rationale: 與 MVC 表單流程天然契合，實作成本低，且符合設計文件原始規劃。
- Alternative considered: JWT。捨棄原因是本系統以伺服器渲染頁為主，Cookie 更直接。

2. 帳號流程分層：Controller -> AuthService -> UserRepository
- Decision: `AccountController` 僅處理流程與回應；驗證與密碼比對集中於 `AuthService`；資料庫操作由 `UserRepository` 執行 SP。
- Rationale: 降低控制器責任，讓登入邏輯可測試並與資料層解耦。
- Alternative considered: Controller 直接打 DB。捨棄原因是可測試性與維護性差。

3. 密碼儲存策略：不可逆 hash（含 salt）
- Decision: 註冊時產生 salt + hash，資料庫僅存 hash 與 salt（或合併格式）；登入比對 hash。
- Rationale: 避免明文密碼，符合基本安全要求。
- Alternative considered: 明文或可逆加密。捨棄原因是高風險且不符合安全基準。

4. ReturnUrl 回跳與開放重導防護
- Decision: 登入成功後僅允許 `Url.IsLocalUrl(ReturnUrl)` 的本地回跳，否則導向 `/LikeList`。
- Rationale: 防止 open redirect。
- Alternative considered: 無條件導向 ReturnUrl。捨棄原因是安全風險高。

5. 與 Docker/Nginx 的協作
- Decision: 保持 app behind nginx；在 Development 環境避免 https 重導干擾；保留 forwarded headers。
- Rationale: 現有部署即為反向代理，需確保登入跳轉 URL 與 port 一致。
- Alternative considered: 移除 nginx。捨棄原因是違反原先架構目標。

## Risks / Trade-offs

- [Risk] 密碼 hash 實作不一致導致登入失敗 → Mitigation: 使用單一 `PasswordHasher` 元件，註冊與登入共用。
- [Risk] ReturnUrl 處理不當造成 open redirect → Mitigation: 嚴格使用 `Url.IsLocalUrl` 檢查。
- [Risk] 舊資料沒有 hash/salt 欄位 → Mitigation: SP 與 DDL 一次補齊，初始化腳本同步更新。
- [Risk] Cookie 設定過寬造成安全風險 → Mitigation: 設定 `HttpOnly`, `SameSite=Lax`，並在正式環境強制 `SecurePolicy=Always`。
- [Risk] 反向代理環境下 URL 重寫異常 → Mitigation: 以 nginx 設定固定 host/port 傳遞並在 compose 驗證登入導向。

## Migration Plan

1. 更新 DB 腳本與 SP：新增/調整 `SP_User_Register`, `SP_User_Login` 所需欄位（hash/salt）。
2. 新增 `AuthService`, `UserRepository`, `PasswordHasher`，接入 DI。
3. 新增 `AccountController` 與 Login/Register Razor 頁面。
4. 設定 Cookie Authentication 參數與登入成功回跳邏輯。
5. 執行測試（服務層 + 控制器流程）與 Docker 手動驗證（`/LikeList` -> `/Account/Login` -> login success -> return）。

## Open Questions

- 是否沿用現有 `UserID` 作為登入帳號，或新增 `Account`/`Email` 作為唯一登入識別？
- 是否要在本次就加上「註冊時帳號唯一性錯誤提示」的完整 UX 文案？
- 是否在這次變更同時補「預設測試帳號」初始化策略（僅 dev）？
