using System.ComponentModel.DataAnnotations;

namespace PEA.Models.ViewModels;

public class BulkTitlesVM
{
    [Required]
    public int EmpNo { get; set; }

    public List<Row> Items { get; set; } = new()
    {
        new Row() // al menos una fila por defecto
    };

    public class Row
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(50)]
        public string TitleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha desde es obligatoria")]
        public DateTime FromDate { get; set; } = DateTime.Today;

        public DateTime? ToDate { get; set; }
    }
}
