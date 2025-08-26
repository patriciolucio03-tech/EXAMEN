using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEA.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace PEA.Controllers;


[Authorize]
public class ReportsController : Controller
{
    private readonly PayrollDbContext _db;
    public ReportsController(PayrollDbContext db) => _db = db;


    public IActionResult Index() => View();


    // Vista web
    [HttpGet]
    public async Task<IActionResult> PayrollByDept(string deptNo, DateTime? fechaCorte)
    {
        var date = fechaCorte ?? DateTime.Today;
        var data = await (from de in _db.DeptEmps
                          join e in _db.Employees on de.EmpNo equals e.EmpNo
                          join s in _db.Salaries on e.EmpNo equals s.EmpNo
                          where de.DeptNo == deptNo
                          && de.FromDate <= date && (de.ToDate == null || de.ToDate >= date)
                          && s.FromDate <= date && (s.ToDate == null || s.ToDate >= date)
                          select new { e.EmpNo, e.FirstName, e.LastName, s.Salario })
        .OrderBy(x => x.LastName).ToListAsync();
        ViewBag.DeptNo = deptNo; ViewBag.Fecha = date.ToShortDateString();
        return View(data);
    }


    // Excel
    [HttpGet]
    public async Task<FileResult> PayrollByDeptExcel(string deptNo, DateTime? fechaCorte)
    {
        var date = fechaCorte ?? DateTime.Today;
        var data = await (from de in _db.DeptEmps
                          join e in _db.Employees on de.EmpNo equals e.EmpNo
                          join s in _db.Salaries on e.EmpNo equals s.EmpNo
                          where de.DeptNo == deptNo
                          && de.FromDate <= date && (de.ToDate == null || de.ToDate >= date)
                          && s.FromDate <= date && (s.ToDate == null || s.ToDate >= date)
                          select new { e.EmpNo, e.FirstName, e.LastName, s.Salario })
        .OrderBy(x => x.LastName).ToListAsync();


        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Nomina");
        ws.Cell(1, 1).Value = "EmpNo"; ws.Cell(1, 2).Value = "Nombre"; ws.Cell(1, 3).Value = "Apellido"; ws.Cell(1, 4).Value = "Salario";
        int r = 2;
        foreach (var x in data) { ws.Cell(r, 1).Value = x.EmpNo; ws.Cell(r, 2).Value = x.FirstName; ws.Cell(r, 3).Value = x.LastName; ws.Cell(r, 4).Value = x.Salario; r++; }
        using var ms = new MemoryStream(); wb.SaveAs(ms);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Nomina_{deptNo}.xlsx");
    }
    // PDF (QuestPDF)
    [HttpGet]
    public async Task<FileResult> PayrollByDeptPdf(string deptNo, DateTime? fechaCorte)
    {
        var date = fechaCorte ?? DateTime.Today;
        var data = await (from de in _db.DeptEmps
                          join e in _db.Employees on de.EmpNo equals e.EmpNo
                          join s in _db.Salaries on e.EmpNo equals s.EmpNo
                          where de.DeptNo == deptNo
                          && de.FromDate <= date && (de.ToDate == null || de.ToDate >= date)
                          && s.FromDate <= date && (s.ToDate == null || s.ToDate >= date)
                          select new { e.EmpNo, e.FirstName, e.LastName, s.Salario })
        .OrderBy(x => x.LastName).ToListAsync();


        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text($"Nómina {deptNo} · {date:yyyy-MM-dd}").SemiBold().FontSize(18);
                page.Content().Table(t =>
                {
                    t.ColumnsDefinition(c => { c.ConstantColumn(60); c.RelativeColumn(); c.RelativeColumn(); c.ConstantColumn(90); });
                    t.Header(h => {
                        h.Cell().Background("#eeeeee").Padding(5).Text("EmpNo");
                        h.Cell().Background("#eeeeee").Padding(5).Text("Nombre");
                        h.Cell().Background("#eeeeee").Padding(5).Text("Apellido");
                        h.Cell().Background("#eeeeee").Padding(5).Text("Salario");
                    });
                    foreach (var x in data)
                    {
                        t.Cell().Padding(3).Text(x.EmpNo.ToString());
                        t.Cell().Padding(3).Text(x.FirstName);
                        t.Cell().Padding(3).Text(x.LastName);
                        t.Cell().Padding(3).Text(x.Salario.ToString("N2"));
                    }
                });
                page.Footer().AlignRight().Text(txt => {
                    txt.Span("Generado "); txt.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                });
            });
        }).GeneratePdf();


        return File(bytes, "application/pdf", $"Nomina_{deptNo}.pdf");
    }
}