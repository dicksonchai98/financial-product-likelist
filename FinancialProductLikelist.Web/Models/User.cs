namespace FinancialProductLikelist.Models;

public sealed record User
{
    public string UserID { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Account { get; init; } = string.Empty;
}
