using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinancialProductLikelist.ViewModels;

public sealed class LikeListFormViewModel : IValidatableObject
{
    public sealed record ProductOption(int No, string ProductName, decimal Price, decimal FeeRate);

    public int Sn { get; set; }
    public int ProductNo { get; set; }
    public List<SelectListItem> Products { get; set; } = [];
    public List<ProductOption> ProductCatalog { get; set; } = [];

    [Required]
    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, 1)]
    [Display(Name = "Fee Rate")]
    public decimal FeeRate { get; set; }

    [Required]
    public string Account { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Display(Name = "Order Quantity")]
    public int OrderQty { get; set; }

    // Backward-compatible alias for historical naming in requirement drafts.
    public int? OrderName { get; set; }

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var effectiveOrderQty = OrderQty > 0 ? OrderQty : (OrderName ?? 0);
        if (effectiveOrderQty <= 0)
        {
            yield return new ValidationResult("Order quantity must be positive.", [nameof(OrderQty)]);
        }
    }
}
