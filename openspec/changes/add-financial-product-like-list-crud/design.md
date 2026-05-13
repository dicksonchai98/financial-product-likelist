## Context

本次變更針對金融商品喜好清單（Like List）建立可落地的技術設計，目標是讓 ASP.NET Core MVC 專案能一致實作新增、查詢、修改、刪除流程，並符合既有技術約束（Stored Procedure、Transaction、RWD、SQL Injection/XSS 防護）。

現況是需求分散在文件中，缺少可直接對應程式分層與資料流的設計決策，容易造成前後端欄位不一致、金額計算責任不明、與資料庫存取策略不一致。此設計文件將對齊 proposal 中定義的能力 `financial-product-like-list-crud`。

## Goals / Non-Goals

**Goals:**
- 建立 Like List CRUD 的端到端技術路徑：Controller -> Service -> Repository -> SQL Server Stored Procedure。
- 明確定義關鍵資料模型與欄位責任，尤其是 `TotalAmount`、`TotalFee` 的後端重算規則。
- 定義多資料表異動時的交易邊界（新增/修改商品與喜好紀錄）。
- 明確納入安全控制：參數化呼叫 Stored Procedure、輸出編碼防 XSS。
- 確保資料隔離：所有 Like List 查詢與異動需綁定登入使用者 `UserID`。

**Non-Goals:**
- 不在本變更內新增複雜授權模型（角色權限、細緻 ACL）。
- 不處理即時報價、行情串流或金融商品外部來源整合。
- 不設計新的前端框架（維持 MVC Razor + Bootstrap）。
- 不涵蓋效能壓測與大規模分散式擴充方案。

## Decisions

1. 採用分層架構實作 CRUD
- Decision: 以 MVC + Service + Repository 分層，Controller 只處理請求/回應與模型驗證，商業規則集中在 Service，資料存取集中在 Repository。
- Rationale: 可測試性高、責任邊界清楚，便於未來擴充與除錯。
- Alternative considered: Controller 直接呼叫 DB。捨棄原因是耦合過高、規則重複與測試困難。

2. DB 存取僅走 Stored Procedure
- Decision: 所有 Like List 讀寫透過 SP（Create/GetList/Update/Delete），避免內嵌 SQL 字串。
- Rationale: 符合題目要求，降低 SQL Injection 風險，並統一 DB 介面。
- Alternative considered: ORM 直連表格。捨棄原因是與需求約束不一致。

3. 金額計算由後端主導
- Decision: `TotalAmount = Price * OrderQty`、`TotalFee = TotalAmount * FeeRate`，於 Service 或 SP 端重算並覆蓋輸入值。
- Rationale: 防止前端竄改，確保資料一致性。
- Alternative considered: 僅採前端計算。捨棄原因是安全與一致性不足。

4. 交易邊界設在「跨表異動」用例
- Decision: 新增與修改流程若同時觸發 `Product` 與 `LikeList` 寫入，必須包在同一交易中，失敗即 rollback。
- Rationale: 避免部分成功導致資料錯亂。
- Alternative considered: 分開提交。捨棄原因是高機率留下不一致資料。

5. 使用者資料隔離為硬性規則
- Decision: Repository 層所有 Like List 查詢與更新/刪除皆需帶 `UserID` 條件。
- Rationale: 防止跨使用者資料存取。
- Alternative considered: 僅在 UI 隱藏他人資料。捨棄原因是無法防止惡意請求。

6. 安全控制採「預設安全」
- Decision: 表單使用 Anti-Forgery Token，View 預設 Razor Encode 並禁止 `Html.Raw` 顯示未清洗內容。
- Rationale: 避免常見 Web 攻擊面。
- Alternative considered: 僅依賴輸入驗證。捨棄原因是對輸出面 XSS 防護不足。

## Risks / Trade-offs

- [Risk] Product 與 LikeList 欄位命名不一致（例如 `OrderName`/`OrderQty`）可能造成 mapping 錯誤 -> Mitigation: 在 model 與 SP 參數命名統一使用 `OrderQty`，並在 specs 明確欄位契約。
- [Risk] 金額計算精度（小數位、四捨五入）不一致 -> Mitigation: 指定 decimal 型別與固定 rounding 策略，於 Service 與 DB 同步。
- [Risk] 若把太多規則放進 SP 會降低應用層可測試性 -> Mitigation: 保留關鍵規則在 Service，可在 SP 端做防呆重算。
- [Risk] 交易範圍過大可能影響併發 -> Mitigation: 只包覆必要寫入語句，縮短 transaction duration。
- [Risk] 目前能力定義未含進階稽核需求（操作記錄） -> Mitigation: 後續若有合規需求，再新增獨立 capability。
