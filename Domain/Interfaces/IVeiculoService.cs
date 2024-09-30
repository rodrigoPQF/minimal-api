using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;

namespace minimal_api.Infra.Interfaces;

public interface IVeiculoService
{
    List<Veiculo> TodosVeiculos(int page = 1, string? nome = null, string? marca = null);
    Veiculo? BuscaPorId(int id);
    void CadastraVeiculo(Veiculo veiculo);
    Veiculo AtualizaVeiculo(int id, Veiculo veiculo);
    bool DeletaVeiculo(int id);
}
