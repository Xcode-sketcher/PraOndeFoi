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
                BadDataFound = null,
                HeaderValidated = null
            };

            using var reader = new StreamReader(csvStream, Encoding.UTF8, true, leaveOpen: true);
            using var csv = new CsvReader(reader, config);

            await foreach (var row in csv.GetRecordsAsync<TransacaoCsvRow>())
            {
                resultado.LinhasProcessadas++;
                if (row == null)
                {
                    resultado.Erros.Add($"Linha {resultado.LinhasProcessadas}: Registro vazio.");
                    continue;
                }

                var linha = csv.Context?.Parser?.Row ?? resultado.LinhasProcessadas;

                if (row.Valor <= 0)
                {
                    resultado.Erros.Add($"Linha {linha}: Valor inválido.");
                    continue;
                }

                if (row.CategoriaId <= 0)
                {
                    resultado.Erros.Add($"Linha {linha}: CategoriaId inválido.");
                    continue;
                }

                if (!TryParseTipo(row.Tipo, out var tipo))
                {
                    resultado.Erros.Add($"Linha {linha}: Tipo inválido.");
                    continue;
                }

                var data = row.DataTransacao;
                if (data == default)
                {
                    resultado.Erros.Add($"Linha {linha}: DataTransacao inválida.");
                    continue;
                }

                if (data.Kind == DateTimeKind.Unspecified)
                {
                    data = DateTime.SpecifyKind(data, DateTimeKind.Utc);
                }

                _repository.AdicionarTransacao(new Transacao
                {
                    ContaId = contaId,
                    Tipo = tipo,
                    Valor = row.Valor,
                    Moeda = string.IsNullOrWhiteSpace(row.Moeda) ? "BRL" : row.Moeda,
                    DataTransacao = data,
                    CategoriaId = row.CategoriaId,
                    Descricao = row.Descricao ?? string.Empty
                });

                resultado.LinhasImportadas++;
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

            if (Enum.TryParse(valor, true, out tipo))
            {
                return true;
            }

            if (int.TryParse(valor, out var numero) && Enum.IsDefined(typeof(TipoMovimento), numero))
            {
                tipo = (TipoMovimento)numero;
                return true;
            }

            return false;
        }
    }
}
