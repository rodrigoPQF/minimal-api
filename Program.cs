using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Services;
using minimal_api.Infra.Database;
using minimal_api.Infra.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddDbContext<DatabaseContext>(
  options =>
  {
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres"));
  }
);

var app = builder.Build();



app.MapGet("/", () => "Hello World!");

app.MapPost("/login", ([FromBody] LoginDTO logintDTO, IAdminService adminService) =>
{
  if (adminService.Login(logintDTO) != null)
    return Results.Ok("Login efetuado com sucesso");
  else
    return Results.Unauthorized();
});

app.Run();

