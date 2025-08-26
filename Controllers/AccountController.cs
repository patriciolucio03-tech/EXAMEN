using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PEA.Data;
using Microsoft.EntityFrameworkCore;


namespace PEA.Controllers;


public class AccountController : Controller
{
    private readonly PayrollDbContext _db;
    public AccountController(PayrollDbContext db) => _db = db;


    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(model: returnUrl);


    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Usuario == username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Clave))
        {
            ModelState.AddModelError("", "Usuario o contraseña inválidos");
            return View(model: returnUrl);
        }


        var claims = new List<Claim>
{
new Claim(ClaimTypes.Name, user.Usuario),
new Claim(ClaimTypes.Role, user.Rol)
};
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));


        return !string.IsNullOrEmpty(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");
    }


    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login");
    }


    public IActionResult Denied() => View();
}