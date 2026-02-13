using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PraOndeFoi.Repository;
using PraOndeFoi.Models;

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

            var builder = new StringBuilder();

            // Metadados do relatório
            builder.AppendLine($"# Relatório de Transações");
            builder.AppendLine($"# Conta ID: {contaId}");
            builder.AppendLine($"# Período: {inicio?.ToString("dd/MM/yyyy") ?? "Início"} até {fim?.ToString("dd/MM/yyyy") ?? "Fim"}");
            builder.AppendLine($"# Gerado em: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} UTC");
            builder.AppendLine($"# Total de transações: {transacoes.Count}");
            builder.AppendLine();

            using var writer = new StringWriter(builder);
            var config = new CsvConfiguration(CultureInfo.GetCultureInfo("pt-BR"))
            {
                Delimiter = ";",
                HasHeaderRecord = true
            };

            using var csv = new CsvWriter(writer, config);

            var rows = transacoes.Select(t => new ExportacaoTransacaoRow
            {
                DataTransacao = t.DataTransacao,
                Tipo = t.Tipo == TipoMovimento.Entrada ? "Entrada" : "Saída",
                Valor = t.Valor,
                Moeda = t.Moeda,
                CategoriaId = t.CategoriaId,
                Categoria = t.Categoria?.Nome ?? "Sem categoria",
                Descricao = t.Descricao
            }).ToList();

            csv.WriteRecords(rows);

            // Adicionar totais ao final
            writer.WriteLine();
            writer.WriteLine("# Resumo");
            var totalEntradas = transacoes.Where(t => t.Tipo == TipoMovimento.Entrada).Sum(t => t.Valor);
            var totalSaidas = transacoes.Where(t => t.Tipo == TipoMovimento.Saida).Sum(t => t.Valor);
            writer.WriteLine($"# Total Entradas: {totalEntradas:N2}");
            writer.WriteLine($"# Total Saídas: {totalSaidas:N2}");
            writer.WriteLine($"# Saldo: {totalEntradas - totalSaidas:N2}");

            return Encoding.UTF8.GetBytes(writer.ToString());
        }

        public async Task<byte[]> ExportarTransacoesPdfAsync(int contaId, DateTime? inicio, DateTime? fim)
        {
            if (!await _repository.ContaExisteAsync(contaId))
            {
                throw new InvalidOperationException("Conta não encontrada.");
            }

            var todasTransacoes = await _repository.ObterTransacoesParaExportacaoAsync(contaId, inicio, fim);

            // Validar se há transações
            if (todasTransacoes == null || todasTransacoes.Count == 0)
            {
                var periodoMsg = inicio.HasValue || fim.HasValue
                    ? $" no período de {inicio?.ToString("dd/MM/yyyy") ?? "início"} até {fim?.ToString("dd/MM/yyyy") ?? "fim"}"
                    : "";
                throw new InvalidOperationException($"Nenhuma transação encontrada para exportar{periodoMsg}.");
            }

            // Limitar para evitar timeout (máximo 1000 transações no PDF)
            const int maxTransacoes = 1000;
            var transacoes = todasTransacoes.Count > maxTransacoes
                ? todasTransacoes.Take(maxTransacoes).ToList()
                : todasTransacoes;

            var avisoLimite = todasTransacoes.Count > maxTransacoes;
            var totalOmitido = todasTransacoes.Count - maxTransacoes;

            // Calcular totais (de TODAS as transações, não apenas as exibidas)
            var totalEntradas = todasTransacoes.Where(t => t.Tipo == TipoMovimento.Entrada).Sum(t => t.Valor);
            var totalSaidas = todasTransacoes.Where(t => t.Tipo == TipoMovimento.Saida).Sum(t => t.Valor);
            var saldo = totalEntradas - totalSaidas;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);

                    // Cabeçalho
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("Relatório de Transações")
                                .FontSize(20)
                                .SemiBold()
                                .FontColor(Colors.Blue.Darken3);

                            column.Item().Text($"Conta ID: {contaId}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);

                            column.Item().Text($"Período: {inicio?.ToString("dd/MM/yyyy") ?? "Início"} até {fim?.ToString("dd/MM/yyyy") ?? "Fim"}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(100).AlignRight().Text($"Gerado em:\n{DateTime.UtcNow:dd/MM/yyyy HH:mm}")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Medium);
                    });

                    // Conteúdo
                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        // Resumo em destaque
                        column.Item().Background(Colors.Blue.Lighten4).Padding(15).Column(resumo =>
                        {
                            resumo.Item().Text("Resumo Financeiro").FontSize(14).SemiBold().FontColor(Colors.Blue.Darken3);
                            resumo.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text($"Total Entradas: R$ {totalEntradas:N2}").FontSize(11).FontColor(Colors.Green.Darken2);
                                row.RelativeItem().Text($"Total Saídas: R$ {totalSaidas:N2}").FontSize(11).FontColor(Colors.Red.Darken2);
                                row.RelativeItem().Text($"Saldo: R$ {saldo:N2}").FontSize(11).SemiBold()
                                    .FontColor(saldo >= 0 ? Colors.Green.Darken3 : Colors.Red.Darken3);
                            });
                        });

                        column.Item().PaddingTop(15).Column(infoColumn =>
                        {
                            infoColumn.Item().Text($"Total de transações encontradas: {todasTransacoes.Count}")
                                .FontSize(10).FontColor(Colors.Grey.Darken1);

                            if (avisoLimite)
                            {
                                infoColumn.Item().PaddingTop(3).Text($"⚠ Exibindo apenas as {maxTransacoes} transações mais recentes. {totalOmitido} transações foram omitidas da visualização, mas os totais consideram todas as transações.")
                                    .FontSize(9).FontColor(Colors.Orange.Darken2).Italic();
                            }
                        });

                        // Tabela de transações
                        column.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80);  // Data
                                columns.ConstantColumn(60);  // Tipo
                                columns.ConstantColumn(80);  // Valor
                                columns.ConstantColumn(50);  // Moeda
                                columns.RelativeColumn(2);   // Categoria
                                columns.RelativeColumn(3);   // Descrição
                            });

                            // Cabeçalho da tabela
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text("Data").FontColor(Colors.White).FontSize(10).SemiBold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text("Tipo").FontColor(Colors.White).FontSize(10).SemiBold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text("Valor").FontColor(Colors.White).FontSize(10).SemiBold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text("Moeda").FontColor(Colors.White).FontSize(10).SemiBold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text("Categoria").FontColor(Colors.White).FontSize(10).SemiBold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                                    .Text("Descrição").FontColor(Colors.White).FontSize(10).SemiBold();
                            });

                            // Linhas de transações
                            var index = 0;
                            foreach (var transacao in transacoes)
                            {
                                var isEven = index % 2 == 0;
                                var bgColor = isEven ? Colors.White : Colors.Grey.Lighten3;
                                var tipoColor = transacao.Tipo == TipoMovimento.Entrada
                                    ? Colors.Green.Darken1
                                    : Colors.Red.Darken1;

                                table.Cell().Background(bgColor).Padding(5)
                                    .Text(transacao.DataTransacao.ToString("dd/MM/yyyy")).FontSize(9);
                                table.Cell().Background(bgColor).Padding(5)
                                    .Text(transacao.Tipo == TipoMovimento.Entrada ? "Entrada" : "Saída")
                                    .FontSize(9).FontColor(tipoColor).SemiBold();
                                table.Cell().Background(bgColor).Padding(5)
                                    .Text($"{transacao.Valor:N2}").FontSize(9);
                                table.Cell().Background(bgColor).Padding(5)
                                    .Text(transacao.Moeda).FontSize(9);
                                table.Cell().Background(bgColor).Padding(5)
                                    .Text(transacao.Categoria?.Nome ?? "Sem categoria").FontSize(9);
                                table.Cell().Background(bgColor).Padding(5)
                                    .Text(transacao.Descricao).FontSize(9);

                                index++;
                            }
                        });
                    });

                    // Rodapé
                    page.Footer().AlignCenter().Row(row =>
                    {
                        row.RelativeItem().Text(text =>
                        {
                            text.Span("PraOndeFoi - Sistema de Gestão Financeira").FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                    });
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
