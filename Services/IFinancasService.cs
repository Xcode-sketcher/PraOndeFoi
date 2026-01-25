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
        Task<ResumoMensalResponse> ObterResumoMensalAsync(int contaId, int mes, int ano);
        Task<IReadOnlyList<OrcamentoStatusResponse>> ObterStatusOrcamentosAsync(int contaId, int mes, int ano);
        Task<IReadOnlyList<TransacaoResponse>> ObterTransacoesAsync(int contaId, TipoMovimento? tipo, int? categoriaId, DateTime? inicio, DateTime? fim);
        Task<IReadOnlyList<Categoria>> ObterCategoriasAsync();
        Task<Tag> CriarTagAsync(NovaTagRequest request);
        Task<IReadOnlyList<Tag>> ObterTagsAsync(int contaId);
        Task<TransacaoTag> VincularTagAsync(AdicionarTagTransacaoRequest request);
        Task<AnexoTransacao> AdicionarAnexoAsync(NovoAnexoTransacaoRequest request);
        Task<MetaFinanceira> CriarMetaAsync(NovaMetaRequest request);
        Task<IReadOnlyList<MetaFinanceira>> ObterMetasAsync(int contaId);
        Task<decimal> ObterSaldoAtualAsync(int contaId);
    }
}
