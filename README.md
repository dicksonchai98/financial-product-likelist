# Financial Product Like List MVC

這是一個使用 ASP.NET Core MVC 與 SQL Server 開發的金融商品喜好清單範例，並透過 Docker Compose 進行容器化部署。

## 技術棧

- ASP.NET Core MVC (.NET 10)
- ADO.NET (`Microsoft.Data.SqlClient`)
- SQL Server 2022
- Stored Procedures
- Nginx（反向代理）
- Docker Compose
- xUnit（單元測試）

## 功能特色（Features）

- 會員註冊（UserName + Email + Password）
- 會員登入/登出（Cookie Authentication）
- 喜好清單功能：新增、列表、編輯、刪除
- 商品主檔共用：建立清單時會自動對應/建立商品資料
- 權限控制：未登入不可存取 Like List
- 後端金額重算：`TotalAmount = Price * OrderQty`、`TotalFee = TotalAmount * FeeRate`
- 安全基線：Anti-Forgery、參數化 SP 呼叫、Razor 預設 HTML Encode

## 後續優化方向

1. 架構優化：拆分 Query 與 Command
- 現況：Service + Repository 已分層，但讀寫仍在同一 repository 介面。
- 優化：改為 CQRS 風格（例如 Query Repository + Command Repository），讓邏輯更清晰、維護性更高。

2. 驗證機制優化：補齊更細緻授權策略
- 現況：使用 Cookie Authentication + `[Authorize]`。
- 優化：可加入 policy-based authorization 與資源層授權，以便未來擴展多角色需求。

3. 資料存取優化：統一參數與錯誤策略
- 現況：已改用明確 SQL 型別參數，並補齊更新/刪除找不到資料的處理。
- 優化：可再抽出共用 Command helper 與一致的 DB 例外映射規範。

4. 可觀測性優化：補齊監控與告警
- 現況：有基本錯誤頁與例外流程。
- 優化：導入集中式 logging、metrics 與 tracing，建立 API latency/DB failure 告警。

5. 安全性優化：生產環境密鑰管理
- 現況：本機以 `appsettings` / `.env` 示範。
- 優化：導入 Secret Manager / Vault，避免敏感資訊出現在設定檔。

## 實作題需求對照（依「【新進.Net】玉山銀行軟體工程師實作題 II」）

1. 新增喜好金融商品
- 你怎麼做：
  - 介面新增 Like Item（選商品、填購買數量）
  - 產品名稱/價格/費率由產品主檔帶入，後端重算總額與手續費
- 實作位置：
  - Controller：[LikeListController.cs](C:\Users\dickson\Desktop\financial-product-likelist\FinancialProductLikelist.Web\Controllers\LikeListController.cs)
  - Service：[LikeListService.cs](C:\Users\dickson\Desktop\financial-product-likelist\FinancialProductLikelist.Web\Services\LikeListService.cs)
  - Repository：[SqlLikeListRepository.cs](C:\Users\dickson\Desktop\financial-product-likelist\FinancialProductLikelist.Web\Repositories\SqlLikeListRepository.cs)

2. 查詢喜好金融商品清單
- 你怎麼做：
  - 清單顯示商品名稱、價格、手續費率、購買數量、預計扣款總額、總手續費、扣款帳號、Email
- 實作位置：
  - View：[Views/LikeList/Index.cshtml](C:\Users\dickson\Desktop\financial-product-likelist\FinancialProductLikelist.Web\Views\LikeList\Index.cshtml)
  - SP：[StoredProcedures.sql](C:\Users\dickson\Desktop\financial-product-likelist\DB\StoredProcedures.sql)

3. 刪除喜好金融商品資訊
- 你怎麼做：
  - 清單頁可刪除，僅可刪除目前登入者自己的資料
  - 刪除失敗（資料不存在）會回友善訊息
- 實作位置：
  - Controller：[LikeListController.cs](C:\Users\dickson\Desktop\financial-product-likelist\FinancialProductLikelist.Web\Controllers\LikeListController.cs)
  - Service：[LikeListService.cs](C:\Users\dickson\Desktop\financial-product-likelist\FinancialProductLikelist.Web\Services\LikeListService.cs)

4. 更改喜好金融商品資訊
- 你怎麼做：
  - 提供編輯頁修改商品、購買數量，並由後端重新計算總額與手續費
  - 更新不到資料時回 404
- 實作位置：
  - Controller：[LikeListController.cs](C:\Users\dickson\Desktop\financial-product-likelist\FinancialProductLikelist.Web\Controllers\LikeListController.cs)
  - Service：[LikeListService.cs](C:\Users\dickson\Desktop\financial-product-likelist\FinancialProductLikelist.Web\Services\LikeListService.cs)

