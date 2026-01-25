using System;
using System.ComponentModel.DataAnnotations;
using PraOndeFoi.Models;

namespace PraOndeFoi.DTOs
{
    public class NovaTransacaoRequest
    {
        [Range(1, int.MaxValue)]
        public int ContaId { get; set; }
        [Required]
        public TipoMovimento Tipo { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }
        public string Moeda { get; set; } = "BRL";
        public DateTime DataTransacao { get; set; } = DateTime.UtcNow;
        [Range(1, int.MaxValue)]
        public int CategoriaId { get; set; }
        public string Descricao { get; set; } = string.Empty;
    }

    public class NovaRecorrenciaRequest
    {
        [Range(1, int.MaxValue)]
        public int ContaId { get; set; }
        [Required]
        public TipoMovimento Tipo { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }
        public string Moeda { get; set; } = "BRL";
        [Range(1, int.MaxValue)]
        public int CategoriaId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Frequencia { get; set; } = "Mensal";
        [Range(1, int.MaxValue)]
        public int IntervaloQuantidade { get; set; } = 1;
        public IntervaloUnidade IntervaloUnidade { get; set; } = IntervaloUnidade.Mes;
        public DateTime DataInicio { get; set; } = DateTime.UtcNow.Date;
        public int? DiaDoMes { get; set; }
        public DateTime? ProximaExecucao { get; set; }
        public bool Ativa { get; set; } = true;
    }

    public class NovaAssinaturaRequest
    {
        [Range(1, int.MaxValue)]
        public int ContaId { get; set; }
        [Required]
        public string Nome { get; set; } = string.Empty;
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }
        public string Moeda { get; set; } = "BRL";
        [Range(1, int.MaxValue)]
        public int CategoriaId { get; set; }
        public string Frequencia { get; set; } = "Mensal";
        [Range(1, int.MaxValue)]
        public int IntervaloQuantidade { get; set; } = 1;
        public IntervaloUnidade IntervaloUnidade { get; set; } = IntervaloUnidade.Mes;
        public DateTime DataInicio { get; set; } = DateTime.UtcNow.Date;
        public DateTime? ProximaCobranca { get; set; }
        public bool Ativa { get; set; } = true;
    }

    public class NovoOrcamentoRequest
    {
        [Range(1, int.MaxValue)]
        public int ContaId { get; set; }
        [Range(1, 12)]
        public int Mes { get; set; }
        [Range(2000, 2100)]
        public int Ano { get; set; }
        [Range(1, int.MaxValue)]
        public int CategoriaId { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal Limite { get; set; }
    }

    public enum OrdenacaoTransacao
    {
        DataDesc,
        DataAsc,
        ValorDesc,
        ValorAsc
    }

    public class TransacaoQueryRequest
    {
        [Range(1, int.MaxValue)]
        public int ContaId { get; set; }
        public TipoMovimento? Tipo { get; set; }
        public int? CategoriaId { get; set; }
        public string? Inicio { get; set; }
        public string? Fim { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal? ValorMin { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal? ValorMax { get; set; }
        public List<int> Tags { get; set; } = new();
        public string? Search { get; set; }
        public OrdenacaoTransacao Ordenacao { get; set; } = OrdenacaoTransacao.DataDesc;
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;
        [Range(1, 10)]
        public int PageSize { get; set; } = 10;
    }

    public class InsightsQueryRequest
    {
        [Range(1, int.MaxValue)]
        public int ContaId { get; set; }
        [Range(3, 24)]
        public int MesesHistorico { get; set; } = 12;
    }
}
