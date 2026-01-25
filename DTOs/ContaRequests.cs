using System.ComponentModel.DataAnnotations;

namespace PraOndeFoi.DTOs
{
    public class CriarContaRequest
    {
        [Required]
        public string UsuarioId { get; set; } = string.Empty;
        [Required]
        public string Nome { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Moeda { get; set; } = "BRL";
        public decimal SaldoInicial { get; set; }
    }
}
