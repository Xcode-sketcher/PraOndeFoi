using PraOndeFoi.DTOs;

namespace PraOndeFoi.Services
{
    public interface IImportacaoService
    {
        Task<ImportacaoResultadoResponse> ImportarTransacoesCsvAsync(int contaId, Stream csvStream);
    }
}
