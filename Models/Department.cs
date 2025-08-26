using System.ComponentModel.DataAnnotations;


namespace PEA.Models;


public class Department
{
    [Key]
    [StringLength(4)]
    public string DeptNo { get; set; } = string.Empty;


    [Required, StringLength(40)]
    public string DeptName { get; set; } = string.Empty;


    public bool Activo { get; set; } = true;


    public ICollection<DeptEmp> DeptEmps { get; set; } = new List<DeptEmp>();
    public ICollection<DeptManager> DeptManagers { get; set; } = new List<DeptManager>();
}