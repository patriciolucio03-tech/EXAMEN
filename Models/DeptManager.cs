using System.ComponentModel.DataAnnotations;


namespace PEA.Models;


public class DeptManager
{
    [Required, StringLength(4)]
    public string DeptNo { get; set; } = string.Empty;
    [Required]
    public int EmpNo { get; set; }


    [Required]
    public DateTime FromDate { get; set; }
    public DateTime? ToDate { get; set; }


    public Employee? Employee { get; set; }
    public Department? Department { get; set; }
}