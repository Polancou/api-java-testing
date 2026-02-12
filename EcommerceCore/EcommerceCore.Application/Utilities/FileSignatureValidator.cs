namespace EcommerceCore.Application.Utilities;

/// <summary>
/// Proporciona métodos para validar la firma de archivos, especialmente imágenes,
/// para asegurar que el contenido del archivo coincide con su extensión declarada.
/// </summary>
public static class FileSignatureValidator
{
    /// <summary>
    /// Diccionario que almacena las firmas de bytes (magic numbers) para diferentes extensiones de archivo.
    /// Cada extensión puede tener múltiples firmas válidas.
    /// </summary>
    private static readonly Dictionary<string, List<byte[]>> _fileSignatures = new()
    {
        { ".jpeg", new List<byte[]>
            {
                // Firmas comunes para archivos JPEG
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, // JPEG JFIF
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 }, // JPEG Exif
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }, // JPEG Exif
            }
        },
        { ".jpg", new List<byte[]>
            {
                // Firmas comunes para archivos JPG (a menudo las mismas que JPEG)
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, // JPEG JFIF
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 }, // JPEG Exif
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }, // JPEG SPIFF
            }
        },
        { ".png", new List<byte[]>
            {
                // Firma estándar para archivos PNG
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
            }
        }
    };

    /// <summary>
    /// Valida si un archivo de imagen es legítimo basándose en su firma de bytes (magic number)
    /// y su extensión declarada.
    /// </summary>
    /// <param name="fileStream">El flujo de datos del archivo a validar.</param>
    /// <param name="extension">La extensión del archivo (ej. ".png", ".jpg").</param>
    /// <returns>
    /// <c>true</c> si el archivo es una imagen válida con la firma esperada para su extensión;
    /// de lo contrario, <c>false</c>.
    /// </returns>
    public static bool IsValidImage(Stream fileStream, string extension)
    {
        // Si la extensión es nula o vacía, no se puede validar.
        if (string.IsNullOrWhiteSpace(value: extension)) return false;
        
        // Normalizar la extensión a minúsculas para la búsqueda.
        var ext = extension.ToLower();
        // Si no tenemos firmas registradas para esta extensión, no podemos validarla.
        if (!_fileSignatures.ContainsKey(key: ext)) return false;

        // Asegurarse de que el flujo esté al principio para leer la firma.
        fileStream.Position = 0; 

        // Usar BinaryReader para leer los bytes del encabezado.
        // Se deja el flujo abierto para que pueda ser usado posteriormente.
        using var reader = new BinaryReader(input: fileStream,
            encoding: System.Text.Encoding.UTF8,
            leaveOpen: true);
        // Leer la cantidad máxima de bytes necesaria para cualquier firma de la extensión.
        var headerBytes = reader.ReadBytes(count: _fileSignatures[key: ext].Max(selector: m => m.Length));

        // Restablecer la posición del flujo al principio después de leer.
        fileStream.Position = 0; 

        // Verificar si alguno de los patrones de firma conocidos para la extensión
        // coincide con el inicio de los bytes leídos del archivo.
        return _fileSignatures[key: ext].Any(predicate: signature => 
            headerBytes.Take(count: signature.Length).SequenceEqual(second: signature)
        );
    }
}