using System;
using System.ComponentModel.DataAnnotations;

namespace PraOndeFoi.Models
{
    public class Assinatura
    {
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Moeda { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
        public string Frequencia { get; set; } = "Mensal";
        public int IntervaloQuantidade { get; set; } = 1;
        public IntervaloUnidade IntervaloUnidade { get; set; } = IntervaloUnidade.Mes;
        public DateTime DataInicio { get; set; } = DateTime.UtcNow.Date;
        public DateTime? ProximaCobranca { get; set; }
        public bool Ativa { get; set; } = true;
        public int ContaId { get; set; }
        public Conta? Conta { get; set; }
    }
}
