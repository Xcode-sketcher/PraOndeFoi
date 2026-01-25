using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PraOndeFoi.Models;
using PraOndeFoi.Repository;

namespace PraOndeFoi.Services
{
    public class RecorrenciasHostedService : BackgroundService
    {
        private static readonly TimeSpan IntervaloExecucao = TimeSpan.FromHours(24);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RecorrenciasHostedService> _logger;

        public RecorrenciasHostedService(IServiceScopeFactory scopeFactory, ILogger<RecorrenciasHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ProcessarAsync(stoppingToken);
            using var timer = new PeriodicTimer(IntervaloExecucao);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessarAsync(stoppingToken);
            }
        }

        private async Task ProcessarAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IFinancasRepository>();
                var agora = DateTime.UtcNow;

                var recorrencias = await repository.ObterRecorrenciasVencidasAsync(agora);
                foreach (var recorrencia in recorrencias)
                {
                    var proxima = recorrencia.ProximaExecucao ?? recorrencia.DataInicio;
                    while (proxima <= agora)
                    {
                        repository.AdicionarTransacao(new Transacao
                        {
                            ContaId = recorrencia.ContaId,
                            Tipo = recorrencia.Tipo,
                            Valor = recorrencia.Valor,
                            Moeda = recorrencia.Moeda,
                            DataTransacao = proxima,
                            CategoriaId = recorrencia.CategoriaId,
                            Descricao = string.IsNullOrWhiteSpace(recorrencia.Descricao) ? "Recorrência" : recorrencia.Descricao
                        });
                        proxima = CalcularProximaData(proxima, recorrencia.IntervaloQuantidade, recorrencia.IntervaloUnidade);
                    }

                    recorrencia.ProximaExecucao = proxima;
                }

                var assinaturas = await repository.ObterAssinaturasVencidasAsync(agora);
                foreach (var assinatura in assinaturas)
                {
                    var proxima = assinatura.ProximaCobranca ?? assinatura.DataInicio;
                    while (proxima <= agora)
                    {
                        repository.AdicionarTransacao(new Transacao
                        {
                            ContaId = assinatura.ContaId,
                            Tipo = TipoMovimento.Saida,
                            Valor = assinatura.Valor,
                            Moeda = assinatura.Moeda,
                            DataTransacao = proxima,
                            CategoriaId = assinatura.CategoriaId,
                            Descricao = $"Assinatura: {assinatura.Nome}"
                        });
                        proxima = CalcularProximaData(proxima, assinatura.IntervaloQuantidade, assinatura.IntervaloUnidade);
                    }

                    assinatura.ProximaCobranca = proxima;
                }

                await repository.SalvarAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar recorrências e assinaturas.");
            }
        }

        private static DateTime CalcularProximaData(DateTime atual, int intervaloQuantidade, IntervaloUnidade intervaloUnidade)
        {
            if (intervaloQuantidade <= 0)
            {
                intervaloQuantidade = 1;
            }

            return intervaloUnidade == IntervaloUnidade.Dia
                ? atual.AddDays(intervaloQuantidade)
                : atual.AddMonths(intervaloQuantidade);
        }
    }
}
