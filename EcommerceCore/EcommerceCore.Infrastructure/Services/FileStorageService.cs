using EcommerceCore.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace EcommerceCore.Infrastructure.Services;

/// <summary>
/// Implementación de <see cref="IFileStorageService"/> para almacenar archivos localmente en el sistema de archivos del servidor.
/// </summary>
/// <param name="env">Entorno de alojamiento web para obtener la ruta raíz de la aplicación.</param>
public class LocalFileStorageService(IWebHostEnvironment env) : IFileStorageService
{
    // Nombre del contenedor o subdirectorio donde se guardarán los archivos de avatares.
    private const string ContainerName = "uploads/avatars";

    /// <summary>
    /// Guarda un archivo de forma asíncrona en el sistema de archivos local.
    /// </summary>
    /// <param name="fileStream">El flujo de datos del archivo a guardar.</param>
    /// <param name="fileName">El nombre del archivo, incluyendo su extensión.</param>
    /// <returns>Una tarea que representa la operación de guardado, que devuelve la URL relativa del archivo guardado.</returns>
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
    {
        // 1. Definir la ruta física completa en el servidor donde se almacenará el archivo.
        // Combina la ruta raíz del contenido web (wwwroot) con el nombre del contenedor.
        var uploadsPath = Path.Combine(path1: env.WebRootPath,
            path2: ContainerName);
        // Combina la ruta de subida con el nombre del archivo para obtener la ruta completa.
        var filePath = Path.Combine(path1: uploadsPath,
            path2: fileName);
        // Extrae el directorio del archivo para asegurar que exista.
        var directoryPath = Path.GetDirectoryName(path: filePath);

        // Si el directorio no existe, lo crea.
        if (directoryPath != null && !Directory.Exists(path: directoryPath))
        {
            Directory.CreateDirectory(path: directoryPath);
        }

        // 2. Guardar el archivo físicamente en la ubicación definida.
        // Abre un FileStream para escribir el archivo, creando uno nuevo si no existe o sobrescribiéndolo.
        using (var outputStream = new FileStream(path: filePath,
                   mode: FileMode.Create))
        {
            // Copia el contenido del flujo de entrada al flujo de salida de forma asíncrona.
            await fileStream.CopyToAsync(destination: outputStream);
        }

        // 3. Retornar la URL relativa del archivo, que se utilizará para acceder a él desde el cliente.
        // Formatea la URL usando el nombre del contenedor y el nombre del archivo.
        return $"/{ContainerName}/{fileName}";
    }

    /// <summary>
    /// Elimina un archivo del sistema de archivos local basándose en su ruta relativa.
    /// </summary>
    /// <param name="fileRoute">La ruta relativa del archivo a eliminar (ej. "/uploads/avatars/foto.jpg").</param>
    /// <returns>Una tarea que representa la operación de eliminación.</returns>
    public Task DeleteFileAsync(string fileRoute)
    {
        // Si la ruta del archivo es nula o vacía, no hay nada que eliminar.
        if (string.IsNullOrEmpty(value: fileRoute)) return Task.CompletedTask;

        // Convertir la URL relativa (ej. "/uploads/avatars/foto.jpg") a una ruta física del sistema de archivos.
        // Quitar la barra inicial y reemplazar los separadores de ruta web ('/') por los separadores del sistema operativo.
        var relativePath = fileRoute.TrimStart(trimChar: '/').Replace(oldValue: "/",
            newValue: Path.DirectorySeparatorChar.ToString());
        // Combina la ruta raíz del contenido web con la ruta relativa para obtener la ruta física completa.
        var filePath = Path.Combine(path1: env.WebRootPath,
            path2: relativePath);

        // Verificar si el archivo existe antes de intentar eliminarlo.
        if (File.Exists(path: filePath))
        {
            // Eliminar el archivo.
            File.Delete(path: filePath);
        }

        // Retorna una tarea completa ya que la operación es síncrona o se ha manejado la ausencia del archivo.
        return Task.CompletedTask;
    }
}
