using Microsoft.Extensions.Caching.Memory;

namespace PraOndeFoi.Services
{
    public class ContaCacheService : IContaCacheService
    {
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan VersaoExpiracao = TimeSpan.FromHours(12);

        public ContaCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public int ObterVersao(int contaId)
        {
            var key = ObterChaveVersao(contaId);
            if (_cache.TryGetValue(key, out int versao))
            {
                return versao;
            }

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = VersaoExpiracao,
                Size = 1
            };
            _cache.Set(key, 0, options);
            return 0;
        }

        public int IncrementarVersao(int contaId)
        {
            var key = ObterChaveVersao(contaId);
            var versaoAtual = ObterVersao(contaId);
            var novaVersao = versaoAtual + 1;
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = VersaoExpiracao,
                Size = 1
            };
            _cache.Set(key, novaVersao, options);
            return novaVersao;
        }

        public MemoryCacheEntryOptions CriarOpcoesCurta()
        {
            return new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3),
                SlidingExpiration = TimeSpan.FromMinutes(1),
                Size = 1
            };
        }

        public MemoryCacheEntryOptions CriarOpcoesMedia()
        {
            return new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(3),
                Size = 1
            };
        }

        private static string ObterChaveVersao(int contaId) => $"cache:conta:{contaId}:versao";
    }
}
