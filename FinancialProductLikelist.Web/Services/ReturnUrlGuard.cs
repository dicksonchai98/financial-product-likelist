namespace FinancialProductLikelist.Services;

public static class ReturnUrlGuard
{
    public static bool IsSafeLocal(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl)
               && returnUrl.StartsWith('/')
               && !returnUrl.StartsWith("//")
               && !returnUrl.StartsWith("/\\");
    }
}
