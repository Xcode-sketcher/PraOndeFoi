using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using PraOndeFoi.Models;

namespace PraOndeFoi.DTOs
{
    public class NovaMetaRequest
    {
        [Range(1, int.MaxValue)]
        public int ContaId { get; set; }
        [Required]
        public string Nome { get; set; } = string.Empty;
        [Range(0.01, double.MaxValue)]
        public decimal ValorAlvo { get; set; }
        public decimal ValorAtual { get; set; }
        public DateTime DataInicio { get; set; } = DateTime.UtcNow.Date;
        public DateTime? DataFim { get; set; }
        public int? CategoriaId { get; set; }
    }

    public class NovaTagRequest
    {
        [Range(1, int.MaxValue)]
        public int ContaId { get; set; }
        [Required]
        public string Nome { get; set; } = string.Empty;
    }

    public class AdicionarTagTransacaoRequest
    {
        [Range(1, int.MaxValue)]
        public int TransacaoId { get; set; }
        [Range(1, int.MaxValue)]
        public int TagId { get; set; }
    }

    public class NovoAnexoTransacaoRequest
    {
        [Range(1, int.MaxValue)]
        public int TransacaoId { get; set; }
        public TipoAnexo Tipo { get; set; }
        public string ConteudoTexto { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class NovoAnexoArquivoRequest
    {
        [Range(1, int.MaxValue)]
        public int TransacaoId { get; set; }
        public TipoAnexo Tipo { get; set; } = TipoAnexo.Foto;
        [Required]
        public IFormFile Arquivo { get; set; } = null!;
    }

    public class ContribuirMetaRequest
    {
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }
    }
}
