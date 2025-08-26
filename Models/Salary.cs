using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PEA.Models;


public class Salary
{
    [Required]
    public int EmpNo { get; set; }


    [Column("Salario", TypeName = "decimal(10,2)")]
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }



    [Required]
    public DateTime FromDate { get; set; }
    public DateTime? ToDate { get; set; }


    public Employee? Employee { get; set; }
}