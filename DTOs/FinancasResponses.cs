using PraOndeFoi.Models;

namespace PraOndeFoi.DTOs
{
    public class ResumoMensalResponse
    {
        public int ContaId { get; set; }
        public int Mes { get; set; }
        public int Ano { get; set; }
        public decimal TotalEntradas { get; set; }
        public decimal TotalSaidas { get; set; }
        public decimal SaldoMes { get; set; }
        public decimal TotalRecorrenteEntrada { get; set; }
        public decimal TotalRecorrenteSaida { get; set; }
        public decimal TotalAssinaturasSaida { get; set; }
        public decimal SaldoProjetado { get; set; }
    }

    public class OrcamentoStatusResponse
    {
        public int CategoriaId { get; set; }
        public string CategoriaNome { get; set; } = string.Empty;
        public int Mes { get; set; }
        public int Ano { get; set; }
        public decimal Limite { get; set; }
        public decimal Gasto { get; set; }
        public decimal Disponivel { get; set; }
    }

    public class TransacaoResponse
    {
        public int Id { get; set; }
        public decimal Valor { get; set; }
        public TipoMovimento Tipo { get; set; }
        public string Moeda { get; set; } = string.Empty;
        public DateTime DataTransacao { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int ContaId { get; set; }
        public string ContaNome { get; set; } = string.Empty;
        public IReadOnlyList<TagResponse> Tags { get; set; } = new List<TagResponse>();
        public IReadOnlyList<AnexoResponse> Anexos { get; set; } = new List<AnexoResponse>();
    }

    public class TagResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class AnexoResponse
    {
        public int Id { get; set; }
        public TipoAnexo Tipo { get; set; }
        public string ConteudoTexto { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class ContaResponse
    {
        public int Id { get; set; }
        public decimal Saldo { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public string UsuarioNome { get; set; } = string.Empty;
        public IReadOnlyList<TransacaoResponse> Transacoes { get; set; } = new List<TransacaoResponse>();
    }

    public class PaginationMetadata
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }

    public class PagedResponse<T>
    {
        public IReadOnlyList<T> Data { get; set; } = new List<T>();
        public PaginationMetadata Pagination { get; set; } = new PaginationMetadata();
    }

    public class InsightCategoriaResponse
    {
        public int CategoriaId { get; set; }
        public string CategoriaNome { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Percentual { get; set; }
    }

    public class InsightsResponse
    {
        public int ContaId { get; set; }
        public int MesReferencia { get; set; }
        public int AnoReferencia { get; set; }
        public decimal TotalSaidasMesAtual { get; set; }
        public decimal TotalSaidasMesAnterior { get; set; }
        public decimal VariacaoPercentual { get; set; }
        public decimal PrevisaoSaidasProximoMes { get; set; }
        public string Modelo { get; set; } = string.Empty;
        public decimal Confianca { get; set; }
        public IReadOnlyList<InsightCategoriaResponse> TopCategorias { get; set; } = new List<InsightCategoriaResponse>();
        public IReadOnlyList<string> Sugestoes { get; set; } = new List<string>();
    }
}
