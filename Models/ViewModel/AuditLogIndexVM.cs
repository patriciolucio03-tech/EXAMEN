namespace PEA.Models.ViewModels;

public class AuditLogIndexVM
{
    public IEnumerable<Log_AuditoriaSalarios> Items { get; set; } = Enumerable.Empty<Log_AuditoriaSalarios>();

    // Filtros
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public string? Usuario { get; set; }
    public int? EmpNo { get; set; }

    // Paginación
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int Total { get; set; }
}
