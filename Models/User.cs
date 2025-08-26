using System.ComponentModel.DataAnnotations;


namespace PEA.Models;


public class User
{
    public int UserId { get; set; }
    public int? EmpNo { get; set; }


    [Required, StringLength(40)]
    public string Usuario { get; set; } = string.Empty;


    [Required]
    public string Clave { get; set; } = string.Empty;


    [Required, StringLength(20)]
    public string Rol { get; set; } = "RRHH"; // Admin | RRHH
}