using Microsoft.EntityFrameworkCore;
using PraOndeFoi.Data;
using PraOndeFoi.Models;

namespace PraOndeFoi.Repository
{
    public class ContaRepository : IContaRepository
    {
        private readonly AppDbContext _db;

        public ContaRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<Usuario?> ObterUsuarioAsync(string usuarioId)
        {
            return _db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
        }

        public Task<Conta?> ObterContaAsync(int contaId)
        {
            return _db.Contas
                .Include(c => c.Usuario)
                .Include(c => c.Transacoes)
                    .ThenInclude(t => t.Categoria)
                .Include(c => c.Transacoes)
                    .ThenInclude(t => t.Tags)
                        .ThenInclude(tt => tt.Tag)
                .Include(c => c.Transacoes)
                    .ThenInclude(t => t.Anexos)
                .FirstOrDefaultAsync(c => c.Id == contaId);
        }

        public Task<Conta?> ObterContaPorUsuarioAsync(string usuarioId)
        {
            return _db.Contas.AsNoTracking().FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
        }

        public void AdicionarUsuario(Usuario usuario)
        {
            _db.Usuarios.Add(usuario);
        }

        public void AdicionarConta(Conta conta)
        {
            _db.Contas.Add(conta);
        }

        public Task<decimal> ObterSaldoInicialAsync(int contaId)
        {
            return _db.Contas.Where(c => c.Id == contaId).Select(c => c.Saldo).FirstOrDefaultAsync();
        }

        public Task<int> SalvarAsync()
        {
            return _db.SaveChangesAsync();
        }
    }
}
