using System.ComponentModel.DataAnnotations;

namespace PraOndeFoi.Models
{
    public class OrcamentoMensal
    {
        public int Id { get; set; }
        public int Mes { get; set; }
        public int Ano { get; set; }
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
        public decimal Limite { get; set; }
        public int ContaId { get; set; }
        public Conta? Conta { get; set; }
    }
}
