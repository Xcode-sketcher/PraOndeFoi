using System.ComponentModel.DataAnnotations;

namespace PraOndeFoi.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; } = string.Empty;
        public bool Predefinida { get; set; } = false;
    }
}
