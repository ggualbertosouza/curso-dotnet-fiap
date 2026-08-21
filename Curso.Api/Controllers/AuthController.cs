using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Curso.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration) => _configuration = configuration;

    // POST /Auth/token?role=Admin
    // Rota pública: é o único jeito de conseguir um token, então não pode exigir token pra ser chamada.
    // Gera um JWT simples contendo a role recebida por parâmetro como claim.
    [AllowAnonymous]
    [HttpPost("token")]
    public IActionResult GenerateToken([FromQuery] string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return BadRequest(new { mensagem = "O parâmetro 'role' é obrigatório." });

        var jwtSettings = _configuration.GetSection("Jwt");
        var secret = jwtSettings["Secret"]!;
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiresInMinutes = jwtSettings.GetValue<int>("ExpiresInMinutes");

        // Claim de role: é o que o [Authorize(Roles = "...")] verifica depois, nos endpoints protegidos.
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, role)
        };

        // A mesma chave usada aqui pra assinar precisa ser a configurada no
        // TokenValidationParameters do Program.cs pra validar o token depois.
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { token = tokenString });
    }
}
