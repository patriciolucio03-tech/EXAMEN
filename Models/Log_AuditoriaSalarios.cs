using System.ComponentModel.DataAnnotations;


namespace PEA.Models;


public class Log_AuditoriaSalarios
{
    public int Id { get; set; }
    [Required]
    public string Usuario { get; set; } = string.Empty;
    [Required]
    public DateTime FechaActualizacion { get; set; }
    [Required]
    public string DetalleCambio { get; set; } = string.Empty;
    public decimal Salario { get; set; }
    public int EmpNo { get; set; }
}