using Microsoft.Extensions.Caching.Memory;

namespace PraOndeFoi.Services
{
    public interface IContaCacheService
    {
        int ObterVersao(int contaId);
        int IncrementarVersao(int contaId);
        MemoryCacheEntryOptions CriarOpcoesCurta();
        MemoryCacheEntryOptions CriarOpcoesMedia();
    }
}
