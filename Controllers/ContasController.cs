using Microsoft.AspNetCore.Mvc;
using PraOndeFoi.DTOs;
using PraOndeFoi.Services;

namespace PraOndeFoi.Controllers
{
    [ApiController]
    [Route("api/contas")]
    public class ContasController : ControllerBase
    {
        private readonly IContaService _contaService;

        public ContasController(IContaService contaService)
        {
            _contaService = contaService;
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarContaRequest request)
        {
            try
            {
                var conta = await _contaService.CriarContaAsync(request);
                return CreatedAtAction(nameof(ObterPorId), new { contaId = conta.Id }, conta);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpGet("{contaId:int}")]
        public async Task<IActionResult> ObterPorId(int contaId)
        {
            var conta = await _contaService.ObterContaAsync(contaId);
            if (conta == null)
            {
                return NotFound();
            }

            return Ok(conta);
        }
    }
}
