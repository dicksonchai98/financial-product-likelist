# Financial Product Preference System Design

## 1. Overview

本系統為金融商品喜好紀錄系統，提供使用者登入後管理個人喜好的金融商品。

### 1.1 Features

- 使用者註冊
- 使用者登入 / 登出
- 新增喜好金融商品
- 查詢喜好商品清單
- 修改喜好商品
- 刪除使用者喜好紀錄

### 1.2 Tech Stack

- ASP.NET Core MVC (.NET 10+)
- SQL Server
- Stored Procedure
- Bootstrap (RWD)
- Transaction
- Cookie Authentication

## 2. Architecture

```text
Browser
  ↓
Nginx
  ↓
ASP.NET Core MVC
  ↓
SQL Server
```

## 3. Folder Structure

採用傳統 MVC 架構：

```text
FinancialProductSystem
│
├── Controllers
│   ├── AccountController.cs
│   └── LikeListController.cs
│
├── Models
│   ├── User.cs
│   ├── Product.cs
│   └── LikeList.cs
│
├── ViewModels
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   └── LikeListViewModel.cs
│
├── Services
│   ├── AuthService.cs
│   └── LikeListService.cs
│
├── Repositories
│   ├── UserRepository.cs
│   ├── ProductRepository.cs
│   └── LikeListRepository.cs
│
├── Views
│   ├── Account
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   │
│   └── LikeList
│       ├── Index.cshtml
│       ├── Create.cshtml
│       └── Edit.cshtml
│
└── DB
    ├── DDL.sql
    ├── DML.sql
    └── StoredProcedures.sql
```

## 4. Database Design

### 4.1 User

使用者資料。

| Column | Description |
| --- | --- |
| UserID | Primary Key |
| UserName | 使用者名稱 |
| Email | Email |
| Account | 扣款帳號 |
| PasswordHash | 密碼 Hash |

### 4.2 Product

金融商品主檔。

| Column | Description |
| --- | --- |
| No | PK |
| ProductName | 商品名稱 |
| Price | 商品價格 |
| FeeRate | 手續費率 |

### 4.3 LikeList

使用者喜好清單。

| Column | Description |
| --- | --- |
| SN | PK |
| UserID | FK User |
| ProductNo | FK Product |
| OrderQty | 購買數量 |
| Account | 扣款帳號 |
| TotalFee | 總手續費 |
| TotalAmount | 預計扣款金額 |

## 5. Authentication Design

使用 Cookie Authentication。

- 登入成功後建立 Cookie，保存 `UserID`、`UserName`、`Email`
- 需登入才可進入喜好商品頁面（`[Authorize]`）

## 6. Business Rules

### 6.1 金額計算

後端重新計算，不可信任前端傳入金額：

- `TotalAmount = Price × OrderQty`
- `TotalFee = TotalAmount × FeeRate`

### 6.2 刪除規則

- 刪除：`LikeList`
- 不刪除：`Product`
- 原因：`Product` 為金融商品主檔，可能被其他使用者共用

### 6.3 使用者隔離

使用者只能查詢 / 修改 / 刪除自己的資料，所有查詢條件需帶入 `UserID`。

## 7. Stored Procedure

所有 DB 存取皆透過 Stored Procedure。

### 7.1 User

- `SP_User_Register`
- `SP_User_Login`

### 7.2 LikeList

- `SP_LikeList_Create`
- `SP_LikeList_GetList`
- `SP_LikeList_Update`
- `SP_LikeList_Delete`

## 8. Transaction Design

以下情境需使用 Transaction（Commit / Rollback）：

- 新增喜好商品：可能同時新增 `Product` 與 `LikeList`
- 修改喜好商品：可能同時更新 `Product` 與 `LikeList`

## 9. Security Design

### 9.1 SQL Injection

防護方式：

- Stored Procedure
- Parameterized Query

禁止：

```csharp
string sql = "SELECT * FROM User WHERE UserID='" + userId + "'";
```

### 9.2 XSS

防護方式：

- Razor Encode
- 不使用 `Html.Raw()`

### 9.3 CSRF

使用 `[ValidateAntiForgeryToken]`。

### 9.4 Password Security

密碼不存明文，僅保存 `PasswordHash`。

## 10. Main Flow

### 10.1 Register

```text
Register
  ↓
Validate Input
  ↓
Save User
  ↓
Redirect Login
```

### 10.2 Login

```text
Login
  ↓
Validate Password
  ↓
Create Cookie
  ↓
Redirect LikeList
```

### 10.3 Like Product CRUD

```text
Login
  ↓
Create / Query / Update / Delete
  ↓
LikeList
```

## 11. UI Pages

- `/login`
- `/register`
- `/likelist`
- `/likelist/create`
- `/likelist/edit/{id}`
