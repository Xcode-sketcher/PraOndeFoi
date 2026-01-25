using Microsoft.AspNetCore.Mvc;
using PraOndeFoi.DTOs;
using Supabase;

namespace PraOndeFoi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly Client _supabase;

        public AuthController(Client supabase)
        {
            _supabase = supabase;
        }

        [HttpPost("entrar")]
        public async Task<IActionResult> Entrar([FromBody] LoginRequest request)
        {
            try
            {
                var session = await _supabase.Auth.SignIn(request.Email, request.Password);
                if (session == null)
                {
                    return Unauthorized(new { error = "Falha ao autenticar." });
                }
                return Ok(new { token = session.AccessToken });
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