using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEA.Data;
using PEA.Models;
using PEA.Services;


namespace PEA.Controllers;


[Authorize(Roles = "Admin,RRHH")]
public class SalariesController : Controller
{
    private readonly PayrollDbContext _db;
    private readonly DomainRules _rules;
    public SalariesController(PayrollDbContext db, DomainRules rules) { _db = db; _rules = rules; }


    public async Task<IActionResult> Index(int empNo)
    {
        var list = await _db.Salaries.Where(s => s.EmpNo == empNo).OrderByDescending(s => s.FromDate).ToListAsync();
        ViewBag.EmpNo = empNo;
        return View(list);
    }


    public IActionResult Create(int empNo) => View(new Salary { EmpNo = empNo, FromDate = DateTime.Today });


    [HttpPost]
    public async Task<IActionResult> Create(Salary s)
    {
        if (s.ToDate != null && s.ToDate < s.FromDate)
            ModelState.AddModelError("", "La fecha fin no puede ser menor que la de inicio");


        if (!ModelState.IsValid) return View(s);


        if (await _rules.SalaryOverlapsAsync(s))
        {
            ModelState.AddModelError("", "Existe un salario activo que se solapa en fechas");
            return View(s);
        }


        _db.Salaries.Add(s);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { empNo = s.EmpNo });
    }
}