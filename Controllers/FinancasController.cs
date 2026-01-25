using Microsoft.AspNetCore.Mvc;
using PraOndeFoi.DTOs;
using PraOndeFoi.Models;
using PraOndeFoi.Services;

namespace PraOndeFoi.Controllers
{
    [ApiController]
    [Route("api/financas")]
    public class FinancasController : ControllerBase
    {
        private readonly IFinancasService _financasService;
        private readonly IExportacaoService _exportacaoService;
        private readonly IImportacaoService _importacaoService;
        private readonly IAnexoStorageService _anexoStorageService;
        private readonly ILogger<FinancasController> _logger;

        public FinancasController(IFinancasService financasService, IExportacaoService exportacaoService, IImportacaoService importacaoService, IAnexoStorageService anexoStorageService, ILogger<FinancasController> logger)
        {
            _financasService = financasService;
            _exportacaoService = exportacaoService;
            _importacaoService = importacaoService;
            _anexoStorageService = anexoStorageService;
            _logger = logger;
        }

        [HttpPost("transacoes")]
        public async Task<IActionResult> CriarTransacao([FromBody] NovaTransacaoRequest request)
        {
            try
            {
                var transacao = await _financasService.CriarTransacaoAsync(request);
                return Ok(transacao);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("transacoes/{transacaoId:int}")]
        public async Task<IActionResult> AtualizarTransacao(int transacaoId, [FromBody] NovaTransacaoRequest request)
        {
            try
            {
                var transacao = await _financasService.AtualizarTransacaoAsync(transacaoId, request);
                return Ok(transacao);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("transacoes/{transacaoId:int}")]
        public async Task<IActionResult> RemoverTransacao(int transacaoId)
        {
            try
            {
                await _financasService.RemoverTransacaoAsync(transacaoId);
                return Ok(new { message = "Transação removida com sucesso." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("recorrencias")]
        public async Task<IActionResult> CriarRecorrencia([FromBody] NovaRecorrenciaRequest request)
        {
            try
            {
                var recorrencia = await _financasService.CriarRecorrenciaAsync(request);
                return Ok(recorrencia);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("assinaturas")]
        public async Task<IActionResult> CriarAssinatura([FromBody] NovaAssinaturaRequest request)
        {
            try
            {
                var assinatura = await _financasService.CriarAssinaturaAsync(request);
                return Ok(assinatura);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("orcamentos")]
        public async Task<IActionResult> CriarOrcamento([FromBody] NovoOrcamentoRequest request)
        {
            try
            {
                var orcamento = await _financasService.CriarOrcamentoAsync(request);
                return Ok(orcamento);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("orcamentos/{orcamentoId:int}")]
        public async Task<IActionResult> AtualizarOrcamento(int orcamentoId, [FromBody] NovoOrcamentoRequest request)
        {
            try
            {
                var orcamento = await _financasService.AtualizarOrcamentoAsync(orcamentoId, request);
                return Ok(orcamento);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("orcamentos/{orcamentoId:int}")]
        public async Task<IActionResult> RemoverOrcamento(int orcamentoId)
        {
            try
            {
                await _financasService.RemoverOrcamentoAsync(orcamentoId);
                return Ok(new { message = "Orçamento removido com sucesso." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("orcamentos")]
        public async Task<IActionResult> ObterOrcamentos([FromQuery] int contaId, [FromQuery] int mes, [FromQuery] int ano)
        {
            try
            {
                var orcamentos = await _financasService.ObterOrcamentosAsync(contaId, mes, ano);
                return Ok(orcamentos);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("orcamentos/analises")]
        public async Task<IActionResult> ObterAnaliseOrcamentos([FromQuery] OrcamentoAnaliseQueryRequest request)
        {
            try
            {
                var analise = await _financasService.ObterAnaliseOrcamentosAsync(request);
                return Ok(analise);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("resumo-mensal")]
        public async Task<IActionResult> ObterResumoMensal([FromQuery] int contaId, [FromQuery] int mes, [FromQuery] int ano)
        {
            try
            {
                var resumo = await _financasService.ObterResumoMensalAsync(contaId, mes, ano);
                return Ok(resumo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("orcamentos/status")]
        public async Task<IActionResult> ObterStatusOrcamentos([FromQuery] int contaId, [FromQuery] int mes, [FromQuery] int ano)
        {
            try
            {
                var status = await _financasService.ObterStatusOrcamentosAsync(contaId, mes, ano);
                return Ok(status);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("transacoes")]
        public async Task<IActionResult> ObterTransacoes([FromQuery] TransacaoQueryRequest request)
        {
            try
            {
                var transacoes = await _financasService.ObterTransacoesAsync(request);
                return Ok(transacoes);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("insights")]
        public async Task<IActionResult> ObterInsights([FromQuery] InsightsQueryRequest request)
        {
            try
            {
                var insights = await _financasService.ObterInsightsAsync(request);
                return Ok(insights);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperation ao obter insights: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao obter insights");
                return StatusCode(500, new { error = "Erro interno ao gerar insights. Verifique os logs do servidor." });
            }
        }

        [HttpGet("categorias")]
        public async Task<IActionResult> ObterCategorias()
        {
            var categorias = await _financasService.ObterCategoriasAsync();
            return Ok(categorias);
        }

        [HttpPost("tags")]
        public async Task<IActionResult> CriarTag([FromBody] NovaTagRequest request)
        {
            try
            {
                var tag = await _financasService.CriarTagAsync(request);
                return Ok(tag);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("tags")]
        public async Task<IActionResult> ObterTags([FromQuery] int contaId)
        {
            try
            {
                var tags = await _financasService.ObterTagsAsync(contaId);
                return Ok(tags);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("transacoes/tags")]
        public async Task<IActionResult> VincularTag([FromBody] AdicionarTagTransacaoRequest request)
        {
            try
            {
                var vinculo = await _financasService.VincularTagAsync(request);
                return Ok(vinculo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("transacoes/anexos")]
        public async Task<IActionResult> AdicionarAnexo([FromBody] NovoAnexoTransacaoRequest request)
        {
            try
            {
                var anexo = await _financasService.AdicionarAnexoAsync(request);
                return Ok(anexo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("transacoes/anexos/arquivo")]
        [RequestSizeLimit(49_000_000)]
        public async Task<IActionResult> AdicionarAnexoArquivo([FromQuery] int contaId, [FromForm] NovoAnexoArquivoRequest request, CancellationToken cancellationToken)
        {
            if (request.Arquivo == null || request.Arquivo.Length == 0)
            {
                return BadRequest(new { error = "Arquivo inválido." });
            }

            try
            {
                await using var stream = request.Arquivo.OpenReadStream();
                var url = await _anexoStorageService.UploadAsync(contaId, request.TransacaoId, request.Arquivo.FileName, stream, request.Arquivo.ContentType, cancellationToken);

                var anexo = await _financasService.AdicionarAnexoAsync(new NovoAnexoTransacaoRequest
                {
                    TransacaoId = request.TransacaoId,
                    Tipo = request.Tipo,
                    ConteudoTexto = string.Empty,
                    Url = url
                });

                return Ok(anexo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("metas")]
        public async Task<IActionResult> CriarMeta([FromBody] NovaMetaRequest request)
        {
            try
            {
                var meta = await _financasService.CriarMetaAsync(request);
                return Ok(meta);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("metas/{metaId:int}")]
        public async Task<IActionResult> AtualizarMeta(int metaId, [FromBody] NovaMetaRequest request)
        {
            try
            {
                var meta = await _financasService.AtualizarMetaAsync(metaId, request);
                return Ok(meta);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("metas/{metaId:int}")]
        public async Task<IActionResult> RemoverMeta(int metaId)
        {
            try
            {
                await _financasService.RemoverMetaAsync(metaId);
                return Ok(new { message = "Meta removida com sucesso." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("metas")]
        public async Task<IActionResult> ObterMetas([FromQuery] int contaId)
        {
            try
            {
                var metas = await _financasService.ObterMetasAsync(contaId);
                return Ok(metas);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("metas/{metaId}/contribuir")]
        public async Task<IActionResult> ContribuirMeta(int metaId, [FromBody] ContribuirMetaRequest request)
        {
            try
            {
                var meta = await _financasService.ContribuirMetaAsync(metaId, request);
                return Ok(meta);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("saldo-atual")]
        public async Task<IActionResult> ObterSaldoAtual([FromQuery] int contaId)
        {
            try
            {
                var saldo = await _financasService.ObterSaldoAtualAsync(contaId);
                return Ok(new { saldoAtual = saldo });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("exportacao/csv")]
        public async Task<IActionResult> ExportarCsv([FromQuery] int contaId, [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
        {
            try
            {
                var bytes = await _exportacaoService.ExportarTransacoesCsvAsync(contaId, inicio, fim);
                var nomeArquivo = $"transacoes_{contaId}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
                return File(bytes, "text/csv", nomeArquivo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("exportacao/pdf")]
        public async Task<IActionResult> ExportarPdf([FromQuery] int contaId, [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
        {
            try
            {
                var bytes = await _exportacaoService.ExportarTransacoesPdfAsync(contaId, inicio, fim);
                var nomeArquivo = $"transacoes_{contaId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
                return File(bytes, "application/pdf", nomeArquivo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("exportar")]
        public async Task<IActionResult> Exportar([FromQuery] int contaId, [FromQuery] string formato, [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
        {
            if (string.IsNullOrWhiteSpace(formato))
            {
                return BadRequest(new { error = "Formato é obrigatório (csv ou pdf)." });
            }

            var formatoNormalizado = formato.Trim().ToLowerInvariant();
            return formatoNormalizado switch
            {
                "csv" => await ExportarCsv(contaId, inicio, fim),
                "pdf" => await ExportarPdf(contaId, inicio, fim),
                _ => BadRequest(new { error = "Formato inválido. Use csv ou pdf." })
            };
        }

        [HttpPost("importacao/csv")]
        [RequestSizeLimit(49_000_000)]
        public async Task<IActionResult> ImportarCsv([FromQuery] int contaId, [FromForm] IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                return BadRequest(new { error = "Arquivo CSV inválido." });
            }

            try
            {
                await using var stream = arquivo.OpenReadStream();
                var resultado = await _importacaoService.ImportarTransacoesCsvAsync(contaId, stream);
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("importar")]
        [RequestSizeLimit(49_000_000)]
        public async Task<IActionResult> Importar([FromQuery] int contaId, [FromQuery] string? formato, [FromForm] IFormFile arquivo)
        {
            var formatoNormalizado = string.IsNullOrWhiteSpace(formato) ? "csv" : formato.Trim().ToLowerInvariant();
            if (formatoNormalizado != "csv")
            {
                return BadRequest(new { error = "Formato inválido. Use csv." });
            }

            return await ImportarCsv(contaId, arquivo);
        }
    }
}
