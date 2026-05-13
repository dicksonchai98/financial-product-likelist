using System.Security.Claims;
using FinancialProductLikelist.Models;
using FinancialProductLikelist.Services;
using FinancialProductLikelist.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        return View(new LikeListFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(LikeListFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
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

        return View(new LikeListFormViewModel
        {
            Sn = item.Sn,
            ProductName = item.ProductName,
            Price = item.Price,
            FeeRate = item.FeeRate,
            Account = item.Account,
            OrderQty = item.OrderQty,
            Email = item.Email
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, LikeListFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Sn = id;
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
}
