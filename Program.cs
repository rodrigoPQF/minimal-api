using MinimalApi.Domain.DTOs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", (LoginDTO logintDTO) =>
{
  if (logintDTO.Email == "adm@test.com" && logintDTO.Senha == "123456")
    return Results.Ok("Login efetuado com sucesso");
  else
    return Results.Unauthorized();
});

app.Run();

