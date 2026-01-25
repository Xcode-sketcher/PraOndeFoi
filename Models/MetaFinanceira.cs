using System.ComponentModel.DataAnnotations;

namespace PraOndeFoi.Models
{
    public class MetaFinanceira
    {
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; } = string.Empty;
        public decimal ValorAlvo { get; set; }
        public decimal ValorAtual { get; set; }
        public DateTime DataInicio { get; set; } = DateTime.UtcNow.Date;
        public DateTime? DataFim { get; set; }
        public int ContaId { get; set; }
        public Conta? Conta { get; set; }
        public int? CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
    }
}
