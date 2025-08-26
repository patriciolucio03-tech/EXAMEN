using System.ComponentModel.DataAnnotations;


namespace PEA.Models;


public class Employee
{
    [Key]
    public int EmpNo { get; set; }


    [Required, StringLength(20)]
    public string CI { get; set; } = string.Empty; // cédula


    [Required]
    public DateTime BirthDate { get; set; }


    [Required, StringLength(14)]
    public string FirstName { get; set; } = string.Empty;


    [Required, StringLength(16)]
    public string LastName { get; set; } = string.Empty;


    [Required, RegularExpression("^(M|F)$")]
    public string Gender { get; set; } = "M"; // 'M' o 'F'


    [Required]
    public DateTime HireDate { get; set; }


    [Required, EmailAddress, StringLength(100)]
    public string Correo { get; set; } = string.Empty;


    public bool Activo { get; set; } = true;


    public ICollection<DeptEmp> DeptEmps { get; set; } = new List<DeptEmp>();
    public ICollection<Title> Titles { get; set; } = new List<Title>();
    public ICollection<Salary> Salaries { get; set; } = new List<Salary>();
}