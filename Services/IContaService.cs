using PraOndeFoi.DTOs;
using PraOndeFoi.Models;

namespace PraOndeFoi.Services
{
    public interface IContaService
    {
        Task<Conta> CriarContaAsync(CriarContaRequest request);
        Task<ContaResponse?> ObterContaAsync(int contaId);
    }
}
