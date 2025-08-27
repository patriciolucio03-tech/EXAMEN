namespace PEA.Models.ViewModels;

public class PayrollByDeptRow
{
    public int EmpNo { get; set; }
    public string FullName { get; set; } = "";
    public string DeptName { get; set; } = "";
    public decimal Amount { get; set; }        // ← viene de la col. Salario
    public DateTime FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class PayrollByDeptVM
{
    public DateTime? Fecha { get; set; }       // filtro
    public int? DeptNo { get; set; }           // filtro
    public List<PayrollByDeptRow> Rows { get; set; } = new();
}
