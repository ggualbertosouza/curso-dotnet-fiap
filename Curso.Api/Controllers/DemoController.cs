using Microsoft.AspNetCore.Mvc;

namespace Curso.Api.Controllers;

// =====================================================================
// CONTROLLER DE DEMONSTRAÇÃO
// =====================================================================
// Esse controller NÃO representa um recurso de domínio (como Course).
// Ele existe só pra mostrar, lado a lado, as várias formas de escrever
// uma Action e os vários tipos de retorno que o ASP.NET Core oferece.
//
// Sugestão de uso em aula: rodar cada endpoint no Swagger/Postman/.http
// e observar o status code + corpo da resposta que cada um devolve.
[ApiController]
[Route("[controller]")]
public class DemoController : ControllerBase
{
    // =====================================================================
    // PARTE 1 — FORMAS DE ESCREVER UMA ACTION
    // =====================================================================

    // ---------------------------------------------------------------
    // 1.1 - Retornando void
    // ---------------------------------------------------------------
    // GET /Demo/void
    //
    // Uma Action pode não retornar nada. Nesse caso o ASP.NET Core
    // devolve uma resposta vazia com status 200 OK.
    //
    // Na prática isso é raro em APIs REST: como não controlamos o
    // status code dentro do método, não dá pra sinalizar erro. Serve
    // mais pra entender que uma Action é só um método comum de C# —
    // ela não *precisa* devolver algo.
    [HttpGet("void")]
    public void RetornoVoid()
    {
        // Nada aqui. A resposta será 200 OK, corpo vazio.
    }

    // ---------------------------------------------------------------
    // 1.2 - Retornando o tipo direto (sem "embrulhar" em ActionResult)
    // ---------------------------------------------------------------
    // GET /Demo/tipo-direto
    //
    // Podemos devolver o objeto puro. O ASP.NET Core serializa pra
    // JSON e responde 200 OK automaticamente.
    //
    // Limitação: não dá pra devolver, por exemplo, um 404 dentro
    // desse método sem lançar uma exceção — o tipo de retorno é fixo.
    [HttpGet("tipo-direto")]
    public Course RetornoTipoDireto()
    {
        return new Course(1, "ASP.NET Core", "curso@email.com");
    }

