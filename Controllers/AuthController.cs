using Microsoft.AspNetCore.Mvc;
using PraOndeFoi.DTOs;
using PraOndeFoi.Repository;
using Supabase;

namespace PraOndeFoi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly Client _supabase;
        private readonly IContaRepository _contaRepository;

        public AuthController(Client supabase, IContaRepository contaRepository)
        {
            _supabase = supabase;
            _contaRepository = contaRepository;
        }

        [HttpPost("entrar")]
        public async Task<IActionResult> Entrar([FromBody] LoginRequest request)
        {
            try
            {
                var session = await _supabase.Auth.SignIn(request.Email, request.Password);
                if (session?.User == null)
                {
                    return Unauthorized(new { error = "Falha ao autenticar." });
                }

                var userId = session.User.Id;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new { error = "Usuário inválido." });
                }

                var conta = await _contaRepository.ObterContaPorUsuarioAsync(userId);
                return Ok(new { token = session.AccessToken, userId, contaId = conta?.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("perfil")]
        public async Task<IActionResult> Perfil()
        {
            var authHeader = Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { error = "Token ausente." });
            }

            var token = authHeader["Bearer ".Length..].Trim();
            if (string.IsNullOrWhiteSpace(token))
            {
                return Unauthorized(new { error = "Token inválido." });
            }

            try
            {
                var user = await _supabase.Auth.GetUser(token);
                if (user == null)
                {
                    return Unauthorized(new { error = "Usuário não encontrado." });
                }

                var userId = user.Id;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new { error = "Usuário inválido." });
                }

                var conta = await _contaRepository.ObterContaPorUsuarioAsync(userId);
                var usuario = await _contaRepository.ObterUsuarioAsync(userId);

                return Ok(new
                {
                    userId,
                    email = user.Email ?? usuario?.Email ?? string.Empty,
                    nome = usuario?.Nome ?? string.Empty,
                    contaId = conta?.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("cadastrar")]
        public async Task<IActionResult> Cadastrar([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _supabase.Auth.SignUp(request.Email, request.Password);
                return Ok(new { message = "Usuário cadastrado com sucesso", user = user });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("sair")]
        public async Task<IActionResult> Sair()
        {
            try
            {
                await _supabase.Auth.SignOut();
                return Ok(new { message = "Logout realizado com sucesso" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}