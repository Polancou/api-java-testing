using ApiBaseCore.Application.DTOs;
using ApiBaseCore.Application.Exceptions;
using ApiBaseCore.Application.Interfaces;
using ApiBaseCore.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace ApiBaseCore.Application.Services;

public class AuthService(
    IApplicationDbContext context,
    ITokenService tokenService,
    IExternalAuthValidator externalAuthValidator,
    IEmailService emailService,
    IConfiguration configuration,
    IEncryptionService encryptionService) : IAuthService
{
    public async Task<AuthResult> RegisterAsync(RegistroUsuarioDto registroDto)
    {
        var usuarioExistente = await context.Usuarios.AnyAsync(u => u.Email.ToLower() == registroDto.Email.ToLower());
    
        if (usuarioExistente)
        {
            return AuthResult.Ok(null, "Si el correo es válido, recibirás un enlace de confirmación.");
        }

        // AES Encrypt Password
        var passwordEncrypted = encryptionService.Encrypt(registroDto.Password);
        var verificationToken = tokenService.GenerarRefreshToken(); 

        var nuevoUsuario = new Usuario(
            name: registroDto.Name, 
            email: registroDto.Email, 
            phone: registroDto.Phone,
            rol: RolUsuario.User,
            taxId: registroDto.TaxId);
    
        nuevoUsuario.EstablecerPasswordHash(passwordEncrypted);
        nuevoUsuario.SetEmailVerificationToken(verificationToken);

        await context.Usuarios.AddAsync(nuevoUsuario);
        await context.SaveChangesAsync(); 

        await SendVerificationEmailAsync(nuevoUsuario);

        return AuthResult.Ok(null, "Si el correo es válido, recibirás un enlace de confirmación.");
    }

    public async Task<TokenResponseDto> LoginAsync(LoginUsuarioDto loginDto)
    {
        var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (usuario == null || usuario.PasswordHash == null)
        {
             throw new ValidationException("Credenciales inválidas.");
        }
        
        // Decrypt stored password and compare
        var decryptedPassword = encryptionService.Decrypt(usuario.PasswordHash);
        if (decryptedPassword != loginDto.Password)
        {
            throw new ValidationException("Credenciales inválidas.");
        }

        var accessToken = tokenService.CrearToken(usuario);
        var refreshToken = tokenService.GenerarRefreshToken();
        
        usuario.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(30));
        await context.SaveChangesAsync();

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<TokenResponseDto> ExternalLoginAsync(ExternalLoginDto externalLoginDto)
    {
        if (externalLoginDto.Provider.ToLower() != "google")
        {
            throw new ValidationException("Proveedor no soportado.");
        }
        
        var userInfo = await externalAuthValidator.ValidateTokenAsync(externalLoginDto.IdToken);
        if (userInfo == null)
        {
            throw new ValidationException("Token externo inválido.");
        }

        await using var transaction = await context.BeginTransactionAsync();
        try
        {
            var userLogin = await context.UserLogins.Include(ul => ul.Usuario)
                .FirstOrDefaultAsync(ul => ul.LoginProvider == "Google" && ul.ProviderKey == userInfo.ProviderSubjectId);

            Usuario usuario;

            if (userLogin != null)
            {
                usuario = userLogin.Usuario;
            }
            else
            {
                usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == userInfo.Email.ToLower());

                if (usuario == null)
                {
                    usuario = new Usuario(
                        name: userInfo.Name,
                        email: userInfo.Email,
                        phone: "",
                        rol: RolUsuario.User);
                    
                    usuario.SetAvatarUrl(userInfo.PictureUrl);
                    usuario.MarkEmailAsVerified();
                    
                    await context.Usuarios.AddAsync(usuario);
                    await context.SaveChangesAsync();
                }

                var nuevoLogin = new UserLogin(
                    loginProvider: "Google",
                    providerKey: userInfo.ProviderSubjectId,
                    usuario: usuario);
                
                if (string.IsNullOrEmpty(usuario.AvatarUrl))
                {
                     usuario.SetAvatarUrl(userInfo.PictureUrl);
                }

                await context.UserLogins.AddAsync(nuevoLogin);
                await context.SaveChangesAsync();
            }

            var accessToken = tokenService.CrearToken(usuario);
            var refreshToken = tokenService.GenerarRefreshToken();
            
            usuario.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(30));
            await context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        
        if (usuario == null)
        {
            throw new ValidationException("Refresh token inválido.");
        }

        if (usuario.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new ValidationException("Refresh token expirado.");
        }

        var newAccessToken = tokenService.CrearToken(usuario);
        var newRefreshToken = tokenService.GenerarRefreshToken();
        
        usuario.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(30));
        await context.SaveChangesAsync();

        return new TokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task<AuthResult> VerifyEmailAsync(string token)
    {
        var decodedToken = WebUtility.UrlDecode(token);
        var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
        
        if (usuario == null)
        {
            return AuthResult.Fail("Token de verificación inválido.");
        }

        usuario.MarkEmailAsVerified();
        await context.SaveChangesAsync();
        
        return AuthResult.Ok(null, "Email verificado exitosamente.");
    }

    public async Task<AuthResult> ForgotPasswordAsync(string email)
    {
        var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (usuario != null)
        {
            var resetToken = tokenService.GenerarRefreshToken();
            usuario.SetPasswordResetToken(resetToken, DateTime.UtcNow.AddHours(1));
            await context.SaveChangesAsync();
            await SendPasswordResetEmailAsync(usuario);
        }

        return AuthResult.Ok(null, "Si existe una cuenta con ese correo, se ha enviado un enlace para restablecer la contraseña.");
    }

    public async Task<AuthResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var decodedToken = WebUtility.UrlDecode(dto.Token);
        var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.PasswordResetToken == decodedToken);

        if (usuario == null)
        {
            return AuthResult.Fail("El token de restablecimiento no es válido.");
        }

        if (usuario.PasswordResetTokenExpiryTime <= DateTime.UtcNow)
        {
            return AuthResult.Fail("El token de restablecimiento ha expirado.");
        }

        // AES Encrypt new password
        var newPasswordEncrypted = encryptionService.Encrypt(dto.NewPassword);
        usuario.EstablecerPasswordHash(newPasswordEncrypted);
        usuario.ClearPasswordResetToken();

        await context.SaveChangesAsync();

        return AuthResult.Ok(null, "Contraseña restablecida exitosamente.");
    }
    
    #region Private Methods

    private async Task SendPasswordResetEmailAsync(Usuario usuario)
    {
        var frontendBaseUrl = configuration["AppSettings:FrontendBaseUrl"];
        var encodedToken = WebUtility.UrlEncode(usuario.PasswordResetToken);
        var resetLink = $"{frontendBaseUrl}/reset-password?token={encodedToken}";
        try
        {
            await emailService.SendPasswordResetEmailAsync(
                toEmail: usuario.Email, 
                userName: usuario.Name, 
                resetLink: resetLink);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar email de reseteo: {ex.Message}");
        }
    }

    private async Task SendVerificationEmailAsync(Usuario usuario)
    {
        var frontendBaseUrl = configuration["AppSettings:FrontendBaseUrl"];
        var encodedToken = WebUtility.UrlEncode(usuario.EmailVerificationToken);
        var verificationLink = $"{frontendBaseUrl}/verify-email?token={encodedToken}";
        try
        {
            await emailService.SendVerificationEmailAsync(
                toEmail: usuario.Email, 
                userName: usuario.Name, 
                verificationLink: verificationLink);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar email de verificación: {ex.Message}");
        }
    }
    
    #endregion
}