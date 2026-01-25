using Microsoft.Extensions.Caching.Memory;
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

        public async Task<IReadOnlyList<TransacaoResponse>> ObterTransacoesAsync(int contaId, TipoMovimento? tipo, int? categoriaId, DateTime? inicio, DateTime? fim)
        {
            await GarantirContaAsync(contaId);
            var transacoes = await _repository.ObterTransacoesFiltradasAsync(contaId, tipo, categoriaId, inicio, fim);
            return transacoes.Select(t => MapToTransacaoResponse(t, "Conta")).ToList();
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

            // Invalidar cache da conta
            _contaCacheService.IncrementarVersao(meta.ContaId);

            return meta;
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
