using FinancialProductLikelist.Models;

namespace FinancialProductLikelist.Services;

public sealed class LikeListService
{
    private readonly ILikeListRepository _repository;

    public LikeListService(ILikeListRepository repository)
    {
        _repository = repository;
    }

    public LikeListItem Create(string userId, LikeListItemInput input)
    {
        Validate(input);
        var totals = ComputeTotals(input.Price, input.FeeRate, input.OrderQty);
        var newItem = new LikeListItem
        {
            ProductName = input.ProductName.Trim(),
            Price = input.Price,
            FeeRate = input.FeeRate,
            Account = input.Account.Trim(),
            OrderQty = input.OrderQty,
            TotalAmount = totals.totalAmount,
            TotalFee = totals.totalFee,
            Email = input.Email.Trim()
        };

        return _repository.Create(userId, newItem);
    }

    public IReadOnlyList<LikeListItem> GetByUserId(string userId)
        => _repository.GetByUserId(userId);

    public LikeListItem Update(string userId, int sn, LikeListItemInput input)
    {
        Validate(input);
        var current = _repository.GetById(userId, sn)
            ?? throw new InvalidOperationException("Like list record not found.");
        var totals = ComputeTotals(input.Price, input.FeeRate, input.OrderQty);
        var updated = current with
        {
            ProductName = input.ProductName.Trim(),
            Price = input.Price,
            FeeRate = input.FeeRate,
            Account = input.Account.Trim(),
            OrderQty = input.OrderQty,
            TotalAmount = totals.totalAmount,
            TotalFee = totals.totalFee,
            Email = input.Email.Trim()
        };

        return _repository.Update(userId, updated);
    }

    public void Delete(string userId, int sn)
    {
        _repository.Delete(userId, sn);
    }

    private static (decimal totalAmount, decimal totalFee) ComputeTotals(decimal price, decimal feeRate, int orderQty)
    {
        var totalAmount = decimal.Round(price * orderQty, 2, MidpointRounding.AwayFromZero);
        var totalFee = decimal.Round(totalAmount * feeRate, 2, MidpointRounding.AwayFromZero);
        return (totalAmount, totalFee);
    }

    private static void Validate(LikeListItemInput input)
    {
        if (string.IsNullOrWhiteSpace(input.ProductName))
        {
            throw new ArgumentException("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(input.Account))
        {
            throw new ArgumentException("Account is required.");
        }

        if (input.Price <= 0)
        {
            throw new ArgumentException("Price must be positive.");
        }

        if (input.OrderQty <= 0)
        {
            throw new ArgumentException("Order quantity must be positive.");
        }

        if (input.FeeRate < 0)
        {
            throw new ArgumentException("Fee rate cannot be negative.");
        }
    }
}
