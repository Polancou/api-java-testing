using ApiBaseCore.Application.DTOs;
using ApiBaseCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiBaseCore.Api.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PerfilUsuarioDto>>> GetUsers([FromQuery] UserFilterDto filterDto)
    {
        var users = await _userService.GetUsersAsync(filterDto);
        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<PerfilUsuarioDto>> CreateUser(RegistroUsuarioDto createUserDto)
    {
        var newUser = await _userService.CreateUserAsync(createUserDto);
        return CreatedAtAction(nameof(GetUsers), new { id = newUser.Id }, newUser);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<PerfilUsuarioDto>> UpdateUser(Guid id, UpdateUserDto updateDto)
    {
        var updatedUser = await _userService.UpdateUserAsync(id, updateDto);
        return Ok(updatedUser);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return NoContent();
    }
}
