using Asp.Versioning;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Application.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCore.Api.Controllers;

/// <summary>
/// Gestiona las peticiones HTTP relacionadas con el perfil del usuario autenticado.
/// Este es un "controlador delgado" (thin controller): su única responsabilidad es recibir
/// las peticiones web, delegar toda la lógica de negocio al servicio correspondiente
/// (`IProfileService`), y mapear el resultado a una respuesta HTTP.
/// </summary>
[ApiVersion(version: "1.0")]
[Authorize] // El atributo [Authorize] protege todos los endpoints, requiriendo un JWT válido.
public class ProfileController(IProfileService profileService) : BaseApiController
{
    /// <summary>
    /// Obtiene los datos del perfil del usuario actualmente autenticado.
    /// </summary>
    /// <returns>Un 200 OK con el DTO del perfil.</returns>
    /// <returns>Un 200 404 Not Found si el usuario no existe.</returns>
    [HttpGet(template: "me")]
    public async Task<IActionResult> GetMyProfile()
    {
        // Se delega completamente la lógica de negocio al servicio de perfil.
        var perfilDto = await profileService.GetProfileByIdAsync(userId: UserId);
        return Ok(value: perfilDto);
    }

    /// <summary>
    /// Actualiza los datos del perfil del usuario actualmente autenticado.
    /// </summary>
    /// <param name="perfilDto">Un objeto JSON con los nuevos datos para el perfil.</param>
    /// <returns>Un 204 No Content si la actualización fue exitosa</returns>
    /// <returns>Un 404 Not Found si el usuario no existe.</returns>
    [HttpPut(template: "me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] ActualizarPerfilDto perfilDto)
    {
        // Se delega completamente la lógica de actualización al servicio de perfil.
        await profileService.ActualizarPerfilAsync(userId: UserId,
            perfilDto: perfilDto);
        return NoContent();
    }

    /// <summary>
    /// Cambia la contraseña del usuario autenticado.
    /// </summary>
    /// <param name="dto">Objeto con la contraseña antigua y la nueva.</param>
    /// <returns>Un 200 OK con mensaje de éxito o un 400 Bad Request con mensaje de error.</returns>
    [HttpPut(template: "change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] CambiarPasswordDto dto)
    {
        // Se delega completamente la lógica de cambio de contraseña al servicio de perfil
        var result = await profileService.CambiarPasswordAsync(userId: UserId,
            dto: dto);
        // Devuelve 200 OK con el mensaje de éxito
        return Ok(value: new { message = result.Message });
    }

    /// <summary>
    /// Sube o actualiza la imagen de avatar del usuario autenticado.
    /// </summary>
    /// <param name="file">El archivo de imagen enviado como form-data.</param>
    /// <returns>Un 200 OK con la nueva URL del avatar.</returns>
    [HttpPost(template: "avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        // 1. Validaciones básicas
        if (file == null || file.Length == 0)
            return BadRequest(error: new { message = "No se ha proporcionado ningún archivo." });

        if (file.Length > 1024 * 1024 * 2)
            return BadRequest(error: new { message = "El archivo debe ser menor a 2 MB." });

        var fileExtension = Path.GetExtension(path: file.FileName).ToLower();

        // 2. Validación Avanzada (Magic Numbers)
        // Abrimos el stream para leer los bytes reales
        using (var stream = file.OpenReadStream())
        {
            if (!FileSignatureValidator.IsValidImage(fileStream: stream,
                    extension: fileExtension))
            {
                return BadRequest(error: new { message = "El archivo no es una imagen válida o está corrupto." });
            }
        }

        // 3. Si pasa, procedemos a subir
        var newAvatarUrl = await profileService.UploadAvatarAsync(userId: UserId,
            file: file);
        return Ok(value: new { avatarUrl = newAvatarUrl });
    }
}