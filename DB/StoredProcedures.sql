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
