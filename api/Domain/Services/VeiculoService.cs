using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entities;
using minimal_api.Infra.Database;
using minimal_api.Infra.Interfaces;

namespace minimal_api.Domain.Services;


public class VeiculoService : IVeiculoService
{
    private readonly DatabaseContext _context;
    public VeiculoService(DatabaseContext context)
    {
        _context = context;
    }

    public Veiculo AtualizaVeiculo(int id, Veiculo veiculo)
    {
        _context.Veiculos.Update(veiculo);
        _context.SaveChanges();
        return veiculo;
    }

    public Veiculo? BuscaPorId(int id)
    {
        return _context.Veiculos.Where(v => v.Id == id).FirstOrDefault();
    }

    public void CadastraVeiculo(Veiculo veiculo)
    {
        _context.Veiculos.Add(veiculo);
        _context.SaveChanges();

    }

    public bool DeletaVeiculo(int id)
    {
        Veiculo? veiculo = _context.Veiculos.Find(id);
        if (veiculo == null)
        {
            return false;
        }

        _context.Veiculos.Remove(veiculo);
        _context.SaveChanges();

        return true;
    }


    public List<Veiculo> TodosVeiculos(int? page = 1, string? nome = null, string? marca = null)
    {
        var query = _context.Veiculos.AsQueryable();

        if (!string.IsNullOrEmpty(nome))
        {
            query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), $"%{nome}%"));
        }

        int itensPorPagina = 10;

        if (page != null)
        {
            query = query.Skip(((int)page - 1) * itensPorPagina).Take(itensPorPagina);
        }

        return [.. query];
    }
}
