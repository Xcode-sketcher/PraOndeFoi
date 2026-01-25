using Supabase;
using Microsoft.Extensions.Configuration;

namespace PraOndeFoi.Services
{
    public class SupabaseAnexoStorageService : IAnexoStorageService
    {
        private readonly Client _client;
        private readonly string _bucket;
        private readonly bool _publicBucket;
        private readonly int _signedUrlExpiraEm;

        public SupabaseAnexoStorageService(Client client, IConfiguration configuration)
        {
            _client = client;
            _bucket = configuration["Supabase:StorageBucket"] ?? "anexos";
            _publicBucket = bool.TryParse(configuration["Supabase:StoragePublic"], out var publico) && publico;
            _signedUrlExpiraEm = int.TryParse(configuration["Supabase:SignedUrlExpirySeconds"], out var segundos) ? segundos : 3600;
        }

        public async Task<string> UploadAsync(int contaId, int transacaoId, string arquivoNome, Stream conteudo, string contentType, CancellationToken cancellationToken = default)
        {
            var ext = Path.GetExtension(arquivoNome);
            var safeName = Path.GetFileNameWithoutExtension(arquivoNome);
            if (string.IsNullOrWhiteSpace(safeName))
            {
                safeName = "arquivo";
            }

            var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
            var objectPath = $"contas/{contaId}/transacoes/{transacaoId}/{fileName}";

            var tempPath = Path.Combine(Path.GetTempPath(), fileName);
            await using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await conteudo.CopyToAsync(fileStream, cancellationToken);
            }

            try
            {
                var bucket = _client.Storage.From(_bucket);
                await bucket.Upload(tempPath, objectPath);

                if (_publicBucket)
                {
                    return bucket.GetPublicUrl(objectPath);
                }

                var signed = await bucket.CreateSignedUrl(objectPath, _signedUrlExpiraEm);
                return signed;
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }
    }
}
