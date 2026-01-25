using Microsoft.Extensions.Caching.Memory;
using Microsoft.ML;
using Microsoft.ML.Data;
using PraOndeFoi.DTOs;
using PraOndeFoi.Models;
using PraOndeFoi.Repository;

namespace PraOndeFoi.Services
{
    public class FinancasService : IFinancasService
    {
        private readonly IFinancasRepository _repository;
        private readonly IContaRepository _contaRepository;
        private readonly IMemoryCache _cache;
        private readonly IContaCacheService _contaCacheService;
        private static readonly TimeSpan CacheDuracao = TimeSpan.FromMinutes(5);

        public FinancasService(IFinancasRepository repository, IContaRepository contaRepository, IMemoryCache cache, IContaCacheService contaCacheService)
        {
            _repository = repository;
            _contaRepository = contaRepository;
            _cache = cache;
            _contaCacheService = contaCacheService;
        }

        public async Task<Transacao> CriarTransacaoAsync(NovaTransacaoRequest request)
        {
            await GarantirContaAsync(request.ContaId);
            ValidarValor(request.Valor);

            var transacao = new Transacao
            {
                ContaId = request.ContaId,
                Tipo = request.Tipo,
                Valor = request.Valor,
                Moeda = request.Moeda,
                DataTransacao = request.DataTransacao,
                CategoriaId = request.CategoriaId,
                Descricao = request.Descricao
            };

            _repository.AdicionarTransacao(transacao);
            await _repository.SalvarAsync();
            _contaCacheService.IncrementarVersao(request.ContaId);
            return transacao;
        }

        public async Task<Recorrencia> CriarRecorrenciaAsync(NovaRecorrenciaRequest request)
        {
            await GarantirContaAsync(request.ContaId);
            ValidarValor(request.Valor);

            var recorrencia = new Recorrencia
            {
                ContaId = request.ContaId,
                Tipo = request.Tipo,
                Valor = request.Valor,
                Moeda = request.Moeda,
                CategoriaId = request.CategoriaId,
                Descricao = request.Descricao,
                Frequencia = request.Frequencia,
                IntervaloQuantidade = request.IntervaloQuantidade,
                IntervaloUnidade = request.IntervaloUnidade,
                DataInicio = request.DataInicio,
                DiaDoMes = request.DiaDoMes,
                ProximaExecucao = request.ProximaExecucao,
                Ativa = request.Ativa
            };

            _repository.AdicionarRecorrencia(recorrencia);
            await _repository.SalvarAsync();
            _contaCacheService.IncrementarVersao(request.ContaId);
            return recorrencia;
        }

        public async Task<Assinatura> CriarAssinaturaAsync(NovaAssinaturaRequest request)
        {
            await GarantirContaAsync(request.ContaId);
            ValidarValor(request.Valor);

            var assinatura = new Assinatura
            {
                ContaId = request.ContaId,
                Nome = request.Nome,
                Valor = request.Valor,
                Moeda = request.Moeda,
                CategoriaId = request.CategoriaId,
                Frequencia = request.Frequencia,
                IntervaloQuantidade = request.IntervaloQuantidade,
                IntervaloUnidade = request.IntervaloUnidade,
                DataInicio = request.DataInicio,
                ProximaCobranca = request.ProximaCobranca,
                Ativa = request.Ativa
            };

            _repository.AdicionarAssinatura(assinatura);
            await _repository.SalvarAsync();
            _contaCacheService.IncrementarVersao(request.ContaId);
            return assinatura;
        }

        public async Task<OrcamentoMensal> CriarOrcamentoAsync(NovoOrcamentoRequest request)
        {
            await GarantirContaAsync(request.ContaId);
            ValidarValor(request.Limite);

            var orcamento = new OrcamentoMensal
            {
                ContaId = request.ContaId,
                Mes = request.Mes,
                Ano = request.Ano,
                CategoriaId = request.CategoriaId,
                Limite = request.Limite
            };

            _repository.AdicionarOrcamento(orcamento);
            await _repository.SalvarAsync();
            _contaCacheService.IncrementarVersao(request.ContaId);
            return orcamento;
        }

