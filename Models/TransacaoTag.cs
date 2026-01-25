using System.Text.Json.Serialization;

namespace PraOndeFoi.Models
{
    public class TransacaoTag
    {
        public int TransacaoId { get; set; }
        [JsonIgnore]
        public Transacao? Transacao { get; set; }
        public int TagId { get; set; }
        [JsonIgnore]
        public Tag? Tag { get; set; }
    }
}
