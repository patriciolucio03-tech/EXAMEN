using System.ComponentModel.DataAnnotations;


namespace PEA.Models;


public class DeptEmp
{
    [Required]
    public int EmpNo { get; set; }


    [Required, StringLength(4)]
    public string DeptNo { get; set; } = string.Empty;


    [Required]
    public DateTime FromDate { get; set; }
    public DateTime? ToDate { get; set; }


    public Employee? Employee { get; set; }
    public Department? Department { get; set; }
}