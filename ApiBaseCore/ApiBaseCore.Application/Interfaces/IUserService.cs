using ApiBaseCore.Application.DTOs;

namespace ApiBaseCore.Application.Interfaces;

public interface IUserService
{
    Task<List<PerfilUsuarioDto>> GetUsersAsync(UserFilterDto filterDto);
    Task<PerfilUsuarioDto> GetUserByIdAsync(Guid id);
    Task<PerfilUsuarioDto> CreateUserAsync(RegistroUsuarioDto createUserDto);
    Task<PerfilUsuarioDto> UpdateUserAsync(Guid id, UpdateUserDto updateDto);
    Task DeleteUserAsync(Guid id);
}