        public async Task<OrcamentoMensal> AtualizarOrcamentoAsync(int orcamentoId, NovoOrcamentoRequest request)
        {
            await GarantirContaAsync(request.ContaId);
            ValidarValor(request.Limite);

            var orcamento = await _repository.ObterOrcamentoPorIdAsync(orcamentoId);
            if (orcamento == null)
            {
                throw new InvalidOperationException("Orçamento não encontrado.");
            }

            if (orcamento.ContaId != request.ContaId)
            {
                throw new InvalidOperationException("Orçamento não pertence à conta informada.");
            }

            orcamento.Mes = request.Mes;
            orcamento.Ano = request.Ano;
            orcamento.CategoriaId = request.CategoriaId;
            orcamento.Limite = request.Limite;

            _repository.AtualizarOrcamento(orcamento);
            await _repository.SalvarAsync();
            _contaCacheService.IncrementarVersao(request.ContaId);
            return orcamento;
        }

        public async Task RemoverOrcamentoAsync(int orcamentoId)
        {
            var orcamento = await _repository.ObterOrcamentoPorIdAsync(orcamentoId);
            if (orcamento == null)
            {
                throw new InvalidOperationException("Orçamento não encontrado.");
            }

            await GarantirContaAsync(orcamento.ContaId);
            _repository.RemoverOrcamento(orcamento);
            await _repository.SalvarAsync();
            _contaCacheService.IncrementarVersao(orcamento.ContaId);
        }

