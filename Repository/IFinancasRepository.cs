using PraOndeFoi.Models;

namespace PraOndeFoi.Repository
{
    public interface IFinancasRepository
    {
        Task<bool> ContaExisteAsync(int contaId);
        void AdicionarTransacao(Transacao transacao);
        void AdicionarRecorrencia(Recorrencia recorrencia);
        void AdicionarAssinatura(Assinatura assinatura);
        void AdicionarOrcamento(OrcamentoMensal orcamento);
        Task<int> SalvarAsync();

        Task<List<Transacao>> ObterTransacoesMesAsync(int contaId, int mes, int ano);
        Task<List<Transacao>> ObterTransacoesFiltradasAsync(int contaId, TipoMovimento? tipo, int? categoriaId, DateTime? inicio, DateTime? fim);
        Task<List<Recorrencia>> ObterRecorrenciasAtivasAsync(int contaId);
        Task<List<Assinatura>> ObterAssinaturasAtivasAsync(int contaId);
        Task<List<Recorrencia>> ObterRecorrenciasVencidasAsync(DateTime utcAgora);
        Task<List<Assinatura>> ObterAssinaturasVencidasAsync(DateTime utcAgora);
        Task<List<OrcamentoMensal>> ObterOrcamentosMesAsync(int contaId, int mes, int ano);
        Task<List<(int CategoriaId, decimal Total)>> ObterGastosPorCategoriaAsync(int contaId, int mes, int ano);
        Task<List<Categoria>> ObterCategoriasAsync();
        Task<Tag?> ObterTagAsync(int tagId);
        Task<Transacao?> ObterTransacaoAsync(int transacaoId);
        void AdicionarTag(Tag tag);
        void AdicionarTransacaoTag(TransacaoTag transacaoTag);
        void AdicionarAnexo(AnexoTransacao anexo);
        void AdicionarMeta(MetaFinanceira meta);
        Task<List<Tag>> ObterTagsContaAsync(int contaId);
        Task<List<MetaFinanceira>> ObterMetasAsync(int contaId);
        Task<bool> TransacaoTagExisteAsync(int transacaoId, int tagId);
        Task<List<Transacao>> ObterTodasTransacoesAsync(int contaId);
        Task<List<Transacao>> ObterTransacoesParaExportacaoAsync(int contaId, DateTime? inicio, DateTime? fim);
    }
}
