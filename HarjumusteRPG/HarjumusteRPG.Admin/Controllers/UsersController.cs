using HarjumusteRPG.Admin.Models;
using HarjumusteRPG.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarjumusteRPG.Admin.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly AdminApiClient _api;

    public UsersController(AdminApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(string? search, string? role, int page = 1)
    {
        var result = await _api.GetUsersAsync(search, role, page < 1 ? 1 : page);

        ViewBag.Search = search;
        ViewBag.Role = role;
        ViewBag.Error = TempData["Error"] ?? result.Error;
        ViewBag.Message = TempData["Message"];

        return View(result.Value ?? new PagedResult<AdminUser>());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? search, string? role, int page = 1)
    {
        var result = await _api.DeleteUserAsync(id);
        if (result.Ok)
            TempData["Message"] = "User deleted.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index), new { search, role, page });
    }
}