        public async Task<ResumoMensalResponse> ObterResumoMensalAsync(int contaId, int mes, int ano)
        {
            await GarantirContaAsync(contaId);

            var versao = _contaCacheService.ObterVersao(contaId);
            var cacheKey = $"resumo:{contaId}:{mes}:{ano}:v{versao}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuracao;
                entry.SlidingExpiration = TimeSpan.FromMinutes(2);
                entry.Size = 1;

                var transacoesMes = await _repository.ObterTransacoesMesAsync(contaId, mes, ano);

                var totalEntradas = transacoesMes
                    .Where(t => t.Tipo == TipoMovimento.Entrada)
                    .Sum(t => t.Valor);

                var totalSaidas = transacoesMes
                    .Where(t => t.Tipo == TipoMovimento.Saida)
                    .Sum(t => t.Valor);

                var recorrentes = await _repository.ObterRecorrenciasAtivasAsync(contaId);

                var totalRecorrenteEntrada = recorrentes
                    .Where(r => r.Tipo == TipoMovimento.Entrada)
                    .Sum(r => r.Valor * ContarOcorrenciasNoMes(r.DataInicio, r.IntervaloQuantidade, r.IntervaloUnidade, mes, ano));

                var totalRecorrenteSaida = recorrentes
                    .Where(r => r.Tipo == TipoMovimento.Saida)
                    .Sum(r => r.Valor * ContarOcorrenciasNoMes(r.DataInicio, r.IntervaloQuantidade, r.IntervaloUnidade, mes, ano));

                var assinaturas = await _repository.ObterAssinaturasAtivasAsync(contaId);
                var totalAssinaturas = assinaturas
                    .Sum(a => a.Valor * ContarOcorrenciasNoMes(a.DataInicio, a.IntervaloQuantidade, a.IntervaloUnidade, mes, ano));

                var saldoMes = totalEntradas - totalSaidas;
                var saldoProjetado = saldoMes + totalRecorrenteEntrada - totalRecorrenteSaida - totalAssinaturas;

                return new ResumoMensalResponse
                {
                    ContaId = contaId,
                    Mes = mes,
                    Ano = ano,
                    TotalEntradas = totalEntradas,
                    TotalSaidas = totalSaidas,
                    SaldoMes = saldoMes,
                    TotalRecorrenteEntrada = totalRecorrenteEntrada,
                    TotalRecorrenteSaida = totalRecorrenteSaida,
                    TotalAssinaturasSaida = totalAssinaturas,
                    SaldoProjetado = saldoProjetado
                };
            }) ?? new ResumoMensalResponse
            {
                ContaId = contaId,
                Mes = mes,
                Ano = ano
            };
        }

        public async Task<IReadOnlyList<OrcamentoStatusResponse>> ObterStatusOrcamentosAsync(int contaId, int mes, int ano)
        {
            await GarantirContaAsync(contaId);

            var orcamentos = await _repository.ObterOrcamentosMesAsync(contaId, mes, ano);
            var gastosPorCategoria = await _repository.ObterGastosPorCategoriaAsync(contaId, mes, ano);
            var categorias = await ObterCategoriasAsync();

            return orcamentos.Select(o =>
            {
                var gasto = gastosPorCategoria.FirstOrDefault(g => g.CategoriaId == o.CategoriaId).Total;
                var categoriaNome = categorias.FirstOrDefault(c => c.Id == o.CategoriaId)?.Nome ?? string.Empty;
                return new OrcamentoStatusResponse
                {
                    CategoriaId = o.CategoriaId,
                    CategoriaNome = categoriaNome,
                    Mes = o.Mes,
                    Ano = o.Ano,
                    Limite = o.Limite,
                    Gasto = gasto,
                    Disponivel = o.Limite - gasto
                };
            }).ToList();
        }

        public async Task<PagedResponse<TransacaoResponse>> ObterTransacoesAsync(TransacaoQueryRequest request)
        {
            await GarantirContaAsync(request.ContaId);

            var inicioUtc = ParseDateUtc(request.Inicio, isFim: false);
            var fimUtc = ParseDateUtc(request.Fim, isFim: true);

            var page = Math.Max(1, request.Page);
            var pageSize = Math.Min(50, Math.Max(1, request.PageSize));

            var resultado = await _repository.ObterTransacoesPaginadasAsync(
                request.ContaId,
                request.Tipo,
                request.CategoriaId,
                inicioUtc,
                fimUtc,
                request.ValorMin,
                request.ValorMax,
                request.Tags,
                request.Search,
                request.Ordenacao,
                page,
                pageSize);

            var totalPages = resultado.Total == 0 ? 0 : (int)Math.Ceiling(resultado.Total / (double)pageSize);

            return new PagedResponse<TransacaoResponse>
            {
                Data = resultado.Transacoes.Select(t => MapToTransacaoResponse(t, "Conta")).ToList(),
                Pagination = new PaginationMetadata
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = resultado.Total,
                    TotalPages = totalPages,
                    HasNext = page < totalPages,
                    HasPrevious = page > 1
                }
            };
        }

        public async Task<Transacao> AtualizarTransacaoAsync(int transacaoId, NovaTransacaoRequest request)
        {
            await GarantirContaAsync(request.ContaId);
            ValidarValor(request.Valor);

            var transacao = await _repository.ObterTransacaoAsync(transacaoId);
            if (transacao == null)
            {
                throw new InvalidOperationException("Transação não encontrada.");
            }

            if (transacao.ContaId != request.ContaId)
            {
                throw new InvalidOperationException("Transação não pertence à conta informada.");
            }

            transacao.Tipo = request.Tipo;
            transacao.Valor = request.Valor;
            transacao.Moeda = request.Moeda;
            transacao.DataTransacao = request.DataTransacao;
            transacao.CategoriaId = request.CategoriaId;
            transacao.Descricao = request.Descricao;

            _repository.AtualizarTransacao(transacao);
            await _repository.SalvarAsync();
            _contaCacheService.IncrementarVersao(request.ContaId);
            return transacao;
        }

        public async Task RemoverTransacaoAsync(int transacaoId)
        {
            var transacao = await _repository.ObterTransacaoAsync(transacaoId);
            if (transacao == null)
            {
                throw new InvalidOperationException("Transação não encontrada.");
            }

            await GarantirContaAsync(transacao.ContaId);
            _repository.RemoverTransacao(transacao);
            await _repository.SalvarAsync();
            _contaCacheService.IncrementarVersao(transacao.ContaId);
        }

        public async Task<IReadOnlyList<Categoria>> ObterCategoriasAsync()
        {
            return await _cache.GetOrCreateAsync("categorias", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuracao;
                entry.SlidingExpiration = TimeSpan.FromMinutes(2);
                entry.Size = 1;
                return await _repository.ObterCategoriasAsync();
            }) ?? new List<Categoria>();
        }

        public async Task<Tag> CriarTagAsync(NovaTagRequest request)
        {
            await GarantirContaAsync(request.ContaId);

            var tag = new Tag
            {
                ContaId = request.ContaId,
                Nome = request.Nome
            };

            _repository.AdicionarTag(tag);
            await _repository.SalvarAsync();
            _cache.Remove($"tags:{request.ContaId}");
            return tag;
        }

        public async Task<IReadOnlyList<Tag>> ObterTagsAsync(int contaId)
        {
            await GarantirContaAsync(contaId);
            var cacheKey = $"tags:{contaId}";
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuracao;
                entry.SlidingExpiration = TimeSpan.FromMinutes(2);
                entry.Size = 1;
                return await _repository.ObterTagsContaAsync(contaId);
            }) ?? new List<Tag>();
        }

        public async Task<TransacaoTag> VincularTagAsync(AdicionarTagTransacaoRequest request)
        {
            var transacao = await _repository.ObterTransacaoAsync(request.TransacaoId);
            if (transacao == null)
            {
                throw new InvalidOperationException("Transação não encontrada.");
            }

            var tag = await _repository.ObterTagAsync(request.TagId);
            if (tag == null)
            {
                throw new InvalidOperationException("Tag não encontrada.");
            }

            if (tag.ContaId != transacao.ContaId)
            {
                throw new InvalidOperationException("Tag não pertence à conta.");
            }

            // Verificar se a tag já está vinculada à transação
            if (await _repository.TransacaoTagExisteAsync(request.TransacaoId, request.TagId))
            {
                throw new InvalidOperationException("Tag já está vinculada à transação.");
            }

            var vinculo = new TransacaoTag
            {
                TransacaoId = request.TransacaoId,
                TagId = request.TagId
            };

            _repository.AdicionarTransacaoTag(vinculo);
            await _repository.SalvarAsync();
            return vinculo;
        }

        public async Task<AnexoTransacao> AdicionarAnexoAsync(NovoAnexoTransacaoRequest request)
        {
            var transacao = await _repository.ObterTransacaoAsync(request.TransacaoId);
            if (transacao == null)
            {
                throw new InvalidOperationException("Transação não encontrada.");
            }

            var anexo = new AnexoTransacao
            {
                TransacaoId = request.TransacaoId,
                Tipo = request.Tipo,
                ConteudoTexto = request.ConteudoTexto,
                Url = request.Url
            };

            _repository.AdicionarAnexo(anexo);
            await _repository.SalvarAsync();
            _contaCacheService.IncrementarVersao(transacao.ContaId);
            return anexo;
        }

        public async Task<MetaFinanceira> CriarMetaAsync(NovaMetaRequest request)
        {
            await GarantirContaAsync(request.ContaId);
            ValidarValor(request.ValorAlvo);

            var meta = new MetaFinanceira
            {
                ContaId = request.ContaId,
                Nome = request.Nome,
                ValorAlvo = request.ValorAlvo,
                ValorAtual = request.ValorAtual,
                DataInicio = request.DataInicio,
                DataFim = request.DataFim,
                CategoriaId = request.CategoriaId
            };

            _repository.AdicionarMeta(meta);
            await _repository.SalvarAsync();
            return meta;
        }

        public async Task<IReadOnlyList<MetaFinanceira>> ObterMetasAsync(int contaId)
        {
            await GarantirContaAsync(contaId);
            return await _repository.ObterMetasAsync(contaId);
        }

        public async Task<MetaFinanceira> ContribuirMetaAsync(int metaId, ContribuirMetaRequest request)
        {
            var meta = await _repository.ObterMetaPorIdAsync(metaId);
            if (meta == null)
            {
                throw new InvalidOperationException("Meta não encontrada.");
            }

            // Verificar se a meta pertence ao usuário (através da conta)
            await GarantirContaAsync(meta.ContaId);

            meta.ValorAtual += request.Valor;
            _repository.AtualizarMeta(meta);
            await _repository.SalvarAsync();

            // Invalidar cache da conta
            _contaCacheService.IncrementarVersao(meta.ContaId);

            return meta;
        }

        public async Task<MetaFinanceira> AtualizarMetaAsync(int metaId, NovaMetaRequest request)
        {
            await GarantirContaAsync(request.ContaId);
            ValidarValor(request.ValorAlvo);

            var meta = await _repository.ObterMetaPorIdAsync(metaId);
            if (meta == null)
            {
                throw new InvalidOperationException("Meta não encontrada.");
            }

            if (meta.ContaId != request.ContaId)
            {
                throw new InvalidOperationException("Meta não pertence à conta informada.");
            }

            meta.Nome = request.Nome;
            meta.ValorAlvo = request.ValorAlvo;
            meta.ValorAtual = request.ValorAtual;
            meta.DataInicio = request.DataInicio;
            meta.DataFim = request.DataFim;
            meta.CategoriaId = request.CategoriaId;

            _repository.AtualizarMeta(meta);
            await _repository.SalvarAsync();
            _contaCacheService.IncrementarVersao(meta.ContaId);
            return meta;
        }

        public async Task RemoverMetaAsync(int metaId)
        {
            var meta = await _repository.ObterMetaPorIdAsync(metaId);
            if (meta == null)
            {
                throw new InvalidOperationException("Meta não encontrada.");
            }

            await GarantirContaAsync(meta.ContaId);
            _repository.RemoverMeta(meta);
            await _repository.SalvarAsync();
            _contaCacheService.IncrementarVersao(meta.ContaId);
        }

        public async Task<InsightsResponse> ObterInsightsAsync(InsightsQueryRequest request)
        {
            await GarantirContaAsync(request.ContaId);

            var mesesHistorico = Math.Clamp(request.MesesHistorico, 3, 24);
            var versao = _contaCacheService.ObterVersao(request.ContaId);
            var cacheKey = $"insights:{request.ContaId}:{mesesHistorico}:v{versao}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuracao;
                entry.SlidingExpiration = TimeSpan.FromMinutes(2);
                entry.Size = 1;

                var agora = DateTime.UtcNow;
                var mesReferencia = new DateTime(agora.Year, agora.Month, 1);
                var inicio = mesReferencia.AddMonths(-(mesesHistorico - 1));
                var fim = mesReferencia.AddMonths(1).AddTicks(-1);

                var transacoes = await _repository.ObterTransacoesPeriodoAsync(request.ContaId, inicio, fim);
                var saidas = transacoes.Where(t => t.Tipo == TipoMovimento.Saida).ToList();

                var totaisPorMes = saidas
                    .GroupBy(t => new { t.DataTransacao.Year, t.DataTransacao.Month })
                    .ToDictionary(g => (g.Key.Year, g.Key.Month), g => new
                    {
                        Total = g.Sum(t => t.Valor),
                        Quantidade = g.Count()
                    });

                var meses = new List<ResumoGastoMensal>();
                for (var i = 0; i < mesesHistorico; i++)
                {
                    var mesAtual = inicio.AddMonths(i);
                    totaisPorMes.TryGetValue((mesAtual.Year, mesAtual.Month), out var info);
                    meses.Add(new ResumoGastoMensal
                    {
                        InicioMes = mesAtual,
                        TotalSaidas = info?.Total ?? 0m,
                        QuantidadeSaidas = info?.Quantidade ?? 0
                    });
                }

                var totalAtual = meses.LastOrDefault()?.TotalSaidas ?? 0m;
                var totalAnterior = meses.Count > 1 ? meses[^2].TotalSaidas : 0m;
                var variacao = totalAnterior > 0 ? (totalAtual - totalAnterior) / totalAnterior : 0m;

                var saidasMesAtual = saidas.Where(t => t.DataTransacao.Year == mesReferencia.Year && t.DataTransacao.Month == mesReferencia.Month).ToList();
                var topCategorias = saidasMesAtual
                    .GroupBy(t => new { t.CategoriaId, Nome = t.Categoria?.Nome ?? "Sem categoria" })
                    .Select(g => new InsightCategoriaResponse
                    {
                        CategoriaId = g.Key.CategoriaId,
                        CategoriaNome = g.Key.Nome,
                        Total = g.Sum(t => t.Valor),
                        Percentual = totalAtual > 0 ? g.Sum(t => t.Valor) / totalAtual : 0m
                    })
                    .OrderByDescending(c => c.Total)
                    .Take(3)
                    .ToList();

                var (previsao, confianca, modelo) = PreverProximoMes(meses);

                var sugestoes = GerarSugestoes(totalAtual, totalAnterior, variacao, previsao, topCategorias);

                return new InsightsResponse
                {
                    ContaId = request.ContaId,
                    MesReferencia = mesReferencia.Month,
                    AnoReferencia = mesReferencia.Year,
                    TotalSaidasMesAtual = totalAtual,
                    TotalSaidasMesAnterior = totalAnterior,
                    VariacaoPercentual = variacao,
                    PrevisaoSaidasProximoMes = previsao,
                    Modelo = modelo,
                    Confianca = confianca,
                    TopCategorias = topCategorias,
                    Sugestoes = sugestoes
                };
            }) ?? new InsightsResponse { ContaId = request.ContaId };
        }

        public async Task<decimal> ObterSaldoAtualAsync(int contaId)
        {
            await GarantirContaAsync(contaId);

            var versao = _contaCacheService.ObterVersao(contaId);
            var cacheKey = $"saldo:{contaId}:v{versao}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuracao;
                entry.SlidingExpiration = TimeSpan.FromMinutes(1);
                entry.Size = 1;

                var saldoInicial = await _contaRepository.ObterSaldoInicialAsync(contaId);
                var transacoes = await _repository.ObterTodasTransacoesAsync(contaId);

                var entradas = transacoes.Where(t => t.Tipo == TipoMovimento.Entrada).Sum(t => t.Valor);
                var saidas = transacoes.Where(t => t.Tipo == TipoMovimento.Saida).Sum(t => t.Valor);

                return saldoInicial + entradas - saidas;
            });
        }

        private static (decimal previsao, decimal confianca, string modelo) PreverProximoMes(IReadOnlyList<ResumoGastoMensal> meses)
        {
            if (meses.Count < 4)
            {
                return (0m, 0m, "indisponivel");
            }

            var ultima = meses[^1];
            var ultimasTres = meses.Skip(Math.Max(0, meses.Count - 3)).ToList();
            var mediaTres = ultimasTres.Average(m => m.TotalSaidas);

            var dados = new List<GastoMensalData>();
            for (var i = 3; i < meses.Count; i++)
            {
                var anterior = meses[i - 1];
                var anterior2 = meses[i - 2];
                var anterior3 = meses[i - 3];
                var media = (anterior.TotalSaidas + anterior2.TotalSaidas + anterior3.TotalSaidas) / 3m;

                dados.Add(new GastoMensalData
                {
                    MesIndex = i,
                    Mes = meses[i].InicioMes.Month,
                    TotalMesAnterior = (float)anterior.TotalSaidas,
                    MediaTresMeses = (float)media,
                    TotalTransacoesAnterior = anterior.QuantidadeSaidas,
                    Label = (float)meses[i].TotalSaidas
                });
            }

            if (dados.Count < 4)
            {
                return (mediaTres, 0.2m, "baseline");
            }

            var mlContext = new MLContext(seed: 42);
            var dataView = mlContext.Data.LoadFromEnumerable(dados);
            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            var pipeline = mlContext.Transforms.Concatenate(
                    "Features",
                    nameof(GastoMensalData.MesIndex),
                    nameof(GastoMensalData.Mes),
                    nameof(GastoMensalData.TotalMesAnterior),
                    nameof(GastoMensalData.MediaTresMeses),
                    nameof(GastoMensalData.TotalTransacoesAnterior))
                .Append(mlContext.Regression.Trainers.Sdca(
                    labelColumnName: nameof(GastoMensalData.Label),
                    featureColumnName: "Features"));

            var model = pipeline.Fit(split.TrainSet);
            var predictions = model.Transform(split.TestSet);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: nameof(GastoMensalData.Label));

            var engine = mlContext.Model.CreatePredictionEngine<GastoMensalData, GastoMensalPrediction>(model);
            var proximoMes = ultima.InicioMes.AddMonths(1);
            var entrada = new GastoMensalData
            {
                MesIndex = meses.Count,
                Mes = proximoMes.Month,
                TotalMesAnterior = (float)ultima.TotalSaidas,
                MediaTresMeses = (float)mediaTres,
                TotalTransacoesAnterior = ultima.QuantidadeSaidas
            };

            var score = engine.Predict(entrada).Score;
            var previsao = Math.Max(0m, (decimal)score);
            var confianca = (decimal)Math.Clamp(metrics.RSquared, 0, 1);

            return (previsao, confianca, "mlnet-sdca");
        }

        private static IReadOnlyList<string> GerarSugestoes(decimal totalAtual, decimal totalAnterior, decimal variacao, decimal previsao, IReadOnlyList<InsightCategoriaResponse> topCategorias)
        {
            var sugestoes = new List<string>();

            if (totalAtual == 0m)
            {
                sugestoes.Add("Sem gastos registrados neste mês. Registre suas transações para gerar recomendações.");
                return sugestoes;
            }

            if (variacao >= 0.2m)
            {
                sugestoes.Add($"Seus gastos subiram {variacao:P0} em relação ao mês anterior.");
            }
            else if (variacao <= -0.2m)
            {
                sugestoes.Add($"Parabéns! Seus gastos reduziram {Math.Abs(variacao):P0} em relação ao mês anterior.");
            }

            var principal = topCategorias.FirstOrDefault();
            if (principal != null && principal.Percentual >= 0.4m)
            {
                sugestoes.Add($"A categoria {principal.CategoriaNome} concentra {principal.Percentual:P0} dos seus gastos. Avalie oportunidades de redução.");
            }

            if (previsao > totalAtual * 1.1m)
            {
                sugestoes.Add("A previsão indica aumento de gastos no próximo mês. Considere revisar seu orçamento.");
            }

            if (sugestoes.Count == 0)
            {
                sugestoes.Add("Seus gastos estão estáveis. Continue acompanhando suas categorias principais.");
            }

            return sugestoes;
        }

        private sealed class ResumoGastoMensal
        {
            public DateTime InicioMes { get; set; }
            public decimal TotalSaidas { get; set; }
            public int QuantidadeSaidas { get; set; }
        }

        private sealed class GastoMensalData
        {
            [LoadColumn(0)]
            public float MesIndex { get; set; }

            [LoadColumn(1)]
            public float Mes { get; set; }

            [LoadColumn(2)]
            public float TotalMesAnterior { get; set; }

            [LoadColumn(3)]
            public float MediaTresMeses { get; set; }

            [LoadColumn(4)]
            public float TotalTransacoesAnterior { get; set; }

            [LoadColumn(5)]
            public float Label { get; set; }
        }

        private sealed class GastoMensalPrediction
        {
            [ColumnName("Score")]
            public float Score { get; set; }
        }

        private async Task GarantirContaAsync(int contaId)
        {
            var existe = await _repository.ContaExisteAsync(contaId);
            if (!existe)
            {
                throw new InvalidOperationException("Conta não encontrada.");
            }
        }

        private static void ValidarValor(decimal valor)
        {
            if (valor <= 0)
            {
                throw new InvalidOperationException("Valor deve ser maior que zero.");
            }
        }

        private static DateTime? ParseDateUtc(string? value, bool isFim)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (!DateTime.TryParse(value, out var dt))
            {
                return null;
            }

            var data = dt.Date;
            if (isFim)
            {
                data = data.AddDays(1).AddTicks(-1);
            }

            return DateTime.SpecifyKind(data, DateTimeKind.Utc);
        }

        private static TransacaoResponse MapToTransacaoResponse(Transacao t, string contaNome)
        {
            return new TransacaoResponse
            {
                Id = t.Id,
                Valor = t.Valor,
                Tipo = t.Tipo,
                Moeda = t.Moeda,
                DataTransacao = t.DataTransacao,
                CategoriaId = t.CategoriaId,
                CategoriaNome = t.Categoria?.Nome ?? string.Empty,
                Descricao = t.Descricao,
                ContaId = t.ContaId,
                ContaNome = contaNome,
                Tags = t.Tags?.Select(tt => new TagResponse { Id = tt.TagId, Nome = tt.Tag?.Nome ?? string.Empty }).ToList() ?? new List<TagResponse>(),
                Anexos = t.Anexos?.Select(a => new AnexoResponse { Id = a.Id, Tipo = a.Tipo, ConteudoTexto = a.ConteudoTexto, Url = a.Url }).ToList() ?? new List<AnexoResponse>()
            };
        }

        private static int ContarOcorrenciasNoMes(DateTime dataInicio, int intervaloQuantidade, IntervaloUnidade intervaloUnidade, int mes, int ano)
        {
            if (intervaloQuantidade <= 0)
            {
                return 0;
            }

            var inicioMes = new DateTime(ano, mes, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            if (dataInicio.Date > fimMes)
            {
                return 0;
            }

            var count = 0;

            if (intervaloUnidade == IntervaloUnidade.Dia)
            {
                var diasIntervalo = intervaloQuantidade;
                var diasDesdeInicio = (inicioMes - dataInicio.Date).Days;
                if (diasDesdeInicio < 0)
                {
                    diasDesdeInicio = 0;
                }

                var offset = diasDesdeInicio % diasIntervalo;
                var primeira = inicioMes.AddDays(offset == 0 ? 0 : diasIntervalo - offset);
                if (primeira < dataInicio.Date)
                {
                    primeira = dataInicio.Date;
                }

                for (var data = primeira; data <= fimMes; data = data.AddDays(diasIntervalo))
                {
                    if (data >= inicioMes)
                    {
                        count++;
                    }
                }

                return count;
            }

            var mesesIntervalo = intervaloQuantidade;
            var mesesDiff = (inicioMes.Year - dataInicio.Year) * 12 + (inicioMes.Month - dataInicio.Month);
            if (mesesDiff < 0)
            {
                mesesDiff = 0;
            }

            var passos = mesesDiff / mesesIntervalo;
            var primeiraMes = dataInicio.AddMonths(passos * mesesIntervalo);
            if (primeiraMes < inicioMes)
            {
                primeiraMes = primeiraMes.AddMonths(mesesIntervalo);
            }

            for (var data = primeiraMes; data <= fimMes; data = data.AddMonths(mesesIntervalo))
            {
                if (data >= inicioMes)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
