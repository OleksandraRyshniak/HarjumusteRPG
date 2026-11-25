using System.Security.Claims;
using HarjumusteRPG.Admin.Models;
using HarjumusteRPG.Admin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarjumusteRPG.Admin.Controllers;

public class AccountController : Controller
{
    private readonly AdminApiClient _api;

    public AccountController(AdminApiClient api)
    {
        _api = api;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _api.LoginAsync(model.Email, model.Password);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Login failed.");
            return View(model);
        }

        var auth = result.Value!;
        if (auth.User.Role != "Admin")
        {
            ModelState.AddModelError(string.Empty, "Only administrators can use this site.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, auth.User.Id.ToString()),
            new(ClaimTypes.Name, auth.User.Username),
            new(ClaimTypes.Role, auth.User.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var properties = new AuthenticationProperties();
        properties.StoreTokens(new[] { new AuthenticationToken { Name = "access_token", Value = auth.Token } });

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), properties);

        return RedirectToAction("Index", "Users");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}