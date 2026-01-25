using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PraOndeFoi.Models
{
    public class AnexoTransacao
    {
        public int Id { get; set; }
        public TipoAnexo Tipo { get; set; }
        public string ConteudoTexto { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int TransacaoId { get; set; }
        [JsonIgnore]
        public Transacao? Transacao { get; set; }
    }
}
