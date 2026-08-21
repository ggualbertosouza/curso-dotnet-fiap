using System.Text;
using Curso.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

// Configura autenticação via JWT Bearer.
// A mesma chave secreta usada aqui pra validar precisa ser a usada no AuthController pra assinar.
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSettings["Secret"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

// Habilita o uso de [Authorize] (e [Authorize(Roles = "...")]) nos controllers
builder.Services.AddAuthorization();

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

// Lê e valida o JWT enviado no header Authorization (Bearer), preenchendo o usuário da requisição.
// Precisa vir antes do UseAuthorization, que é quem decide se o usuário autenticado pode acessar o recurso.
app.UseAuthentication();
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