    // ---------------------------------------------------------------
    // 1.3 - Retornando IActionResult
    // ---------------------------------------------------------------
    // GET /Demo/iactionresult?encontrado=true
    //
    // IActionResult é uma interface: qualquer resultado (Ok, NotFound,
    // BadRequest...) implementa ela. Como consequência, dentro do
    // método podemos "escolher" qual status code devolver.
    //
    // Desvantagem: o Swagger não sabe sozinho qual é o "formato de
    // sucesso" — por isso usamos [ProducesResponseType] pra documentar
    // manualmente.
    [HttpGet("iactionresult")]
    [ProducesResponseType(typeof(Course), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult RetornoIActionResult([FromQuery] bool encontrado = true)
    {
        if (!encontrado)
            return NotFound();

        return Ok(new Course(1, "ASP.NET Core", "curso@email.com"));
    }

    // ---------------------------------------------------------------
    // 1.4 - Retornando ActionResult<T>
    // ---------------------------------------------------------------
    // GET /Demo/actionresult-generico?encontrado=true
    //
    // ActionResult<T> é o "melhor dos dois mundos": dá pra devolver
    // tanto um resultado de sucesso tipado (T) quanto qualquer outro
    // ActionResult (NotFound, BadRequest...), e o Swagger já entende
    // automaticamente que o corpo de sucesso é T — sem precisar do
    // [ProducesResponseType] pra isso.
    //
    // Por isso é a opção recomendada na maioria dos casos (foi o que
    // usamos na CursoController).
    [HttpGet("actionresult-generico")]
    public ActionResult<Course> RetornoActionResultGenerico([FromQuery] bool encontrado = true)
    {
        if (!encontrado)
            return NotFound();

        return new Course(1, "ASP.NET Core", "curso@email.com");
    }

    // ---------------------------------------------------------------
    // 1.5 - Action assíncrona
    // ---------------------------------------------------------------
    // GET /Demo/assincrono
    //
    // Quando a Action faz algo que "espera" (chamar um banco de
    // dados, uma API externa, ler um arquivo...), ela deve ser
    // assíncrona: retorna Task<T> (ou Task<IActionResult>,
    // Task<ActionResult<T>>) e usa "await" por dentro.
    //
    // Isso libera a thread do servidor enquanto espera a operação
    // terminar, em vez de travá-la parada. Vamos usar bastante
    // Task/async quando chegarmos em Entity Framework, já que toda
    // operação de banco é assíncrona.
    [HttpGet("assincrono")]
    public async Task<ActionResult<Course>> RetornoAssincrono()
    {
        // Simula uma operação que demora (ex: uma consulta ao banco).
        await Task.Delay(200);

        return new Course(1, "ASP.NET Core", "curso@email.com");
    }


    // =====================================================================
    // PARTE 2 — MÉTODOS DE CONVENIÊNCIA PARA STATUS CODE
    // =====================================================================
    // A ControllerBase já vem com vários métodos prontos que criam o
    // ActionResult certo pra cada situação. Todos abaixo estão em GET
    // só pra facilitar o teste (numa API de verdade, cada um apareceria
    // no verbo apropriado: POST, DELETE etc).

    // GET /Demo/status/200  -> sucesso, com corpo
    [HttpGet("status/200")]
    public IActionResult Status200() => Ok(new { mensagem = "Deu certo." });

    // GET /Demo/status/201  -> recurso criado
    // CreatedAtAction aponta, no header "Location", pra rota que
    // permite buscar o recurso recém-criado (aqui apontamos pra
    // Action "Status200" só como exemplo didático).
    [HttpGet("status/201")]
    public IActionResult Status201() =>
        CreatedAtAction(nameof(Status200), null, new { id = 1 });

    // GET /Demo/status/204  -> sucesso, sem corpo (comum em DELETE/PUT)
    [HttpGet("status/204")]
    public IActionResult Status204() => NoContent();

    // GET /Demo/status/400  -> erro do cliente (dado inválido)
    [HttpGet("status/400")]
    public IActionResult Status400() => BadRequest(new { erro = "Dado inválido." });

    // GET /Demo/status/401  -> não autenticado
    [HttpGet("status/401")]
    public IActionResult Status401() => Unauthorized();

    // GET /Demo/status/403  -> autenticado, mas sem permissão
    [HttpGet("status/403")]
    public IActionResult Status403() => Forbid();

    // GET /Demo/status/404  -> recurso não encontrado
    [HttpGet("status/404")]
    public IActionResult Status404() => NotFound();

    // GET /Demo/status/409  -> conflito (ex: e-mail já cadastrado)
    [HttpGet("status/409")]
    public IActionResult Status409() => Conflict(new { erro = "Já existe um curso com esse e-mail." });

    // GET /Demo/status/422  -> entidade não processável
    // (dado bem formado, mas que viola uma regra de negócio)
    [HttpGet("status/422")]
    public IActionResult Status422() => UnprocessableEntity(new { erro = "Regra de negócio violada." });

    // GET /Demo/status/qualquer/{codigo}
    // StatusCode(int) devolve QUALQUER status code, quando nenhum
    // método de conveniência serve pro seu caso.
    [HttpGet("status/qualquer/{codigo:int}")]
    public IActionResult StatusQualquer(int codigo) =>
        StatusCode(codigo, new { mensagem = $"Retornando o status {codigo} manualmente." });

    // GET /Demo/status/problem
    // Problem() gera uma resposta no formato "ProblemDetails" (RFC
    // 7807) — um jeito padronizado de descrever erros em APIs.
    // Por padrão devolve 500, mas dá pra customizar o status.
    [HttpGet("status/problem")]
    public IActionResult StatusProblem() =>
        Problem(title: "Algo deu errado", detail: "Detalhe técnico do erro.", statusCode: 500);


    // =====================================================================
    // PARTE 3 — OUTROS FORMATOS DE CONTEÚDO (além de JSON)
    // =====================================================================

    // GET /Demo/conteudo/texto
    // Content() devolve texto puro, controlando o Content-Type
    // manualmente — não passa pelo serializador JSON.
    [HttpGet("conteudo/texto")]
    public ContentResult ConteudoTexto() =>
        Content("Isso aqui é texto puro, não é JSON.", "text/plain");

    // GET /Demo/conteudo/arquivo
    // File() devolve bytes como um arquivo pra download.
    [HttpGet("conteudo/arquivo")]
    public FileContentResult ConteudoArquivo()
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes("conteúdo do arquivo de exemplo");
        return File(bytes, "text/plain", "exemplo.txt");
    }

    // GET /Demo/conteudo/redirecionar
    // Redirect() devolve 302 (redirecionamento temporário) com o
    // header "Location" apontando pra outra URL.
    [HttpGet("conteudo/redirecionar")]
    public RedirectResult ConteudoRedirecionar() =>
        Redirect("https://learn.microsoft.com/aspnet/core");


    // =====================================================================
    // PARTE 4 — DE ONDE VEM O DADO (fontes do Model Binding)
    // =====================================================================
    // O ASP.NET Core tenta adivinhar de onde tirar cada parâmetro da
    // Action, mas os atributos abaixo deixam isso explícito — bom pra
    // comparar as diferenças lado a lado.

    // GET /Demo/origem/rota/42
    // [FromRoute]: o valor vem de dentro da própria URL.
    [HttpGet("origem/rota/{valor}")]
    public IActionResult OrigemRota([FromRoute] string valor) =>
        Ok(new { origem = "rota", valor });

    // GET /Demo/origem/query?valor=42
    // [FromQuery]: o valor vem da query string (depois do "?").
    [HttpGet("origem/query")]
    public IActionResult OrigemQuery([FromQuery] string? valor) =>
        Ok(new { origem = "query string", valor });

    // POST /Demo/origem/corpo   Body: { "name": "...", "email": "..." }
    // [FromBody]: o valor vem do corpo (body) da requisição, como JSON.
    [HttpPost("origem/corpo")]
    public IActionResult OrigemCorpo([FromBody] CreateCourseRequest valor) =>
        Ok(new { origem = "corpo (JSON)", valor });

    // GET /Demo/origem/header   Header: X-Exemplo: 42
    // [FromHeader]: o valor vem de um cabeçalho HTTP.
    [HttpGet("origem/header")]
    public IActionResult OrigemHeader([FromHeader(Name = "X-Exemplo")] string? valor) =>
        Ok(new { origem = "header", valor });
}