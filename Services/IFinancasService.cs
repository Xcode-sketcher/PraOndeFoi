using PraOndeFoi.DTOs;
using PraOndeFoi.Models;

namespace PraOndeFoi.Services
{
    public interface IFinancasService
    {
        Task<Transacao> CriarTransacaoAsync(NovaTransacaoRequest request);
        Task<Recorrencia> CriarRecorrenciaAsync(NovaRecorrenciaRequest request);
        Task<Assinatura> CriarAssinaturaAsync(NovaAssinaturaRequest request);
        Task<OrcamentoMensal> CriarOrcamentoAsync(NovoOrcamentoRequest request);
        Task<OrcamentoMensal> AtualizarOrcamentoAsync(int orcamentoId, NovoOrcamentoRequest request);
        Task RemoverOrcamentoAsync(int orcamentoId);
        Task<ResumoMensalResponse> ObterResumoMensalAsync(int contaId, int mes, int ano);
        Task<IReadOnlyList<OrcamentoStatusResponse>> ObterStatusOrcamentosAsync(int contaId, int mes, int ano);

        // Retorna orçamentos do mês (inclui Id para permitir deleção/edição no cliente)
        Task<IReadOnlyList<OrcamentoResponse>> ObterOrcamentosAsync(int contaId, int mes, int ano);
        Task<OrcamentoAnaliseResponse> ObterAnaliseOrcamentosAsync(OrcamentoAnaliseQueryRequest request);
        Task<PagedResponse<TransacaoResponse>> ObterTransacoesAsync(TransacaoQueryRequest request);
        Task<Transacao> AtualizarTransacaoAsync(int transacaoId, NovaTransacaoRequest request);
        Task RemoverTransacaoAsync(int transacaoId);
        Task<IReadOnlyList<Categoria>> ObterCategoriasAsync();
        Task<Tag> CriarTagAsync(NovaTagRequest request);
        Task<IReadOnlyList<Tag>> ObterTagsAsync(int contaId);
        Task<TransacaoTag> VincularTagAsync(AdicionarTagTransacaoRequest request);
        Task<AnexoTransacao> AdicionarAnexoAsync(NovoAnexoTransacaoRequest request);
        Task<MetaFinanceira> CriarMetaAsync(NovaMetaRequest request);
        Task<IReadOnlyList<MetaFinanceira>> ObterMetasAsync(int contaId);
        Task<MetaFinanceira> AtualizarMetaAsync(int metaId, NovaMetaRequest request);
        Task RemoverMetaAsync(int metaId);
        Task<MetaFinanceira> ContribuirMetaAsync(int metaId, ContribuirMetaRequest request);
        Task<decimal> ObterSaldoAtualAsync(int contaId);
        Task<InsightsResponse> ObterInsightsAsync(InsightsQueryRequest request);
    }
}
