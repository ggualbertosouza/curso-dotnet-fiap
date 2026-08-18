using Microsoft.AspNetCore.Mvc;
using Curso.Api.Dto;

namespace Curso.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DemoController : ControllerBase
{
    // GET /Demo/encontrado?existe=true
    [HttpGet("encontrado")]
    public IActionResult Encontrado([FromQuery] bool existe = true)
    {
        if (!existe)
            return NotFound();

        return Ok(new { mensagem = "Encontrado!" });
    }

    // GET /Demo/rota/42
    [HttpGet("rota/{valor}")]
    public IActionResult ExemploRota([FromRoute] string valor) =>
        Ok(new { origem = "rota", valor });

    // GET /Demo/query?valor=42
    [HttpGet("query")]
    public IActionResult ExemploQuery([FromQuery] string? valor) =>
        Ok(new { origem = "query string", valor });

    // POST /Demo/corpo   Body: { "name": "...", "email": "..." }
    [HttpPost("corpo")]
    public IActionResult ExemploCorpo([FromBody] CreateStudentRequest valor) =>
        Ok(new { origem = "corpo (JSON)", valor });

    // GET /Demo/header   Header: X-Exemplo: 42
    [HttpGet("header")]
    public IActionResult ExemploHeader([FromHeader(Name = "X-Exemplo")] string? valor) =>
        Ok(new { origem = "header", valor });
}
