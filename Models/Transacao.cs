using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PraOndeFoi.Models
{
    public class Transacao
    {
        public int Id { get; set; }
        public decimal Valor { get; set; }
        public TipoMovimento Tipo { get; set; }
        public string Moeda { get; set; } = string.Empty;
        public DateTime DataTransacao { get; set; }
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public int ContaId { get; set; }
        public Conta? Conta { get; set; }
        public ICollection<TransacaoTag>? Tags { get; set; }
        public ICollection<AnexoTransacao>? Anexos { get; set; }
    }
}