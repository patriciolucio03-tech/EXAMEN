
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PEA.Models;


public class Title
{
    [Required]
    public int EmpNo { get; set; }



    [Required, StringLength(100)]
    [Column("Title")]
    public string TitleName { get; set; } = string.Empty;


    [Required]
    public DateTime FromDate { get; set; }
    public DateTime? ToDate { get; set; }


    public Employee? Employee { get; set; }
}