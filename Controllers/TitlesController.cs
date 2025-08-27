using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEA.Data;
using PEA.Models;
using PEA.Models.ViewModels;

namespace PEA.Controllers;

public class TitlesController : Controller
{
    private readonly PayrollDbContext _db;
    public TitlesController(PayrollDbContext db) => _db = db;

   
    [HttpGet]
    public async Task<IActionResult> Index(int empNo)
    {
        var list = await _db.Titles
            .Where(t => t.EmpNo == empNo)
            .OrderByDescending(t => t.FromDate)
            .ToListAsync();

        ViewBag.EmpNo = empNo;
        return View(list);
    }

    [HttpGet]
    public IActionResult Create(int empNo)
    {
        return View(new Title
        {
            EmpNo = empNo,
            FromDate = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Title model)
    {

        if (string.IsNullOrWhiteSpace(model.TitleName))
            ModelState.AddModelError(nameof(model.TitleName), "El título es obligatorio.");
        if (model.ToDate != null && model.ToDate < model.FromDate)
            ModelState.AddModelError(nameof(model.ToDate), "La fecha fin no puede ser menor que la fecha inicio.");

        if (ModelState.IsValid)
        {
            var overlap = await _db.Titles
                .Where(x => x.EmpNo == model.EmpNo && x.TitleName == model.TitleName)
                .Where(x => !(x.ToDate < model.FromDate || (model.ToDate != null && model.ToDate < x.FromDate)))
                .AnyAsync();

            if (overlap)
                ModelState.AddModelError("", "Existe otro registro del mismo título que se solapa en fechas.");
        }

        if (!ModelState.IsValid)
            return View(model);

        _db.Titles.Add(model);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { empNo = model.EmpNo });
    }


    [HttpGet]
    public async Task<IActionResult> Edit(int empNo, string titleName, DateTime fromDate)
    {
        var t = await _db.Titles.FirstOrDefaultAsync(x =>
            x.EmpNo == empNo && x.TitleName == titleName && x.FromDate == fromDate);

        if (t == null) return NotFound();

        ViewBag.OriginalTitleName = titleName;
        ViewBag.OriginalFromDate = fromDate;
        return View(t);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Title model, string originalTitleName, DateTime originalFromDate)
    {
        if (string.IsNullOrWhiteSpace(model.TitleName))
            ModelState.AddModelError(nameof(model.TitleName), "El título es obligatorio.");
        if (model.ToDate != null && model.ToDate < model.FromDate)
            ModelState.AddModelError(nameof(model.ToDate), "La fecha fin no puede ser menor que la fecha inicio.");

        
        if (ModelState.IsValid)
        {
            var overlap = await _db.Titles
                .Where(x => x.EmpNo == model.EmpNo && x.TitleName == model.TitleName)
                .Where(x => !(x.ToDate < model.FromDate || (model.ToDate != null && model.ToDate < x.FromDate)))
                .Where(x => !(x.EmpNo == model.EmpNo && x.TitleName == originalTitleName && x.FromDate == originalFromDate))
                .AnyAsync();

            if (overlap)
                ModelState.AddModelError("", "Existe otro registro del mismo título que se solapa en fechas.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.OriginalTitleName = originalTitleName;
            ViewBag.OriginalFromDate = originalFromDate;
            return View(model);
        }

        var original = await _db.Titles.FirstOrDefaultAsync(x =>
            x.EmpNo == model.EmpNo && x.TitleName == originalTitleName && x.FromDate == originalFromDate);

        if (original == null) return NotFound();


        var pkChanged = model.TitleName != originalTitleName || model.FromDate != originalFromDate;
        if (pkChanged)
        {
            _db.Titles.Remove(original);
            _db.Titles.Add(model);
        }
        else
        {

            original.TitleName = model.TitleName;
            original.FromDate = model.FromDate;
            original.ToDate = model.ToDate;
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { empNo = model.EmpNo });
    }

    [HttpGet]
    public IActionResult Bulk(int empNo, int? rows)
    {
        var vm = new BulkTitlesVM { EmpNo = empNo };


        var n = Math.Max(1, rows ?? vm.Items.Count);
        vm.Items = Enumerable.Range(0, n)
            .Select(_ => new BulkTitlesVM.Row { FromDate = DateTime.Today })
            .ToList();

        return View(vm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bulk(BulkTitlesVM vm)
    {

        vm.Items = vm.Items.Where(x => !string.IsNullOrWhiteSpace(x.TitleName)).ToList();

        if (!vm.Items.Any())
            ModelState.AddModelError("", "Agrega al menos un título.");

        for (int i = 0; i < vm.Items.Count; i++)
        {
            var r = vm.Items[i];
            if (r.ToDate != null && r.ToDate < r.FromDate)
                ModelState.AddModelError($"Items[{i}].ToDate", "La fecha fin no puede ser menor que la fecha inicio.");
        }

        if (ModelState.IsValid)
        {
            foreach (var r in vm.Items)
            {
                var overlapDb = await _db.Titles
                    .Where(x => x.EmpNo == vm.EmpNo && x.TitleName == r.TitleName)
                    .Where(x => !(x.ToDate < r.FromDate || (r.ToDate != null && r.ToDate < x.FromDate)))
                    .AnyAsync();

                if (overlapDb)
                {
                    ModelState.AddModelError("", $"Ya existe un período solapado para el título \"{r.TitleName}\".");
                    break;
                }
            }
        }


        if (ModelState.IsValid)
        {
            for (int i = 0; i < vm.Items.Count; i++)
            {
                for (int j = i + 1; j < vm.Items.Count; j++)
                {
                    var a = vm.Items[i];
                    var b = vm.Items[j];
                    if (!string.Equals(a.TitleName, b.TitleName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var solapa = !((a.ToDate != null && a.ToDate < b.FromDate) ||
                                    (b.ToDate != null && b.ToDate < a.FromDate));
                    if (solapa)
                    {
                        ModelState.AddModelError("", $"Las filas {i + 1} y {j + 1} se solapan para el título \"{a.TitleName}\".");
                        i = vm.Items.Count; 
                        break;
                    }
                }
            }
        }

        if (!ModelState.IsValid)
            return View(vm);

        var entities = vm.Items.Select(x => new Title
        {
            EmpNo = vm.EmpNo,
            TitleName = x.TitleName,
            FromDate = x.FromDate,
            ToDate = x.ToDate
        }).ToList();

        _db.Titles.AddRange(entities);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { empNo = vm.EmpNo });
    }
}
