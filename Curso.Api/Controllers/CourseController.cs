using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Curso.Api.Controllers;

// =====================================================================
// MODELO DE DOMÍNIO
// =====================================================================
// Representa um curso "de verdade" dentro da nossa aplicação: é o que
// guardamos internamente, com um Id gerado por nós.
//
// Repare que o Id NÃO existe nos DTOs de entrada — quem cria um curso
// não escolhe o Id, quem escolhe é a API.
public record Course(int Id, string Name, string Email);


// =====================================================================
// DTOs (Data Transfer Objects)
// =====================================================================
//
// Um DTO é um objeto que existe só para "viajar" entre o cliente e a
// API — ele não é o modelo de domínio, é o formato da requisição/resposta.
//
// Por que não usar o Course direto no Get/Post?
//
//   1. Segurança: o cliente não deveria poder enviar um Id na criação,
//      nem qualquer outro campo "interno" que não faça sentido pra fora.
//   2. Validação: anotamos exatamente o que é obrigatório numa
//      requisição, sem misturar essas regras com o modelo de domínio.
//   3. Flexibilidade: a resposta pode ter um formato diferente do que
//      guardamos internamente (por exemplo, esconder um campo sensível).
//
// Fluxo com DTOs:
//
//   Cliente envia JSON
//         ↓
//   Model Binding -> CreateCourseRequest (DTO de entrada)
//         ↓
//   Validação automática (Data Annotations)
//         ↓
//   Controller converte DTO -> Course (domínio)
//         ↓
//   Controller converte Course -> CourseResponse (DTO de saída)
//         ↓
//   Serialização -> JSON de volta pro cliente

