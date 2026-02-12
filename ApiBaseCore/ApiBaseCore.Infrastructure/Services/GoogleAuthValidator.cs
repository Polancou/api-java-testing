using ApiBaseCore.Application.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace ApiBaseCore.Infrastructure.Services;

public class GoogleAuthValidator(IConfiguration configuration) : IExternalAuthValidator
{
    /// <summary>
    /// Valida un token de ID de un proveedor externo y extrae la información del usuario.
    /// </summary>
    /// <param name="idToken">El token de ID proporcionado por el cliente. </param>
    /// <returns></returns>
    public async Task<ExternalAuthUserInfo> ValidateTokenAsync(string idToken)
    {
        // Obtenemos la clave pública de Google.
        var googleClientId = configuration[key: "Authentication:Google:ClientId"];
        // Creamos la configuración de validación para el token de Google.
        var settings = new GoogleJsonWebSignature.ValidationSettings()
        {
            Audience = new List<string> { googleClientId }
        };

        try
        {   
            // Validamos el token de Google.
            var payload = await GoogleJsonWebSignature.ValidateAsync(jwt: idToken,
                validationSettings: settings);
            // Si el token es válido, devolvemos la información del usuario.
            return new ExternalAuthUserInfo
            {
                ProviderSubjectId = payload.Subject,
                Email = payload.Email,
                Name = payload.Name,
                PictureUrl = payload.Picture
            };
        }
        catch (InvalidJwtException)
        {
            // Si el token no es válido, la librería lanza una excepción.
            // La manejamos devolviendo null para indicar el fallo.
            return null;
        }
    }
}