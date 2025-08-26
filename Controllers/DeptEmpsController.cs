using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEA.Data;
using PEA.Models;
using PEA.Services;


namespace PEA.Controllers;


[Authorize]
public class DeptEmpsController : Controller
{
    private readonly PayrollDbContext _db;
    private readonly DomainRules _rules;
    public DeptEmpsController(PayrollDbContext db, DomainRules rules) { _db = db; _rules = rules; }


    public async Task<IActionResult> Index(int empNo)
    {
        ViewBag.EmpNo = empNo;
        var list = await _db.DeptEmps.Include(d => d.Department)
        .Where(d => d.EmpNo == empNo).OrderByDescending(d => d.FromDate).ToListAsync();
        return View(list);
    }


    public IActionResult Create(int empNo)
    {
        ViewBag.Departments = _db.Departments.OrderBy(d => d.DeptName).ToList();
        return View(new DeptEmp { EmpNo = empNo, FromDate = DateTime.Today });
    }


    [HttpPost]
    public async Task<IActionResult> Create(DeptEmp de)
    {
        if (de.ToDate != null && de.ToDate < de.FromDate)
            ModelState.AddModelError("", "La fecha fin no puede ser menor que la de inicio");
        if (!ModelState.IsValid) { ViewBag.Departments = _db.Departments.ToList(); return View(de); }
        if (await _rules.DeptEmpOverlapsAsync(de))
        {
            ModelState.AddModelError("", "Existe una asignación que se solapa en fechas");
            ViewBag.Departments = _db.Departments.ToList();
            return View(de);
        }
        _db.DeptEmps.Add(de);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { empNo = de.EmpNo });
    }
}