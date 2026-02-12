using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ApiBaseCore.Application.Interfaces;
using ApiBaseCore.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ApiBaseCore.Infrastructure.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    public string CrearToken(Usuario usuario)
    {
        // Los 'claims' son piezas de información sobre el usuario que queremos incluir en el token.
        var claims = new List<Claim>
        {
            // El ID del usuario es el claim más importante para identificarlo.
            // Usamos JwtRegisteredClaimNames.Sub para asegurar que se mapee correctamente a 'sub' en el JWT
            // y luego a ClaimTypes.NameIdentifier por el middleware de autenticación.
            new Claim(type: JwtRegisteredClaimNames.Sub,
                value: usuario.Id.ToString()),
            // También incluimos el email.
            new Claim(type: JwtRegisteredClaimNames.Email,
                value: usuario.Email),
             // JTI (JWT ID) para identificar el token de forma única
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // Y el rol, para poder hacer autorizaciones en el futuro.
            new Claim(type: ClaimTypes.Role,
                value: usuario.Rol.ToString())
        };

        // Obtenemos la clave secreta desde nuestra configuración.
        var key = new SymmetricSecurityKey(key: Encoding.UTF8.GetBytes(s: config[key: "Jwt:Key"] ?? throw new InvalidOperationException(message: "Jwt key no configurado")));
        // Creamos las credenciales de firma usando la clave y el algoritmo de seguridad más fuerte.
        var creds = new SigningCredentials(key: key,
            algorithm: SecurityAlgorithms.HmacSha512Signature);

        // Creamos el descriptor del token, que une toda la información.
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims: claims),
            Expires = DateTime.UtcNow.AddMinutes(value: 15),
            SigningCredentials = creds,
            Issuer = config[key: "Jwt:Issuer"]
        };

        // Usamos un manejador para crear el token basado en el descriptor.
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor: tokenDescriptor);

        // Finalmente, escribimos el token como un string. Este es el string que enviaremos al cliente.
        return tokenHandler.WriteToken(token: token);
    }

    /// <summary>
    /// Genera un token de actualización (Refresh Token) aleatorio y seguro.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public string GenerarRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(data: randomNumber);
        return Convert.ToBase64String(inArray: randomNumber);
    }
}