using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;

namespace minimal_api.Infra.Interfaces;

public interface IAdminService
{
    Admin? Login(LoginDTO loginDTO);

}
