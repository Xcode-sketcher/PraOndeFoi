using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace PraOndeFoi.Models
{
    public class Usuario
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        [Required]
        public string Nome { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [JsonIgnore]
        public Conta? Conta { get; set; }
    }
}