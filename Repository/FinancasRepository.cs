using Microsoft.EntityFrameworkCore;
using PraOndeFoi.Data;
using PraOndeFoi.Models;

namespace PraOndeFoi.Repository
{
    public class FinancasRepository : IFinancasRepository
    {
        private readonly AppDbContext _db;

        public FinancasRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<bool> ContaExisteAsync(int contaId)
        {
            return _db.Contas.AnyAsync(c => c.Id == contaId);
        }

        public void AdicionarTransacao(Transacao transacao)
        {
            _db.Transacoes.Add(transacao);
        }

        public void AdicionarRecorrencia(Recorrencia recorrencia)
        {
            _db.Recorrencias.Add(recorrencia);
        }

        public void AdicionarAssinatura(Assinatura assinatura)
        {
            _db.Assinaturas.Add(assinatura);
        }

        public void AdicionarOrcamento(OrcamentoMensal orcamento)
        {
            _db.OrcamentosMensais.Add(orcamento);
        }

        public Task<int> SalvarAsync()
        {
            return _db.SaveChangesAsync();
        }

        public Task<List<Transacao>> ObterTransacoesMesAsync(int contaId, int mes, int ano)
        {
            return _db.Transacoes
                .AsNoTracking()
                .Where(t => t.ContaId == contaId && t.DataTransacao.Month == mes && t.DataTransacao.Year == ano)
                .ToListAsync();
        }

        public Task<List<Transacao>> ObterTransacoesFiltradasAsync(int contaId, TipoMovimento? tipo, int? categoriaId, DateTime? inicio, DateTime? fim)
        {
            var query = _db.Transacoes
                .AsNoTracking()
                .Include(t => t.Conta)
                .Include(t => t.Categoria)
                .Include(t => t.Tags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Anexos)
                .Where(t => t.ContaId == contaId);

            if (tipo.HasValue)
            {
                query = query.Where(t => t.Tipo == tipo.Value);
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(t => t.CategoriaId == categoriaId.Value);
            }

            if (inicio.HasValue)
            {
                query = query.Where(t => t.DataTransacao >= inicio.Value);
            }

            if (fim.HasValue)
            {
                query = query.Where(t => t.DataTransacao <= fim.Value);
            }

            return query.OrderByDescending(t => t.DataTransacao).ToListAsync();
        }

        public Task<List<Recorrencia>> ObterRecorrenciasAtivasAsync(int contaId)
        {
            return _db.Recorrencias
                .AsNoTracking()
                .Where(r => r.ContaId == contaId && r.Ativa)
                .ToListAsync();
        }

        public Task<List<Assinatura>> ObterAssinaturasAtivasAsync(int contaId)
        {
            return _db.Assinaturas
                .AsNoTracking()
                .Where(a => a.ContaId == contaId && a.Ativa)
                .ToListAsync();
        }

        public Task<List<Recorrencia>> ObterRecorrenciasVencidasAsync(DateTime utcAgora)
        {
            return _db.Recorrencias
                .Where(r => r.Ativa && ((r.ProximaExecucao != null && r.ProximaExecucao <= utcAgora) || (r.ProximaExecucao == null && r.DataInicio <= utcAgora)))
                .ToListAsync();
        }

        public Task<List<Assinatura>> ObterAssinaturasVencidasAsync(DateTime utcAgora)
        {
            return _db.Assinaturas
                .Where(a => a.Ativa && ((a.ProximaCobranca != null && a.ProximaCobranca <= utcAgora) || (a.ProximaCobranca == null && a.DataInicio <= utcAgora)))
                .ToListAsync();
        }

        public Task<List<OrcamentoMensal>> ObterOrcamentosMesAsync(int contaId, int mes, int ano)
        {
            return _db.OrcamentosMensais
                .AsNoTracking()
                .Where(o => o.ContaId == contaId && o.Mes == mes && o.Ano == ano)
                .ToListAsync();
        }

        public async Task<List<(int CategoriaId, decimal Total)>> ObterGastosPorCategoriaAsync(int contaId, int mes, int ano)
        {
            var resultado = await _db.Transacoes
                .AsNoTracking()
                .Where(t => t.ContaId == contaId && t.DataTransacao.Month == mes && t.DataTransacao.Year == ano && t.Tipo == TipoMovimento.Saida)
                .GroupBy(t => t.CategoriaId)
                .Select(g => new { CategoriaId = g.Key, Total = g.Sum(t => t.Valor) })
                .ToListAsync();

            return resultado.Select(r => (r.CategoriaId, r.Total)).ToList();
        }

        public Task<List<Categoria>> ObterCategoriasAsync()
        {
            return _db.Categorias.AsNoTracking().OrderBy(c => c.Nome).ToListAsync();
        }

        public Task<Tag?> ObterTagAsync(int tagId)
        {
            return _db.Tags.FirstOrDefaultAsync(t => t.Id == tagId);
        }

        public Task<Transacao?> ObterTransacaoAsync(int transacaoId)
        {
            return _db.Transacoes
                .Include(t => t.Conta)
                .Include(t => t.Tags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Anexos)
                .FirstOrDefaultAsync(t => t.Id == transacaoId);
        }

        public void AdicionarTag(Tag tag)
        {
            _db.Tags.Add(tag);
        }

        public void AdicionarTransacaoTag(TransacaoTag transacaoTag)
        {
            _db.TransacaoTags.Add(transacaoTag);
        }

        public void AdicionarAnexo(AnexoTransacao anexo)
        {
            _db.AnexosTransacao.Add(anexo);
        }

        public void AdicionarMeta(MetaFinanceira meta)
        {
            _db.MetasFinanceiras.Add(meta);
        }

        public Task<List<Tag>> ObterTagsContaAsync(int contaId)
        {
            return _db.Tags.AsNoTracking().Where(t => t.ContaId == contaId).OrderBy(t => t.Nome).ToListAsync();
        }

        public Task<List<MetaFinanceira>> ObterMetasAsync(int contaId)
        {
            return _db.MetasFinanceiras.AsNoTracking().Where(m => m.ContaId == contaId).OrderBy(m => m.DataInicio).ToListAsync();
        }

        public Task<List<Transacao>> ObterTodasTransacoesAsync(int contaId)
        {
            return _db.Transacoes
                .AsNoTracking()
                .Include(t => t.Conta)
                .Include(t => t.Categoria)
                .Include(t => t.Tags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Anexos)
                .Where(t => t.ContaId == contaId)
                .ToListAsync();
        }

        public Task<List<Transacao>> ObterTransacoesParaExportacaoAsync(int contaId, DateTime? inicio, DateTime? fim)
        {
            var query = _db.Transacoes
                .AsNoTracking()
                .Include(t => t.Categoria)
                .Where(t => t.ContaId == contaId);

            if (inicio.HasValue)
            {
                query = query.Where(t => t.DataTransacao >= inicio.Value);
            }

            if (fim.HasValue)
            {
                query = query.Where(t => t.DataTransacao <= fim.Value);
            }

            return query.OrderBy(t => t.DataTransacao).ToListAsync();
        }

        public Task<bool> TransacaoTagExisteAsync(int transacaoId, int tagId)
        {
            return _db.TransacaoTags.AnyAsync(tt => tt.TransacaoId == transacaoId && tt.TagId == tagId);
        }
    }
}
