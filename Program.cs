using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Enums;
using minimal_api.Domain.Model.Views;
using minimal_api.Domain.Services;
using minimal_api.Infra.Database;
using minimal_api.Infra.Interfaces;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt").ToString();
if (string.IsNullOrEmpty(jwtSettings)) jwtSettings = "123456";

builder.Services.AddAuthentication(option =>
{
  option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateLifetime = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings)),
    ValidateIssuer = false,
    ValidateAudience = false
  };
});

builder.Services.AddAuthorization();


builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();


builder.Services.AddDbContext<DatabaseContext>(
  options =>
  {
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres"));
  }
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\nEnter your token: "
  });
  options.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme{
        Reference = new OpenApiReference
        {
          Type = ReferenceType.SecurityScheme,
          Id = "Bearer"
        }
      },
      Array.Empty<string>()
    }
  }
  );
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Admin

string GerarTokenJwt(Admin admin)
{
  if (string.IsNullOrEmpty(jwtSettings)) return string.Empty;

  var security = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings));
  var credentials = new SigningCredentials(security, SecurityAlgorithms.HmacSha256);

  var claims = new List<Claim>(){
    new("Email", admin.Email),
    new("Pefil", admin.Perfil.ToString())
  };


  var token = new JwtSecurityToken(
    claims: claims,
    expires: DateTime.Now.AddDays(1),
    signingCredentials: credentials
  );

  return new JwtSecurityTokenHandler().WriteToken(token);

}

app.MapPost("/admin/login", ([FromBody] LoginDTO logintDTO, IAdminService adminService) =>
{
  var adm = adminService.Login(logintDTO);
  if (adm != null)
  {
    string token = GerarTokenJwt(adm);
    return Results.Ok(new AdminLogado
    {
      Email = adm.Email,
      Perfil = adm.Perfil,
      Token = token
    });
  }
  else
    return Results.Unauthorized();
}).RequireAuthorization().WithTags("Admin");

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
}).AllowAnonymous().WithTags("Veiculos");

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

app.UseAuthentication();
app.UseAuthorization();

app.Run();

#endregion
