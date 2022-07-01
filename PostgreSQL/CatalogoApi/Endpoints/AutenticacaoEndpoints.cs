using CatalogoApi.Models;
using CatalogoApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace CatalogoApi.Endpoints;

public static class AutenticacaoEndpoints
{
    public static void MapAutenticacaoEndpoints(this WebApplication app)
    {
        app.MapPost("/login", [AllowAnonymous] (UserModel userModel, ITokenService tokenService) => {
            if (userModel is null)
                return Results.BadRequest("Objeto inválido");
            
            if (userModel.UserName == "admin" && userModel.Password == "Numsey#123")
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
    }
}