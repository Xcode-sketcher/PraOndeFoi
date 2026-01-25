using PraOndeFoi.DTOs;
using PraOndeFoi.Models;
using PraOndeFoi.Repository;

namespace PraOndeFoi.Services
{
    public class ContaService : IContaService
    {
        private readonly IContaRepository _repository;

        public ContaService(IContaRepository repository)
        {
            _repository = repository;
        }

        public async Task<Conta> CriarContaAsync(CriarContaRequest request)
        {
            if (request.SaldoInicial < 0)
            {
                throw new InvalidOperationException("Saldo inicial não pode ser negativo.");
            }

            var contaExistente = await _repository.ObterContaPorUsuarioAsync(request.UsuarioId);

            if (contaExistente != null)
            {
                throw new InvalidOperationException("Usuário já possui uma conta.");
            }

            var usuario = await _repository.ObterUsuarioAsync(request.UsuarioId);
            if (usuario == null)
            {
                usuario = new Usuario
                {
                    Id = request.UsuarioId,
                    Nome = request.Nome,
                    Email = request.Email
                };
                _repository.AdicionarUsuario(usuario);
            }

            var conta = new Conta
            {
                Moeda = request.Moeda,
                Saldo = request.SaldoInicial,
                UsuarioId = usuario.Id,
                Usuario = usuario
            };

            _repository.AdicionarConta(conta);
            await _repository.SalvarAsync();

            return conta;
        }

        public async Task<ContaResponse?> ObterContaAsync(int contaId)
        {
            var conta = await _repository.ObterContaAsync(contaId);
            if (conta == null)
            {
                return null;
            }

            var transacoes = conta.Transacoes?.Select(t => MapToTransacaoResponse(t, "Conta")).ToList() ?? new List<TransacaoResponse>();

            return new ContaResponse
            {
                Id = conta.Id,
                Saldo = conta.Saldo,
                UsuarioId = conta.UsuarioId,
                UsuarioNome = conta.Usuario?.Nome ?? string.Empty,
                Transacoes = transacoes
            };
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
    }
}
