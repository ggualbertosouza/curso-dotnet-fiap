# Entity Framework Core no ASP.NET Core 10

## 1. Suba o SQL Server no Docker

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SuaSenhaForte123!" \
  -p 1433:1433 --name sql-local -d mcr.microsoft.com/mssql/server:2022-latest
```

## 2. Instale os pacotes

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

## 3. O que é DbContext e DbSet

- **DbContext**: é a "ponte" entre suas classes C# e o banco. Ele gerencia a conexão, rastreia mudanças nas entidades e traduz LINQ em SQL.
- **DbSet\<T>**: representa uma tabela do banco. Cada `DbSet<Student>` mapeia para a tabela `Students`, e você usa LINQ nele (`.Where()`, `.ToList()`, etc).

## 4. Crie a entidade

```csharp
namespace Curso.Api.Models;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

## 5. Crie o DbContext

```csharp
using Microsoft.EntityFrameworkCore;
using Curso.Api.Models;

namespace Curso.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
}
```

## 6. Connection string (appsettings.json)

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost,1433;Database=CursoDb;User Id=sa;Password=SuaSenhaForte123!;TrustServerCertificate=True"
  }
}
```

## 7. Registre no Program.cs

```csharp
using Curso.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();
```

## 8. Migrations (criar/atualizar tabelas)

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 9. Usando num Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Curso.Api.Data;
using Curso.Api.Models;

namespace Curso.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _context;
    public StudentsController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _context.Students.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = student.Id }, student);
    }
}
```

`AddDbContext` injeta o contexto com escopo por requisição (injeção de dependência no construtor do controller) — é assim que ele chega até você.

> **Nota:** o projeto já possui um `StudentController` com armazenamento em memória (`Controllers/StudentController.cs`). Este guia usa `StudentsController` (com "s") como um controller separado, orientado a EF Core, para não conflitar com o existente. Ao migrar de fato para o banco, decida se substitui o controller em memória ou remove um dos dois.
