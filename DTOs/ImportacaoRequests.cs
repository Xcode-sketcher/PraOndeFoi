using System;

namespace PraOndeFoi.DTOs
{
    public class TransacaoCsvRow
    {
        public DateTime DataTransacao { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Moeda { get; set; } = "BRL";
        public int CategoriaId { get; set; }
        public string Descricao { get; set; } = string.Empty;
    }
}
