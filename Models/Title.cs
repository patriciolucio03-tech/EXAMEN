using System.ComponentModel.DataAnnotations;


namespace PEA.Models;


public class Title
{
    [Required]
    public int EmpNo { get; set; }


    [Required, StringLength(50)]
    public string TitleName { get; set; } = string.Empty;


    [Required]
    public DateTime FromDate { get; set; }
    public DateTime? ToDate { get; set; }


    public Employee? Employee { get; set; }
}