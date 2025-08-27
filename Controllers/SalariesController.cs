using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEA.Data;
using PEA.Models;

namespace PEA.Controllers;


public class SalariesController : Controller
{
    private readonly PayrollDbContext _db;
    public SalariesController(PayrollDbContext db) => _db = db;


    [HttpGet]
    public async Task<IActionResult> Index(int empNo)
    {
        var list = await _db.Salaries
            .Where(s => s.EmpNo == empNo)
            .OrderByDescending(s => s.FromDate)
            .ToListAsync();

        ViewBag.EmpNo = empNo;
        return View(list);
    }

    
    [HttpGet]
    public IActionResult Create(int empNo)
    {
        return View(new Salary
        {
            EmpNo = empNo,
            FromDate = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Salary model, string? motivoAumento)
    {

        if (model.ToDate != null && model.ToDate < model.FromDate)
            ModelState.AddModelError(nameof(model.ToDate), "La fecha fin no puede ser menor que la fecha inicio.");


        var haySolape = await _db.Salaries
            .Where(x => x.EmpNo == model.EmpNo)
            .Where(x => !(x.ToDate < model.FromDate || (model.ToDate != null && model.ToDate < x.FromDate)))
            .AnyAsync();
        if (haySolape)
            ModelState.AddModelError("", "Existe un salario que se solapa en fechas.");

        
        if (ModelState.IsValid)
        {
            var anterior = await _db.Salaries
                .Where(x => x.EmpNo == model.EmpNo && x.FromDate <= model.FromDate)
                .Where(x => x.ToDate == null || x.ToDate >= model.FromDate)
                .OrderByDescending(x => x.FromDate)
                .FirstOrDefaultAsync();

            var esAumento = anterior != null && model.Amount > anterior.Amount;
            if (esAumento && string.IsNullOrWhiteSpace(motivoAumento))
                ModelState.AddModelError("", "Debe indicar el motivo del aumento.");
        }

        if (!ModelState.IsValid)
            return View(model);


        if (!string.IsNullOrWhiteSpace(motivoAumento))
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sys.sp_set_session_context @key=N'MotivoAumento', @value={motivoAumento};");

        _db.Salaries.Add(model);
        await _db.SaveChangesAsync();


        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sys.sp_set_session_context @key=N'MotivoAumento', @value={null as string};");

        return RedirectToAction(nameof(Index), new { empNo = model.EmpNo });
    }

   
    [HttpGet]
    public async Task<IActionResult> Edit(int empNo, DateTime fromDate)
    {
        var s = await _db.Salaries.FirstOrDefaultAsync(x => x.EmpNo == empNo && x.FromDate == fromDate);
        if (s == null) return NotFound();

        ViewBag.OriginalFromDate = fromDate;
        return View(s);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Salary model, DateTime originalFromDate, string? motivoAumento)
    {
        if (model.ToDate != null && model.ToDate < model.FromDate)
            ModelState.AddModelError(nameof(model.ToDate), "La fecha fin no puede ser menor que la fecha inicio.");


        var haySolape = await _db.Salaries
            .Where(x => x.EmpNo == model.EmpNo && x.FromDate != originalFromDate)
            .Where(x => !(x.ToDate < model.FromDate || (model.ToDate != null && model.ToDate < x.FromDate)))
            .AnyAsync();
        if (haySolape)
            ModelState.AddModelError("", "Existe un salario que se solapa en fechas.");

        var original = await _db.Salaries.FirstOrDefaultAsync(x => x.EmpNo == model.EmpNo && x.FromDate == originalFromDate);
        if (original == null)
        {
            ModelState.AddModelError("", "No se encontró el salario original.");
            ViewBag.OriginalFromDate = originalFromDate;
            return View(model);
        }

      
        if (ModelState.IsValid)
        {
            var esAumento = model.Amount > original.Amount;
            if (esAumento && string.IsNullOrWhiteSpace(motivoAumento))
                ModelState.AddModelError("", "Debe indicar el motivo del aumento.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.OriginalFromDate = originalFromDate;
            return View(model);
        }

 
        if (!string.IsNullOrWhiteSpace(motivoAumento))
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC sys.sp_set_session_context @key=N'MotivoAumento', @value={motivoAumento};");


        if (model.FromDate != originalFromDate)
        {
            _db.Salaries.Remove(original);
            _db.Salaries.Add(model);
        }
        else
        {
            original.Amount = model.Amount;
            original.ToDate = model.ToDate;
        }

        await _db.SaveChangesAsync();

       
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC sys.sp_set_session_context @key=N'MotivoAumento', @value={null as string};");

        return RedirectToAction(nameof(Index), new { empNo = model.EmpNo });
    }
}
