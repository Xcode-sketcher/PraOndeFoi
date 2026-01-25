using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PraOndeFoi.Models
{
    public class Conta
    {
        public int Id { get; set; }

        public decimal Saldo { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        [Required]
        public Usuario Usuario { get; set; } = null!;
        public string Moeda { get; set; } = string.Empty;
        public ICollection<Transacao>? Transacoes { get; set; }

    }

}