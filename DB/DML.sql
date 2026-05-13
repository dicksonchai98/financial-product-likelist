INSERT INTO [User] (UserID, UserName, Email, Account)
VALUES
('A1236456789', N'王小明', 'test@email.com', '1111999666'),
('B1234567890', N'陳小華', 'demo@email.com', '2222888899');

INSERT INTO Product (ProductName, Price, FeeRate)
VALUES
(N'台股ETF A', 100.00, 0.0100),
(N'美股ETF B', 250.00, 0.0050);

INSERT INTO LikeList (UserID, ProductNo, OrderQty, Account, TotalFee, TotalAmount, Email)
VALUES
('A1236456789', 1, 10, '1111999666', 10.00, 1000.00, 'test@email.com'),
('B1234567890', 2, 2, '2222888899', 2.50, 500.00, 'demo@email.com');
