using System.Text;
using CatalogoApi.Data;
using CatalogoApi.Models;
using CatalogoApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<ITokenService>(new TokenService());
builder.Services.AddAuthentication
    (JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters 
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
            };
        });
builder.Services.AddAuthorization();

var app = builder.Build();

app.MapGet("/", () => "Início");

#region Login
app.MapPost("/login", [AllowAnonymous] (UserModel userModel, ITokenService tokenService) => {
    if (userModel is null)
        return Results.BadRequest("Objeto inválido");
    
    if (userModel.UserName.Equals("admin") && userModel.Password.Equals("Numsey#123"))
    {
        var tokenString = tokenService.GerarToken(
            app.Configuration["Jwt:Key"], app.Configuration["Jwt:Issuer"], app.Configuration["Jwt:Audience"], userModel
        );

        return Results.Ok(new { token = tokenString });
    }
    else
    {
        return Results.BadRequest("Usuário ou senha inválidos");
    }
});
#endregion

#region Categorias
app.MapPost("/categorias", async (Categoria categoria, AppDbContext db) => {
    db.Categorias.Add(categoria);
    await db.SaveChangesAsync();

    return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);
});

app.MapGet("/categorias", async (AppDbContext db) =>
    await db.Categorias.ToListAsync())
    .RequireAuthorization();;

app.MapGet("/categorias/{id:int}", async (int id, AppDbContext db) => {
    return await db.Categorias.FindAsync(id)
        is Categoria categoria ? Results.Ok(categoria) : Results.NotFound();
});

app.MapPut("/categorias/{id:int}", async (int id, Categoria categoria, AppDbContext db) => {
    if (categoria.CategoriaId != id)
        return Results.BadRequest();

    var categoriaDB = await db.Categorias.FindAsync(id);

    if (categoriaDB is null)
        return Results.NotFound();

    categoriaDB.Nome = categoria.Nome;
    categoriaDB.Descricao = categoria.Descricao;

    await db.SaveChangesAsync();

    return Results.Ok(categoriaDB);
});

app.MapDelete("/categorias/{id:int}", async (int id, AppDbContext db) => {
    var categoria = await db.Categorias.FindAsync(id);

    if (categoria is null)
        return Results.NotFound();

    db.Categorias.Remove(categoria);
    await db.SaveChangesAsync();

    return Results.NoContent();
});
#endregion

#region Produtos
app.MapPost("/produtos", async (Produto produto, AppDbContext db) => {
    db.Produtos.Add(produto);
    await db.SaveChangesAsync();

    return Results.Created($"/produtos/{produto.ProdutoId}", produto);
});

app.MapGet("/produtos", async (AppDbContext db) => 
    await db.Produtos.ToListAsync());

app.MapGet("/produtos/{id:int}", async (int id, AppDbContext db) => {
    return await db.Produtos.FindAsync(id)
        is Produto produto ?
            Results.Ok(produto) : Results.NotFound("Produto não encontrado");
});

app.MapPut("/produtos", async (int id, string produtoNome, AppDbContext db) => {
    var produtoDb = await db.Produtos.SingleOrDefaultAsync(x => x.ProdutoId == id);
    if (produtoDb is null)
        return Results.NotFound();

    produtoDb.Nome = produtoNome;
    await db.SaveChangesAsync();

    return Results.Ok(produtoDb);
});

app.MapPut("/produtos/{id:int}", async (int id, Produto produto, AppDbContext db) => {
    if (produto.ProdutoId != id)
        return Results.BadRequest();

    var produtoDb = await db.Produtos.FindAsync(id);
    if (produtoDb is null)
        return Results.NotFound("Produto não encontrado");

    produtoDb.Nome = produto.Nome;
    produtoDb.Descricao = produto.Descricao;
    produtoDb.Preco = produto.Preco;
    produtoDb.DataCompra = produto.DataCompra;
    produtoDb.Estoque = produto.Estoque;
    produtoDb.Imagem = produto.Imagem;
    produtoDb.CategoriaId = produto.CategoriaId;

    await db.SaveChangesAsync();

    return Results.Ok(produtoDb);
});

app.MapDelete("/produtos/{id:int}", async (int id, AppDbContext db) => {
    var produto = await db.Produtos.FindAsync(id);
    if (produto is null)
        return Results.NotFound("produto não encontrado");

    db.Produtos.Remove(produto);
    await db.SaveChangesAsync();

    return Results.NoContent();
});
#endregion

app.UseAuthentication();
app.UseAuthorization();

app.Run();
