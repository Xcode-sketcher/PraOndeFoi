using System.ComponentModel.DataAnnotations;

namespace PraOndeFoi.Models
{
    public class Tag
    {
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; } = string.Empty;
        public int ContaId { get; set; }
        public Conta? Conta { get; set; }
        public ICollection<TransacaoTag>? Transacoes { get; set; }
    }
}
