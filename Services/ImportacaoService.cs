using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using PraOndeFoi.DTOs;
using PraOndeFoi.Models;
using PraOndeFoi.Repository;

namespace PraOndeFoi.Services
{
    public class ImportacaoService : IImportacaoService
    {
        private readonly IFinancasRepository _repository;
        private readonly IContaCacheService _cacheService;

        public ImportacaoService(IFinancasRepository repository, IContaCacheService cacheService)
        {
            _repository = repository;
            _cacheService = cacheService;
        }

        public async Task<ImportacaoResultadoResponse> ImportarTransacoesCsvAsync(int contaId, Stream csvStream)
        {
            if (!await _repository.ContaExisteAsync(contaId))
            {
                throw new InvalidOperationException("Conta não encontrada.");
            }

            var resultado = new ImportacaoResultadoResponse();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = context =>
                {
                    resultado.Erros.Add($"Linha {context.Context.Parser.Row}: Dados malformados - {context.Field}");
                },
                HeaderValidated = null,
                Delimiter = ",", // Tenta vírgula primeiro
                DetectDelimiter = true // Detecta automaticamente ponto-e-vírgula ou vírgula
            };

            try
            {
                using var reader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

                // Detectar se é um arquivo com comentários no topo
                var firstLine = await reader.ReadLineAsync();
                if (firstLine != null && firstLine.StartsWith("#"))
                {
                    // Pular linhas de comentário
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (line != null && !line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                        {
                            // Reset stream para a linha do cabeçalho
                            csvStream.Position = 0;
                            var skipReader = new StreamReader(csvStream, Encoding.UTF8);
                            while (await skipReader.ReadLineAsync() is string skipLine && skipLine.StartsWith("#")) { }
                            break;
                        }
                    }
                }
                else
                {
                    csvStream.Position = 0; // Reset para o início
                }

                using var csv = new CsvReader(new StreamReader(csvStream, Encoding.UTF8), config);

                await foreach (var row in csv.GetRecordsAsync<TransacaoCsvRow>())
                {
                    resultado.LinhasProcessadas++;

                    if (row == null)
                    {
                        resultado.Erros.Add($"Linha {resultado.LinhasProcessadas}: Registro vazio.");
                        continue;
                    }

                    var linha = csv.Context?.Parser?.Row ?? resultado.LinhasProcessadas;

                    // Validações mais específicas
                    if (row.Valor <= 0)
                    {
                        resultado.Erros.Add($"Linha {linha}: Valor deve ser maior que zero (valor informado: {row.Valor}).");
                        continue;
                    }

                    if (row.CategoriaId <= 0)
                    {
                        resultado.Erros.Add($"Linha {linha}: CategoriaId inválido. Informe um ID de categoria válido.");
                        continue;
                    }

                    if (!TryParseTipo(row.Tipo, out var tipo))
                    {
                        resultado.Erros.Add($"Linha {linha}: Tipo inválido ('{row.Tipo}'). Use 'Entrada' (1) ou 'Saida' (2).");
                        continue;
                    }

                    var data = row.DataTransacao;
                    if (data == default)
                    {
                        resultado.Erros.Add($"Linha {linha}: DataTransacao inválida ou ausente.");
                        continue;
                    }

                    if (data.Kind == DateTimeKind.Unspecified)
                    {
                        data = DateTime.SpecifyKind(data, DateTimeKind.Utc);
                    }
                    else if (data.Kind == DateTimeKind.Local)
                    {
                        data = data.ToUniversalTime();
                    }

                    // Validar descrição
                    var descricao = row.Descricao?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(descricao))
                    {
                        descricao = $"Transação importada em {DateTime.UtcNow:dd/MM/yyyy}";
                    }

                    _repository.AdicionarTransacao(new Transacao
                    {
                        ContaId = contaId,
                        Tipo = tipo,
                        Valor = row.Valor,
                        Moeda = string.IsNullOrWhiteSpace(row.Moeda) ? "BRL" : row.Moeda.Trim().ToUpperInvariant(),
                        DataTransacao = data,
                        CategoriaId = row.CategoriaId,
                        Descricao = descricao
                    });

                    resultado.LinhasImportadas++;
                }
            }
            catch (CsvHelperException ex)
            {
                resultado.Erros.Add($"Erro ao processar CSV: {ex.Message}");
            }
            catch (Exception ex)
            {
                resultado.Erros.Add($"Erro inesperado: {ex.Message}");
            }

            if (resultado.LinhasImportadas > 0)
            {
                await _repository.SalvarAsync();
                _cacheService.IncrementarVersao(contaId);
            }

            return resultado;
        }

        private static bool TryParseTipo(string valor, out TipoMovimento tipo)
        {
            tipo = TipoMovimento.Saida;

            if (string.IsNullOrWhiteSpace(valor))
            {
                return false;
            }

            var valorNormalizado = valor.Trim().ToLowerInvariant();

            // Aceitar nomes em português
            if (valorNormalizado == "entrada" || valorNormalizado == "receita" || valorNormalizado == "credito")
            {
                tipo = TipoMovimento.Entrada;
                return true;
            }

            if (valorNormalizado == "saida" || valorNormalizado == "saída" || valorNormalizado == "despesa" || valorNormalizado == "debito" || valorNormalizado == "débito")
            {
                tipo = TipoMovimento.Saida;
                return true;
            }

            // Tentar parse por enum
            if (Enum.TryParse(valor, true, out tipo))
            {
                return true;
            }

            // Aceitar números
            if (int.TryParse(valor, out var numero) && Enum.IsDefined(typeof(TipoMovimento), numero))
            {
                tipo = (TipoMovimento)numero;
                return true;
            }

            return false;
        }
    }
}
