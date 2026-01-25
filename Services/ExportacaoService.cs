using System.Globalization;
using System.Text;
using CsvHelper;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PraOndeFoi.Repository;

namespace PraOndeFoi.Services
{
    public class ExportacaoService : IExportacaoService
    {
        private readonly IFinancasRepository _repository;

        public ExportacaoService(IFinancasRepository repository)
        {
            _repository = repository;
        }

        public async Task<byte[]> ExportarTransacoesCsvAsync(int contaId, DateTime? inicio, DateTime? fim)
        {
            if (!await _repository.ContaExisteAsync(contaId))
            {
                throw new InvalidOperationException("Conta não encontrada.");
            }

            var transacoes = await _repository.ObterTransacoesParaExportacaoAsync(contaId, inicio, fim);
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            var rows = transacoes.Select(t => new ExportacaoTransacaoRow
            {
                DataTransacao = t.DataTransacao,
                Tipo = t.Tipo.ToString(),
                Valor = t.Valor,
                Moeda = t.Moeda,
                CategoriaId = t.CategoriaId,
                Categoria = t.Categoria?.Nome ?? string.Empty,
                Descricao = t.Descricao
            });

            csv.WriteRecords(rows);
            return Encoding.UTF8.GetBytes(writer.ToString());
        }

        public async Task<byte[]> ExportarTransacoesPdfAsync(int contaId, DateTime? inicio, DateTime? fim)
        {
            if (!await _repository.ContaExisteAsync(contaId))
            {
                throw new InvalidOperationException("Conta não encontrada.");
            }

            var transacoes = await _repository.ObterTransacoesParaExportacaoAsync(contaId, inicio, fim);
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Relatório de Transações").FontSize(16).SemiBold();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(75);
                            columns.ConstantColumn(55);
                            columns.ConstantColumn(70);
                            columns.ConstantColumn(55);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Data").SemiBold();
                            header.Cell().Text("Tipo").SemiBold();
                            header.Cell().Text("Valor").SemiBold();
                            header.Cell().Text("Moeda").SemiBold();
                            header.Cell().Text("Categoria").SemiBold();
                            header.Cell().Text("Descrição").SemiBold();
                        });

                        foreach (var transacao in transacoes)
                        {
                            table.Cell().Text(transacao.DataTransacao.ToString("yyyy-MM-dd"));
                            table.Cell().Text(transacao.Tipo.ToString());
                            table.Cell().Text(transacao.Valor.ToString("F2", CultureInfo.InvariantCulture));
                            table.Cell().Text(transacao.Moeda);
                            table.Cell().Text(transacao.Categoria?.Nome ?? string.Empty);
                            table.Cell().Text(transacao.Descricao);
                        }
                    });

                    page.Footer()
                        .AlignRight()
                        .Text($"Gerado em: {DateTime.UtcNow:yyyy-MM-dd HH:mm}")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Medium);
                });
            });

            return document.GeneratePdf();
        }

        private sealed class ExportacaoTransacaoRow
        {
            public DateTime DataTransacao { get; set; }
            public string Tipo { get; set; } = string.Empty;
            public decimal Valor { get; set; }
            public string Moeda { get; set; } = string.Empty;
            public int CategoriaId { get; set; }
            public string Categoria { get; set; } = string.Empty;
            public string Descricao { get; set; } = string.Empty;
        }
    }
}
