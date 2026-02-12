using ApiBaseCore.Application.DTOs;
using ApiBaseCore.Application.Exceptions;
using ApiBaseCore.Application.Interfaces;
using ApiBaseCore.Domain.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ApiBaseCore.Application.Services;

public class ProfileService(IApplicationDbContext context, IMapper mapper, IFileStorageService fileStorageService, IEncryptionService encryptionService) : IProfileService
{
    public async Task<PerfilUsuarioDto> GetProfileByIdAsync(Guid userId)
    {
        var usuario = await context.Usuarios.Include(u => u.Addresses).FirstOrDefaultAsync(u => u.Id == userId);
        if (usuario == null)
            throw new NotFoundException("Usuario no encontrado.");

        return mapper.Map<PerfilUsuarioDto>(usuario);
    }
    
    public async Task<bool> ActualizarPerfilAsync(Guid userId, ActualizarPerfilDto perfilDto)
    {
        var usuario = await context.Usuarios.FindAsync(userId);

        if (usuario == null)
        {
            throw new NotFoundException("Usuario no encontrado.");
        }
        
        usuario.UpdateProfile(perfilDto.Name!, perfilDto.Phone!, perfilDto.TaxId);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ValidationException("Este usuario fue modificado por otra persona. Por favor, recarga la página e intenta de nuevo.");
        }
        
        return true;
    }

    public async Task<AuthResult> CambiarPasswordAsync(Guid userId, CambiarPasswordDto dto)
    {
        var usuario = await context.Usuarios.FindAsync(userId);
        if (usuario == null)
        {
            return AuthResult.Fail("Usuario no encontrado.");
        }

        if (string.IsNullOrEmpty(usuario.PasswordHash))
        {
            throw new ValidationException("No puedes cambiar la contraseña de una cuenta de inicio de sesión externo.");
        }

        // Verify old password (AES Decrypt)
        var decryptedCurrentPassword = encryptionService.Decrypt(usuario.PasswordHash);
        if (decryptedCurrentPassword != dto.OldPassword)
        {
            throw new ValidationException("La contraseña actual es incorrecta.");
        }

        // Set new password (AES Encrypt)
        var newPasswordEncrypted = encryptionService.Encrypt(dto.NewPassword);
        usuario.EstablecerPasswordHash(newPasswordEncrypted);

        await context.SaveChangesAsync();

        return AuthResult.Ok(null, "Contraseña actualizada exitosamente.");
    }

    public async Task<string> UploadAvatarAsync(Guid userId, IFormFile file)
    {
        var usuario = await context.Usuarios.FindAsync(userId);
        if (usuario == null) throw new NotFoundException("Usuario no encontrado.");
        
        if (!string.IsNullOrEmpty(usuario.AvatarUrl)) await fileStorageService.DeleteFileAsync(usuario.AvatarUrl);
        
        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        string fileUrl;
        await using (var stream = file.OpenReadStream())
        {
            fileUrl = await fileStorageService.SaveFileAsync(stream, uniqueFileName);
        }
        
        usuario.SetAvatarUrl(fileUrl);
        
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ValidationException("Este usuario fue modificado por otra persona. Por favor, recarga la página e intenta de nuevo.");
        }

        return fileUrl;
    }
}