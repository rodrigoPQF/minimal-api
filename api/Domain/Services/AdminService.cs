using Microsoft.EntityFrameworkCore.Storage;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Infra.Database;
using minimal_api.Infra.Interfaces;

namespace minimal_api.Domain.Services;


public class AdminService : IAdminService
{
    private readonly DatabaseContext _context;
    public AdminService(DatabaseContext context)
    {
        _context = context;
    }


    public Admin? Login(LoginDTO loginDTO)
    {
        var admin = _context.Admins.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();

        return admin;
    }

    public Admin? Cadastrar(Admin admin)
    {
        _context.Admins.Add(admin);
        _context.SaveChanges();
        return admin;
    }
}
