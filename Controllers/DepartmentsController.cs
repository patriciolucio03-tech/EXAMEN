using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEA.Data;
using PEA.Models;


namespace PEA.Controllers;


[Authorize]
public class DepartmentsController : Controller
{
    private readonly PayrollDbContext _db;
    public DepartmentsController(PayrollDbContext db) => _db = db;


    public async Task<IActionResult> Index() => View(await _db.Departments.OrderBy(d => d.DeptNo).ToListAsync());


    public IActionResult Create() => View(new Department());


    [HttpPost]
    public async Task<IActionResult> Create(Department d)
    {
        if (!ModelState.IsValid) return View(d);
        _db.Departments.Add(d);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Edit(string id)
    {
        var d = await _db.Departments.FindAsync(id);
        return d == null ? NotFound() : View(d);
    }


    [HttpPost]
    public async Task<IActionResult> Edit(Department d)
    {
        if (!ModelState.IsValid) return View(d);
        _db.Update(d);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}