5. 系統架構要求（C# / ASP.NET 10+ MVC、三層式）
- 你怎麼做：
  - C# + ASP.NET Core MVC (.NET 10)
  - 三層式：Nginx（Web Server）+ ASP.NET App（Application Server）+ SQL Server（RDBMS）
  - 後端分層：
    - 展示層：Controllers + Views
    - 業務層：Services
    - 資料層：Repositories + Stored Procedures
- 實作位置：
  - 啟動設定：[Program.cs](C:\Users\dickson\Desktop\financial-product-likelist\FinancialProductLikelist.Web\Program.cs)
  - 容器架構：[docker-compose.yml](C:\Users\dickson\Desktop\financial-product-likelist\docker-compose.yml)

6. 技術要求對照
- 使用 Bootstrap 支援 RWD
  - 做法：Layout 引入 Bootstrap CSS/JS，頁面使用 Bootstrap 元件
  - 位置：[Views/Shared/_Layout.cshtml](C:\Users\dickson\Desktop\financial-product-likelist\FinancialProductLikelist.Web\Views\Shared\_Layout.cshtml)
- 透過 Stored Procedure 存取資料庫
  - 做法：LikeList/User/Product 主流程走 SP（`SP_LikeList_*`、`SP_User_*`、`SP_Product_Upsert`）
  - 位置：[StoredProcedures.sql](C:\Users\dickson\Desktop\financial-product-likelist\DB\StoredProcedures.sql)
- 多表異動需 Transaction
  - 做法：新增與更新時，`Product upsert + LikeList` 包在同一 transaction
  - 位置：[SqlLikeListRepository.cs](C:\Users\dickson\Desktop\financial-product-likelist\FinancialProductLikelist.Web\Repositories\SqlLikeListRepository.cs)
- DDL/DML 放在 `DB` 資料夾
  - 做法：`DDL.sql`、`DML.sql`、`StoredProcedures.sql` 皆已放置
  - 位置：`DB/`
- 防止 SQL Injection 與 XSS
  - SQL Injection：以 SP + typed parameters 呼叫，不拼接字串 SQL
  - XSS：Razor 預設輸出編碼 + DataAnnotations 驗證 + Anti-Forgery Token

7. 密碼安全（加鹽雜湊）
- 你怎麼做：
  - 使用 `PasswordHasher`（PBKDF2）產生 hash/salt 儲存，登入時驗證 hash
- 實作位置：
  - [PasswordHasher.cs](C:\Users\dickson\Desktop\financial-product-likelist\FinancialProductLikelist.Web\Services\PasswordHasher.cs)
  - [AuthService.cs](C:\Users\dickson\Desktop\financial-product-likelist\FinancialProductLikelist.Web\Services\AuthService.cs)

## 啟動前需要準備什麼

請先安裝以下工具：

1. Git
2. Docker Desktop（需支援 Docker Compose）
3. .NET SDK 10.0（只有在不使用 Docker 本機啟動時需要）

## 下載專案

```bash
git clone <your-repo-url>
cd financial-product-likelist
cp .env.example .env
```

## 啟動方式（建議使用 Docker）

啟動所有服務：

```bash
docker compose up --build
```

背景執行：

```bash
docker compose up -d --build
```

停止服務：

```bash
docker compose down
```

## 服務位址

- 網站（經 Nginx）：`http://localhost:8080`
- SQL Server：`localhost:1433`
  - 帳號：`sa`
  - 密碼：請使用 `.env` 的 `SA_PASSWORD`
  - 資料庫：`FinancialProductLikeListDb`

## 資料庫初始化

使用 Docker Compose 時，`db-init` 會自動依序執行：

1. `DB/DDL.sql`
2. `DB/StoredProcedures.sql`
3. `DB/DML.sql`

## 不使用 Docker 的本機啟動方式（Local .NET）

1. 先啟動本機 SQL Server（或使用 Docker 的 SQL Server）。
2. 設定 `FinancialProductLikelist.Web/appsettings.Development.json` 的連線字串與密碼。
3. 執行：

```bash
dotnet restore FinancialProductLikelist.Web/FinancialProductLikelist.csproj
dotnet run --project FinancialProductLikelist.Web/FinancialProductLikelist.csproj
```

## 如何確認啟動成功

1. 開啟 `http://localhost:8080/Account/Login`。
2. 註冊新帳號並登入後進入 `LikeList`。
3. 查看容器狀態：

```bash
docker compose ps
```

4. 查看服務日誌：

```bash
docker compose logs -f
```

## 專案結構

```text
FinancialProductLikelist.Web/
  Controllers/
  Infrastructure/
  Models/
  Repositories/
  Services/
  ViewModels/
  Views/
FinancialProductLikelist.Tests/
DB/
nginx/
docker-compose.yml
```
