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
        var userId = GetUserId();
        var items = _service.GetByUserId(userId);
        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var model = new LikeListFormViewModel
        {
            Account = GetUserAccount(),
            Email = GetUserEmail()
        };
        PopulateProducts(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(LikeListFormViewModel model)
    {
        model.Account = GetUserAccount();
        model.Email = GetUserEmail();
        ApplySelectedProduct(model);
        RevalidateServerAssignedFields(model);
        if (!ModelState.IsValid)
        {
            PopulateProducts(model);
            return View(model);
        }

        var userId = GetUserId();
        _service.Create(userId, ToInput(model));
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var userId = GetUserId();
        var item = _service.GetByUserId(userId).FirstOrDefault(x => x.Sn == id);
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
            Account = GetUserAccount(),
            OrderQty = item.OrderQty,
            Email = GetUserEmail()
        };
        PopulateProducts(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, LikeListFormViewModel model)
    {
        model.Account = GetUserAccount();
        model.Email = GetUserEmail();
        ApplySelectedProduct(model);
        RevalidateServerAssignedFields(model);
        if (!ModelState.IsValid)
        {
            model.Sn = id;
            PopulateProducts(model);
            return View(model);
        }

        var userId = GetUserId();
        _service.Update(userId, id, ToInput(model));
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var userId = GetUserId();
        _service.Delete(userId, id);
        return RedirectToAction(nameof(Index));
    }

    private string GetUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "A1236456789";

    private string GetUserAccount()
        => User.FindFirstValue("account") ?? "1111999666";

    private string GetUserEmail()
        => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

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
