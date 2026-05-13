CREATE OR ALTER PROCEDURE SP_Product_Upsert
    @ProductName NVARCHAR(200),
    @Price DECIMAL(18,2),
    @FeeRate DECIMAL(9,4)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ProductNo INT;
    SELECT TOP 1 @ProductNo = No
    FROM Product
    WHERE ProductName = @ProductName
      AND Price = @Price
      AND FeeRate = @FeeRate;

    IF @ProductNo IS NULL
    BEGIN
        INSERT INTO Product (ProductName, Price, FeeRate)
        VALUES (@ProductName, @Price, @FeeRate);
        SET @ProductNo = SCOPE_IDENTITY();
    END

    SELECT @ProductNo;
END;
GO

CREATE OR ALTER PROCEDURE SP_User_Register
    @UserID NVARCHAR(20),
    @UserName NVARCHAR(100),
    @Email NVARCHAR(256),
    @Account NVARCHAR(20),
    @PasswordHash NVARCHAR(256),
    @PasswordSalt NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM [User] WHERE UserID = @UserID)
    BEGIN
        RAISERROR('UserID already exists', 16, 1);
        RETURN;
    END

    INSERT INTO [User] (UserID, UserName, Email, Account, PasswordHash, PasswordSalt)
    VALUES (@UserID, @UserName, @Email, @Account, @PasswordHash, @PasswordSalt);
END;
GO

CREATE OR ALTER PROCEDURE SP_User_ExistsByUserId
    @UserID NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS (SELECT 1 FROM [User] WHERE UserID = @UserID) THEN 1 ELSE 0 END;
END;
GO

CREATE OR ALTER PROCEDURE SP_User_ExistsByEmail
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS (SELECT 1 FROM [User] WHERE Email = @Email) THEN 1 ELSE 0 END;
END;
GO

CREATE OR ALTER PROCEDURE SP_User_Login
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 UserID, UserName, Email, Account, PasswordHash, PasswordSalt
    FROM [User]
    WHERE Email = @Email;
END;
GO

CREATE OR ALTER PROCEDURE SP_LikeList_Create
    @UserID NVARCHAR(20),
    @ProductNo INT,
    @OrderQty INT,
    @Account NVARCHAR(20),
    @TotalFee DECIMAL(18,2),
    @TotalAmount DECIMAL(18,2),
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO LikeList (UserID, ProductNo, OrderQty, Account, TotalFee, TotalAmount, Email)
    VALUES (@UserID, @ProductNo, @OrderQty, @Account, @TotalFee, @TotalAmount, @Email);

    SELECT SCOPE_IDENTITY();
END;
GO

CREATE OR ALTER PROCEDURE SP_LikeList_GetList
    @UserID NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        l.SN,
        l.UserID,
        l.ProductNo,
        p.ProductName,
        p.Price,
        p.FeeRate,
        l.OrderQty,
        l.Account,
        l.TotalFee,
        l.TotalAmount,
        l.Email
    FROM LikeList l
    INNER JOIN Product p ON p.No = l.ProductNo
    WHERE l.UserID = @UserID
    ORDER BY l.SN DESC;
END;
GO

CREATE OR ALTER PROCEDURE SP_LikeList_Update
    @SN INT,
    @UserID NVARCHAR(20),
    @ProductNo INT,
    @OrderQty INT,
    @Account NVARCHAR(20),
    @TotalFee DECIMAL(18,2),
    @TotalAmount DECIMAL(18,2),
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE LikeList
    SET
        ProductNo = @ProductNo,
        OrderQty = @OrderQty,
        Account = @Account,
        TotalFee = @TotalFee,
        TotalAmount = @TotalAmount,
        Email = @Email
    WHERE SN = @SN
      AND UserID = @UserID;
END;
GO

CREATE OR ALTER PROCEDURE SP_LikeList_Delete
    @SN INT,
    @UserID NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM LikeList
    WHERE SN = @SN
      AND UserID = @UserID;
END;
GO
