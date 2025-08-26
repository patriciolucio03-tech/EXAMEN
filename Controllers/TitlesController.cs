using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEA.Data;
using PEA.Models;
using PEA.Services;


namespace PEA.Controllers;


[Authorize]
public class TitlesController : Controller
{
    private readonly PayrollDbContext _db;
    public TitlesController(PayrollDbContext db) => _db = db;


    public async Task<IActionResult> Index(int empNo)
    {
        ViewBag.EmpNo = empNo;
        var list = await _db.Titles.Where(t => t.EmpNo == empNo).OrderByDescending(t => t.FromDate).ToListAsync();
        return View(list);
    }


    public IActionResult Create(int empNo) => View(new Title { EmpNo = empNo, FromDate = DateTime.Today });


    [HttpPost]
    public async Task<IActionResult> Create(Title t)
    {
        if (t.ToDate != null && t.ToDate < t.FromDate)
            ModelState.AddModelError("", "La fecha fin no puede ser menor que la de inicio");
        if (!ModelState.IsValid) return View(t);
        // Nota: si quieres evitar solapes por TitleName, añade una consulta similar a Salary/DeptEmp
        _db.Titles.Add(t);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { empNo = t.EmpNo });
    }
}