using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEA.Data;
using PEA.Models;


namespace PEA.Controllers;


[Authorize]
public class EmployeesController : Controller
{
    private readonly PayrollDbContext _db;
    public EmployeesController(PayrollDbContext db) => _db = db;


    public async Task<IActionResult> Index(string? q)
    {
        var qry = _db.Employees.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            qry = qry.Where(e => e.FirstName.Contains(q) || e.LastName.Contains(q) || e.CI.Contains(q) || e.Correo.Contains(q));
        var list = await qry.OrderBy(e => e.EmpNo).ToListAsync();
        return View(list);
    }


    public IActionResult Create() => View(new Employee { HireDate = DateTime.Today, BirthDate = new DateTime(2000, 1, 1) });


    [HttpPost]
    public async Task<IActionResult> Create(Employee e)
    {
        if (!ModelState.IsValid) return View(e);
        _db.Employees.Add(e);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Edit(int id)
    {
        var e = await _db.Employees.FindAsync(id);
        if (e == null) return NotFound();
        return View(e);
    }


    [HttpPost]
    public async Task<IActionResult> Edit(Employee e)
    {
        if (!ModelState.IsValid) return View(e);
        _db.Update(e);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Deactivate(int id)
    {
        var e = await _db.Employees.FindAsync(id);
        if (e == null) return NotFound();
        e.Activo = false;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}