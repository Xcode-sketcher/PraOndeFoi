namespace PraOndeFoi.Services
{
    public interface IAnexoStorageService
    {
        Task<string> UploadAsync(int contaId, int transacaoId, string arquivoNome, Stream conteudo, string contentType, CancellationToken cancellationToken = default);
    }
}
