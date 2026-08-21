using Curso.Api.Data;
using Microsoft.EntityFrameworkCore;

// Cria o builder da nossa aplicação
// responsável por preparar nossa aplicação, vamos encadear configurações aqui como:
// 1.Serviços 2.Ambiente 3.Logging 4.Servidores
var builder = WebApplication.CreateBuilder(args);

// builder.Services é o conjunto de serviçps registrados na aplicação.

// Registra o DbContext com escopo por requisição, usando a connection string "Default"
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Nome da política de CORS, usado tanto no registro quanto na aplicação do middleware
const string LocalhostCorsPolicy = "LocalhostOnly";

// Configura CORS (Cross-Origin Resource Sharing).
// Sem isso, um front-end rodando em outra origem (ex: http://localhost:3000)
// tem as requisições bloqueadas pelo navegador ao chamar essa API.
builder.Services.AddCors(options =>
{
    options.AddPolicy(LocalhostCorsPolicy, policy =>
    {
        policy
            // Libera qualquer origem cujo host seja "localhost", independente da porta
            // (ex: http://localhost:3000, http://localhost:5173, https://localhost:4200).
            // Isso evita ter que fixar uma porta específica só pra desenvolvimento.
            .SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Adiciona as controllers a aplicação
builder.Services.AddControllers();

// Permite ao ASP.NET Core explorar e descrever os endpoints
builder.Services.AddEndpointsApiExplorer();

// Gera documentação Swagger/OpenAPI
builder.Services.AddSwaggerGen();

// Constrói a aplicação - Onde tudo que definimos acima é construído
var app = builder.Build();

// Definimos o utilizar da interface do swagger no ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Adiciona um middleware que redireciona requisições HTTP para HTTPS
app.UseHttpsRedirection();

// Aplica a política de CORS definida acima.
// Precisa vir depois do UseHttpsRedirection e antes do UseAuthorization/MapControllers,
// pra que o navegador já receba os headers de CORS antes de qualquer checagem de autorização.
app.UseCors(LocalhostCorsPolicy);

// Trata a autenticação - Vamos específicar mais depois
app.UseAuthorization();

// Reconhece os endpoints da aplicação
app.MapControllers();

// Efetivamente rodamos a aplicação
app.Run();


/*
   Cria o builder
         ↓
 Configura Serviços
         ↓
 Contrói a aplicação
         ↓
 Configura Middleware
         ↓
  Mapeia Endpoints
         ↓
    Executa Api
 */
