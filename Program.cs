using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Enums;
using minimal_api.Domain.Model.Views;
using minimal_api.Domain.Services;
using minimal_api.Infra.Database;
using minimal_api.Infra.Interfaces;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();


builder.Services.AddDbContext<DatabaseContext>(
  options =>
  {
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres"));
  }
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Admin
app.MapPost("/admin/login", ([FromBody] LoginDTO logintDTO, IAdminService adminService) =>
{
  if (adminService.Login(logintDTO) != null)
    return Results.Ok("Login efetuado com sucesso");
  else
    return Results.Unauthorized();
}).WithTags("Admin");

app.MapPost("/admin", ([FromBody] AdminDTO adminDTO, IAdminService adminService) =>
{

  var validations = new ErrorsValidation { Messages = [] };

  if (string.IsNullOrEmpty(adminDTO.Email))
    validations.Messages.Add("Email inválido");
  if (string.IsNullOrEmpty(adminDTO.Perfil.ToString()))
    validations.Messages.Add("Perfil inválido");
  if (string.IsNullOrEmpty(adminDTO.Senha))
    validations.Messages.Add("Senha inválida");

  if (validations.Messages?.Count > 0)
  {
    return Results.BadRequest(validations);
  }

  var admin = new Admin
  {
    Email = adminDTO.Email,
    Perfil = adminDTO.Perfil.ToString(),
    Senha = adminDTO.Senha
  };
  adminService.Cadastrar(admin);

  var adms = new AdminModelView
  {
    Email = admin.Email,
    Perfil = Enum.Parse<Perfil>(admin.Perfil, true),
  };

  return Results.Created($"/admin/{admin.Id}", adms);
}).WithTags("Admin");

#endregion

#region Veiculos

static ErrorsValidation validaDTO(VeiculoDTO veiculoDTO)
{
  var validations = new ErrorsValidation
  {
    Messages = []
  };


  if (string.IsNullOrEmpty(veiculoDTO.Nome))
  {
    validations.Messages.Add("Nome inválido");
  }
  if (string.IsNullOrEmpty(veiculoDTO.Marca))
  {
    validations.Messages.Add("Marca inválida");
  }
  if (veiculoDTO.Ano < 1950)
  {
    validations.Messages.Add("Ano inválido");
  }

  return validations;

}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>
{

  var validations = validaDTO(veiculoDTO);

  if (validations.Messages?.Count > 0)
  {
    return Results.BadRequest(validations);
  }

  var veiculo = new Veiculo
  {
    Nome = veiculoDTO.Nome,
    Marca = veiculoDTO.Marca,
    Ano = veiculoDTO.Ano
  };
  veiculoService.CadastraVeiculo(veiculo);

  return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoService veiculoService) =>
{
  var veiculos = veiculoService.TodosVeiculos(pagina);

  return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculo/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
  var veiculo = veiculoService.BuscaPorId(id);

  if (veiculo == null) return Results.NotFound();

  return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapPut("/veiculo/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>
{

  var veiculo = veiculoService.BuscaPorId(id);

  if (veiculo == null) return Results.NotFound();

  var validations = validaDTO(veiculoDTO);

  if (validations.Messages?.Count > 0)
  {
    return Results.BadRequest(validations);
  }

  veiculo.Nome = veiculoDTO.Nome;
  veiculo.Marca = veiculoDTO.Marca;
  veiculo.Ano = veiculoDTO.Ano;

  veiculoService.AtualizaVeiculo(id, veiculo);

  return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapDelete("/veiculo/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
  var veiculo = veiculoService.BuscaPorId(id);

  if (veiculo == null) return Results.NotFound();


  veiculoService.DeletaVeiculo(id);

  return Results.NoContent();
}).WithTags("Veiculos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();

#endregion
