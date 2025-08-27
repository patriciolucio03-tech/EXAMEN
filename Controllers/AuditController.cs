using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEA.Data;
using PEA.Models;
using PEA.Models.ViewModels;

namespace PEA.Controllers;

[Authorize(Roles = "Admin,RRHH")]
public class AuditController : Controller
{
    private readonly PayrollDbContext _db;
    public AuditController(PayrollDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, string? usuario, int? empNo, int page = 1, int pageSize = 25)
    {
        var q = _db.Log_AuditoriaSalarios.AsQueryable();

        if (desde.HasValue)
            q = q.Where(x => x.FechaActualizacion >= desde.Value.Date);

        if (hasta.HasValue)
        {
            var fin = hasta.Value.Date.AddDays(1); // inclusivo hasta 23:59:59
            q = q.Where(x => x.FechaActualizacion < fin);
        }

        if (!string.IsNullOrWhiteSpace(usuario))
            q = q.Where(x => x.Usuario.Contains(usuario));

        if (empNo.HasValue)
            q = q.Where(x => x.EmpNo == empNo.Value);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(x => x.FechaActualizacion)
                           .Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .ToListAsync();

        var vm = new AuditLogIndexVM
        {
            Items = items,
            Desde = desde,
            Hasta = hasta,
            Usuario = usuario,
            EmpNo = empNo,
            Page = page,
            PageSize = pageSize,
            Total = total
        };

        return View(vm);
    }
}
