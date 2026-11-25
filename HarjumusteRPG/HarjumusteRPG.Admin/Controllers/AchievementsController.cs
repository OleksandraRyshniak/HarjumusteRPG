using HarjumusteRPG.Admin.Models;
using HarjumusteRPG.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarjumusteRPG.Admin.Controllers;

[Authorize(Roles = "Admin")]
public class AchievementsController : Controller
{
    private readonly AdminApiClient _api;

    public AchievementsController(AdminApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAchievementsAsync();

        ViewBag.Error = TempData["Error"] ?? result.Error;
        ViewBag.Message = TempData["Message"];

        return View(result.Value ?? new List<Achievement>());
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "New achievement";
        return View("Form", new AchievementEdit());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AchievementEdit model)
    {
        ViewData["Title"] = "New achievement";
        if (!ModelState.IsValid)
            return View("Form", model);

        var result = await _api.CreateAchievementAsync(model);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not create the achievement.");
            return View("Form", model);
        }

        TempData["Message"] = "Achievement created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _api.GetAchievementsAsync();
        var achievement = result.Value?.FirstOrDefault(a => a.Id == id);
        if (achievement is null)
            return NotFound();

        ViewData["Title"] = "Edit achievement";
        return View("Form", new AchievementEdit
        {
            Name = achievement.Name,
            Description = achievement.Description,
            Type = achievement.Type,
            RequiredValue = achievement.RequiredValue
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AchievementEdit model)
    {
        ViewData["Title"] = "Edit achievement";
        if (!ModelState.IsValid)
            return View("Form", model);

        var result = await _api.UpdateAchievementAsync(id, model);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not update the achievement.");
            return View("Form", model);
        }

        TempData["Message"] = "Achievement updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _api.DeleteAchievementAsync(id);
        if (result.Ok)
            TempData["Message"] = "Achievement deleted.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction(nameof(Index));
    }
}