// DTO de entrada: o que o cliente manda no corpo (body) do POST.
public record CreateCourseRequest(
    [property: Required(ErrorMessage = "O nome é obrigatório.")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    string Name,

    [property: Required(ErrorMessage = "O e-mail é obrigatório.")]
    [property: EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    string Email
);

// DTO de entrada: o que o cliente manda no corpo do PUT.
// Poderia ser o mesmo tipo do CreateCourseRequest, mas deixamos
// separado porque no futuro é comum as regras divergirem
// (ex: no update talvez o e-mail não possa ser trocado).
public record UpdateCourseRequest(
    [property: Required(ErrorMessage = "O nome é obrigatório.")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    string Name,

    [property: Required(ErrorMessage = "O e-mail é obrigatório.")]
    [property: EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    string Email
);

// DTO de saída: o que devolvemos pro cliente.
// Aqui incluímos o Id, porque agora faz sentido o cliente saber
// qual é o identificador do recurso que ele acabou de criar/consultar.
public record CourseResponse(int Id, string Name, string Email)
{
    // Method de "fábrica" que centraliza a conversão Course -> DTO.
    // Sem isso, ficaríamos repetindo "new CourseResponse(...)" em
    // cada Action.
    public static CourseResponse FromCourse(Course course) =>
        new(course.Id, course.Name, course.Email);
}


// =====================================================================
// CONTROLLER
// =====================================================================
[ApiController]
[Route("[controller]")]
public class CursoController : ControllerBase
{
    // -------------------------------------------------------------
    // ARMAZENAMENTO EM MEMÓRIA
    // -------------------------------------------------------------
    // IMPORTANTE (ponto de atenção pra aula):
    //
    // O ASP.NET Core cria uma NOVA instância de CursoController a
    // CADA requisição. Isso significa que um campo de instância
    // (sem "static") seria recriado do zero toda vez — os dados
    // "sumiriam" entre uma chamada e outra.
    //
    // Por isso usamos "static": o dado fica vivo no processo da
    // aplicação, compartilhado entre todas as requisições, e não
    // preso à instância da controller.
    //
    // Isso é só um "banco de dados de mentirinha" pra aula de hoje.
    // Na aula que vem, trocamos isso por Entity Framework, que
    // resolve esse problema (e outros, como persistência real em
    // disco) de forma apropriada.
    private static readonly List<Course> _courses = new();

    // Contador simples pra gerar Ids incrementais.
    // Também precisa ser static, pelo mesmo motivo acima.
    private static int _nextId = 1;


    // =========================================================
    // GET /Curso
    // =========================================================
    // Lista todos os cursos cadastrados.
    [HttpGet]
    public ActionResult<IEnumerable<CourseResponse>> Get()
    {
        // Convertemos cada Course (domínio) em CourseResponse (DTO)
        // antes de devolver. O cliente nunca "vê" o Course diretamente.
        var response = _courses.Select(CourseResponse.FromCourse);

        // Ok() = HTTP 200. O ASP.NET Core serializa a coleção pra JSON.
        return Ok(response);
    }


    // =========================================================
    // GET /Curso/{id}
    // =========================================================
    // Busca um curso específico pelo Id.
    //
    // "{id}" na rota é um parâmetro de rota (route parameter).
    // O ASP.NET Core faz o Model Binding automaticamente: pega o
    // valor que veio na URL e tenta converter pro tipo do parâmetro
    // do método (aqui, int).
    [HttpGet("{id}")]
    public ActionResult<CourseResponse> GetById(int id)
    {
        var course = _courses.FirstOrDefault(c => c.Id == id);

        // Curso não encontrado -> HTTP 404 Not Found.
        if (course is null)
            return NotFound();

        // Curso encontrado -> HTTP 200 OK com o DTO de resposta.
        return Ok(CourseResponse.FromCourse(course));
    }


    // =========================================================
    // POST /Curso
    // =========================================================
    // Cria um novo curso.
    //
    // [FromBody] diz explicitamente: "esse parâmetro vem do corpo
    // da requisição". Em Controllers marcadas com [ApiController],
    // o ASP.NET Core já assume [FromBody] pra tipos complexos, mas
    // deixamos explícito aqui só pra fins didáticos.
    //
    // VALIDAÇÃO: como a Controller tem [ApiController], se o
    // "request" não passar nas Data Annotations (Required,
    // EmailAddress, StringLength...), o ASP.NET Core devolve
    // automaticamente um HTTP 400 Bad Request com os erros —
    // nosso código nem chega a ser executado. Não precisamos
    // checar "ModelState.IsValid" na mão.
    [HttpPost]
    public ActionResult<CourseResponse> Post([FromBody] CreateCourseRequest request)
    {
        var course = new Course(_nextId++, request.Name, request.Email);
        _courses.Add(course);

        var response = CourseResponse.FromCourse(course);

        // CreatedAtAction() = HTTP 201 Created.
        // Além do corpo com o recurso criado, ele preenche o header
        // "Location" apontando pra rota que busca esse curso
        // (GetById), seguindo a convenção REST de "onde encontrar
        // o recurso que acabei de criar".
        return CreatedAtAction(nameof(GetById), new { id = course.Id }, response);
    }


    // =========================================================
    // PUT /Curso/{id}
    // =========================================================
    // Atualiza um curso existente por completo (substitui Name e Email).
    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] UpdateCourseRequest request)
    {
        var index = _courses.FindIndex(c => c.Id == id);

        if (index == -1)
            return NotFound();

        // Course é um record, então é imutável: pra "atualizar",
        // criamos um novo Course com o mesmo Id e substituímos na lista.
        _courses[index] = new Course(id, request.Name, request.Email);

        // NoContent() = HTTP 204: deu certo, mas não há corpo pra devolver.
        // É a resposta convencional pra updates bem-sucedidos.
        return NoContent();
    }


    // =========================================================
    // DELETE /Curso/{id}
    // =========================================================
    // Remove um curso pelo Id.
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var course = _courses.FirstOrDefault(c => c.Id == id);

        if (course is null)
            return NotFound();

        _courses.Remove(course);

        // Também 204: deletou com sucesso, sem corpo de resposta.
        return NoContent();
    }
}