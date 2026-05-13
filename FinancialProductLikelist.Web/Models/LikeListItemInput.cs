namespace FinancialProductLikelist.Models;

public sealed record LikeListItemInput(
    string ProductName,
    decimal Price,
    decimal FeeRate,
    string Account,
    int OrderQty,
    string Email = "");
