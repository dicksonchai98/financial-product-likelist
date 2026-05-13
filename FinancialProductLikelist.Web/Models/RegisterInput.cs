namespace FinancialProductLikelist.Models;

public sealed record RegisterInput(
    string UserName,
    string Email,
    string Password);
