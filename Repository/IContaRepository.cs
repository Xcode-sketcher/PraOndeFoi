using PraOndeFoi.Models;

namespace PraOndeFoi.Repository
{
    public interface IContaRepository
    {
        Task<Usuario?> ObterUsuarioAsync(string usuarioId);
        Task<Conta?> ObterContaAsync(int contaId);
        Task<Conta?> ObterContaPorUsuarioAsync(string usuarioId);
        void AdicionarUsuario(Usuario usuario);
        void AdicionarConta(Conta conta);
        Task<decimal> ObterSaldoInicialAsync(int contaId);
        Task<int> SalvarAsync();
    }
}
