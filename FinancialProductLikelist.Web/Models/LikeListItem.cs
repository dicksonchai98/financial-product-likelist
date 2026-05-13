namespace FinancialProductLikelist.Models;

public sealed record LikeListItem
{
    public int Sn { get; init; }
    public string UserId { get; init; } = string.Empty;
    public int ProductNo { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal FeeRate { get; init; }
    public string Account { get; init; } = string.Empty;
    public int OrderQty { get; init; }
    public decimal TotalFee { get; init; }
    public decimal TotalAmount { get; init; }
    public string Email { get; init; } = string.Empty;
}
