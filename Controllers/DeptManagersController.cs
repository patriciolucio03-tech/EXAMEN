using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEA.Data;
using PEA.Models;
using PEA.Services;


namespace PEA.Controllers;


[Authorize(Roles = "Admin,RRHH")]
public class DeptManagersController : Controller
{
    private readonly PayrollDbContext _db;
    private readonly DomainRules _rules;
    public DeptManagersController(PayrollDbContext db, DomainRules rules) { _db = db; _rules = rules; }


    public async Task<IActionResult> Index(string deptNo)
    {
        ViewBag.DeptNo = deptNo;
        var list = await _db.DeptManagers.Include(m => m.Employee)
        .Where(m => m.DeptNo == deptNo).OrderByDescending(m => m.FromDate).ToListAsync();
        return View(list);
    }


    public IActionResult Create(string deptNo)
    {
        ViewBag.Employees = _db.Employees.OrderBy(e => e.LastName).ToList();
        return View(new DeptManager { DeptNo = deptNo, FromDate = DateTime.Today });
    }


    [HttpPost]
    public async Task<IActionResult> Create(DeptManager dm)
    {
        if (dm.ToDate != null && dm.ToDate < dm.FromDate)
            ModelState.AddModelError("", "La fecha fin no puede ser menor que la de inicio");
        if (!ModelState.IsValid) { ViewBag.Employees = _db.Employees.ToList(); return View(dm); }
        if (await _rules.ManagerOverlapsAsync(dm))
        {
            ModelState.AddModelError("", "Ya existe un gerente activo en ese periodo");
            ViewBag.Employees = _db.Employees.ToList();
            return View(dm);
        }
        _db.DeptManagers.Add(dm);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { deptNo = dm.DeptNo });
    }
}