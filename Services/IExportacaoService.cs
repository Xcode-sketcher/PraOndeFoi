using PraOndeFoi.Models;

namespace PraOndeFoi.Services
{
    public interface IExportacaoService
    {
        Task<byte[]> ExportarTransacoesCsvAsync(int contaId, DateTime? inicio, DateTime? fim);
        Task<byte[]> ExportarTransacoesPdfAsync(int contaId, DateTime? inicio, DateTime? fim);
    }
}
