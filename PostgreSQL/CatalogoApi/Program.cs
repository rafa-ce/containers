using CatalogoApi.Endpoints;
using CatalogoApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddPersistence();
builder.Services.AddCors();
builder.AddAutenticationJwt();

var app = builder.Build();

app.MapGet("/", () => "Início");

app.MapAutenticacaoEndpoints();
app.MapCategoriasEndpoints();
app.MapProdutosEndpoints();

var environment = app.Environment;

app.UseExceptionHandling(environment)
    .UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

public partial class Program { }