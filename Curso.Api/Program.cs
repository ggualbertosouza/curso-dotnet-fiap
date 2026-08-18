// Cria o builder da nossa aplicação
// responsável por preparar nossa aplicação, vamos encadear configurações aqui como:
// 1.Serviços 2.Ambiente 3.Logging 4.Servidores
var builder = WebApplication.CreateBuilder(args);

// builder.Services é o conjunto de serviçps registrados na aplicação.

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
