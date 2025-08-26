using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEA.Data;
using PEA.Models;


namespace PEA.Controllers;


[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly PayrollDbContext _db;
    public UsersController(PayrollDbContext db) => _db = db;


    public async Task<IActionResult> Index() => View(await _db.Users.OrderBy(u => u.Usuario).ToListAsync());


    public IActionResult Create() => View(new User());


    [HttpPost]
    public async Task<IActionResult> Create(User u, string password)
    {
        if (string.IsNullOrWhiteSpace(password)) ModelState.AddModelError("", "La contraseña es obligatoria");
        if (!ModelState.IsValid) return View(u);
        u.Clave = BCrypt.Net.BCrypt.HashPassword(password);
        _db.Users.Add(u);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Edit(int id)
    {
        var u = await _db.Users.FindAsync(id);
        return u == null ? NotFound() : View(u);
    }


    [HttpPost]
    public async Task<IActionResult> Edit(User u, string? newPassword)
    {
        if (!ModelState.IsValid) return View(u);
        var dbU = await _db.Users.FindAsync(u.UserId);
        if (dbU == null) return NotFound();
        dbU.Usuario = u.Usuario;
        dbU.Rol = u.Rol;
        dbU.EmpNo = u.EmpNo;
        if (!string.IsNullOrWhiteSpace(newPassword))
            dbU.Clave = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}