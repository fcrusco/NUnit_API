using MinhaAPISimples.Data;
using MinhaAPISimples.Models;

var builder = WebApplication.CreateBuilder(args);

// ------ SERVIÇOS ------
// 1. ADICIONA o serviço de Controllers
builder.Services.AddControllers();

// (Serviços do Swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------ PIPELINE ------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. REMOVEMOS os app.MapGet() e app.MapPost()

// 3. ADICIONAMOS o mapeamento dos Controllers
app.MapControllers();


app.Run();

// Esta linha continua sendo essencial para o WebApplicationFactory
public partial class Program { }