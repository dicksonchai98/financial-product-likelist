using System.Security.Claims;
using FinancialProductLikelist.Models;
using FinancialProductLikelist.Services;
using FinancialProductLikelist.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinancialProductLikelist.Controllers;

[Authorize]
public sealed class LikeListController : Controller
{
    private readonly LikeListService _service;

    public LikeListController(LikeListService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (!TryGetUserId(out var userId))
        {
            return Challenge();
        }

        var items = _service.GetByUserId(userId!);
        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!TryGetUserAccount(out var account) || !TryGetUserEmail(out var email))
        {
            return Challenge();
        }

        var model = new LikeListFormViewModel
        {
            Account = account!,
            Email = email!
        };
        PopulateProducts(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(LikeListFormViewModel model)
    {
        if (!TryGetUserAccount(out var account) || !TryGetUserEmail(out var email) || !TryGetUserId(out var userId))
        {
            return Challenge();
        }

        model.Account = account!;
        model.Email = email!;
        ApplySelectedProduct(model);
        RevalidateServerAssignedFields(model);
        if (!ModelState.IsValid)
        {
            PopulateProducts(model);
            return View(model);
        }

        _service.Create(userId!, ToInput(model));
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        if (!TryGetUserId(out var userId) || !TryGetUserAccount(out var account) || !TryGetUserEmail(out var email))
        {
            return Challenge();
        }

        var item = _service.GetByUserId(userId!).FirstOrDefault(x => x.Sn == id);
        if (item is null)
        {
            return NotFound();
        }

        var model = new LikeListFormViewModel
        {
            Sn = item.Sn,
            ProductNo = item.ProductNo,
            ProductName = item.ProductName,
            Price = item.Price,
            FeeRate = item.FeeRate,
            Account = account!,
            OrderQty = item.OrderQty,
            Email = email!
        };
        PopulateProducts(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, LikeListFormViewModel model)
    {
        if (!TryGetUserAccount(out var account) || !TryGetUserEmail(out var email) || !TryGetUserId(out var userId))
        {
            return Challenge();
        }

        model.Account = account!;
        model.Email = email!;
        ApplySelectedProduct(model);
        RevalidateServerAssignedFields(model);
        if (!ModelState.IsValid)
        {
            model.Sn = id;
            PopulateProducts(model);
            return View(model);
        }

        try
        {
            _service.Update(userId!, id, ToInput(model));
        }
        catch (InvalidOperationException ex) when (ex.Message == "Like list record not found.")
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Challenge();
        }

        try
        {
            _service.Delete(userId!, id);
            TempData["StatusMessage"] = "Item deleted.";
        }
        catch (InvalidOperationException ex) when (ex.Message == "Like list record not found.")
        {
            TempData["ErrorMessage"] = "Item not found or already removed.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool TryGetUserId(out string? userId)
        => TryGetClaim(ClaimTypes.NameIdentifier, out userId);

    private bool TryGetUserAccount(out string? account)
        => TryGetClaim("account", out account);

    private bool TryGetUserEmail(out string? email)
        => TryGetClaim(ClaimTypes.Email, out email);

    private bool TryGetClaim(string claimType, out string? claimValue)
    {
        claimValue = User.FindFirstValue(claimType);
        return !string.IsNullOrWhiteSpace(claimValue);
    }

    private static LikeListItemInput ToInput(LikeListFormViewModel model)
    {
        var effectiveOrderQty = model.OrderQty > 0 ? model.OrderQty : (model.OrderName ?? 0);
        return new LikeListItemInput(
            model.ProductName,
            model.Price,
            model.FeeRate,
            model.Account,
            effectiveOrderQty,
            model.Email);
    }

    private void PopulateProducts(LikeListFormViewModel model)
    {
        var products = _service.GetProducts();
        model.Products = products.Select(x => new SelectListItem
        {
            Value = x.No.ToString(),
            Text = $"{x.ProductName} (Price: {x.Price}, Fee: {x.FeeRate})",
            Selected = x.No == model.ProductNo
        }).ToList();
        model.ProductCatalog = products
            .Select(x => new LikeListFormViewModel.ProductOption(x.No, x.ProductName, x.Price, x.FeeRate))
            .ToList();
    }

    private void ApplySelectedProduct(LikeListFormViewModel model)
    {
        if (model.ProductNo <= 0)
        {
            ModelState.AddModelError(nameof(model.ProductNo), "Please select a product.");
            return;
        }

        var product = _service.GetProductByNo(model.ProductNo);
        if (product is null)
        {
            ModelState.AddModelError(nameof(model.ProductNo), "Selected product not found.");
            return;
        }

        model.ProductName = product.ProductName;
        model.Price = product.Price;
        model.FeeRate = product.FeeRate;
    }

    private void RevalidateServerAssignedFields(LikeListFormViewModel model)
    {
        ModelState.Remove(nameof(model.ProductName));
        ModelState.Remove(nameof(model.Price));
        ModelState.Remove(nameof(model.FeeRate));
        ModelState.Remove(nameof(model.Account));
        ModelState.Remove(nameof(model.Email));
        TryValidateModel(model);
    }
}
