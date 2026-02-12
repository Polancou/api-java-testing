using AutoMapper;
using ApiBaseCore.Application.DTOs;
using ApiBaseCore.Application.Interfaces;
using ApiBaseCore.Application.Services;
using ApiBaseCore.Domain.Models;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Http;
using ApiBaseCore.Application.Exceptions;
using Moq.EntityFrameworkCore;

namespace ApiBaseCore.Application.UnitTests;

/// <summary>
/// Contiene las pruebas unitarias para la clase ProfileService.
/// El objetivo es verificar la lógica de negocio de ProfileService de forma aislada,
/// simulando sus dependencias (IApplicationDbContext e IMapper) para asegurar
/// que solo probamos la lógica interna del servicio.
/// </summary>
public class ProfileServiceTests
{
    // Mocks para los contratos (interfaces) que ProfileService necesita.
    private readonly Mock<IApplicationDbContext> _mockDbContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IFileStorageService> _mockFileStorage;
    private readonly Mock<IEncryptionService> _mockEncryptionService;

    // La instancia real del servicio que vamos a probar.
    private readonly IProfileService _profileService;

    public ProfileServiceTests()
    {
        // --- Arrange Global (Preparación antes de cada prueba) ---
        _mockDbContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockFileStorage = new Mock<IFileStorageService>();
        _mockEncryptionService = new Mock<IEncryptionService>();

        // Se inyectan los mocks en la implementación concreta del servicio.
        _profileService = new ProfileService(
            context: _mockDbContext.Object,
            mapper: _mockMapper.Object,
            fileStorageService: _mockFileStorage.Object,
            encryptionService: _mockEncryptionService.Object
        );
    }

    #region Pruebas para GetProfileByIdAsync



    /// <summary>
    /// Prueba el "camino feliz": cuando se solicita un usuario que existe,
    /// el servicio debe encontrarlo y mapearlo a un DTO de perfil.
    /// </summary>
    [Fact]
    public async Task GetProfileByIdAsync_WhenUserExists_ShouldReturnProfileDto()
    {
        // --- Arrange (Preparar) ---
        var userId = Guid.NewGuid();
        var usuario = new Usuario(
            name: "Usuario de Prueba",
            email: "test@email.com",
            phone: "123",
            rol: RolUsuario.User);
        var perfilDto = new PerfilUsuarioDto { Id = userId, Name = "Usuario de Prueba" };
        var usuarios = new List<Usuario> { usuario };

        // 1. Se configura el mock del DbContext para que devuelva el usuario cuando se le busque por ID.
        // Usamos FindAsync, que es lo que el servicio utiliza internamente.
        // Nota: para pruebas unitarias con mocks de EF, FindAsync puede complicarse
        // si no usamos un setup específico, pero con FirstOrDefaultAsync (normalmente usado en el servicio)
        // se puede mockear el DbSet. Sin embargo, el servicio usa FindAsync o FirstOrDefault.
        // Asumiendo que el servicio usa `FirstOrDefault(x => x.Id == id)` o similar.
        // En este caso, el servicio usa `FirstOrDefaultAsync(u => u.Id == userId)`.
        
        // Moficamos la lista para que contenga el usuario con el ID correcto (aunque el ID se asigna privado,
        // tendríamos que usar reflexión o asumir que el mock devuelve el usuario independientemente de la query
        // si usamos ReturnsDbSet).
        
        // Para simplificar, vamos a usar reflexión para setear el ID del usuario si es necesario,
        // o simplemente mockear el comportamiento del DbSet si usamos una librería que soporte linq.
        // Moq.EntityFrameworkCore soporta esto.
        
        // Pero espera, el Usuario.Id es set privado y se genera en el constructor o por la BD.
        // Como es Guid, es posible que sea default o algo.
        // Vamos a usar reflection para forzar el Id del usuario para que coincida con userId.
        var prop = typeof(Usuario).GetProperty("Id");
        prop.SetValue(usuario, userId);

        _mockDbContext.Setup(expression: c => c.Usuarios).ReturnsDbSet(usuarios);

        // 2. Se configura el mock de AutoMapper para que devuelva el DTO esperado cuando mapee el usuario.
        _mockMapper.Setup(expression: m => m.Map<PerfilUsuarioDto>(usuario)).Returns(value: perfilDto);

        // --- Act (Actuar) ---
        var result = await _profileService.GetProfileByIdAsync(userId: userId);

        // --- Assert (Verificar) ---
        // Se comprueba que el resultado no sea nulo y que sea el DTO que configuramos.
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectation: perfilDto);
    }

    /// <summary>
    /// Prueba el caso de error: cuando se solicita un usuario que no existe,
    /// el servicio debe lanzar NotFoundException.
    /// </summary>
    [Fact]
    public async Task GetProfileByIdAsync_WhenUserDoesNotExist_ShouldThrowNotFoundException() // <-- CAMBIADO
    {
        // --- Arrange (Preparar) ---
        var userId = Guid.NewGuid(); // Un ID que sabemos que no existirá.

        _mockDbContext.Setup(c => c.Usuarios).ReturnsDbSet(new List<Usuario>());

        // --- Act (Actuar) ---
        // Usamos Func<Task> para capturar la acción que debe lanzar la excepción
        Func<Task> act = async () => await _profileService.GetProfileByIdAsync(userId: userId);

        // --- Assert (Verificar) ---
        // Verificamos que la acción lanza la excepción correcta con el mensaje correcto
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Usuario no encontrado.");
    }

    #endregion

    #region Pruebas para ActualizarPerfilAsync

    /// <summary>
    /// Prueba el "camino feliz": si el usuario existe, el servicio debe actualizar
    /// sus propiedades y guardar los cambios.
    /// </summary>
    [Fact]
    public async Task ActualizarPerfilAsync_WhenUserExists_ShouldUpdateAndSaveChanges()
    {
        // --- Arrange (Preparar) ---
        var userId = Guid.NewGuid();
        var usuario = new Usuario(name: "Nombre Original",
            email: "test@email.com",
            phone: "123",
            rol: RolUsuario.User);
        
        var updateDto = new ActualizarPerfilDto { Name = "Nombre Actualizado", Phone = "987" };

        var prop = typeof(Usuario).GetProperty("Id");
        prop.SetValue(usuario, userId);

        // Se configura el mock para que devuelva el usuario existente.
        _mockDbContext.Setup(expression: c => c.Usuarios.FindAsync(userId)).ReturnsAsync(value: usuario);

        // --- Act (Actuar) ---
        var result = await _profileService.ActualizarPerfilAsync(userId: userId,
            perfilDto: updateDto);

        // --- Assert (Verificar) ---
        // 1. Se comprueba que el método devuelva 'true', indicando éxito.
        result.Should().BeTrue();

        // 2. Se comprueba que las propiedades del objeto 'usuario' en memoria hayan sido actualizadas.
        usuario.Name.Should().Be(expected: updateDto.Name);
        usuario.Phone.Should().Be(expected: updateDto.Phone);

        // 3. Se verifica que se intentó guardar los cambios en la base de datos,
        // lo cual es una parte crucial de la lógica del servicio.
        _mockDbContext.Verify(expression: c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            times: Times.Once);
    }

    /// <summary>
    /// Prueba el caso de error: si se intenta actualizar un usuario que no existe,
    /// el servicio debe lanzar NotFoundException.
    /// </summary>
    [Fact]
    public async Task ActualizarPerfilAsync_WhenUserDoesNotExist_ShouldThrowNotFoundException() // <-- CAMBIADO
    {
        // --- Arrange (Preparar) ---
        var userId = Guid.NewGuid();
        var updateDto = new ActualizarPerfilDto { Name = "Test", Phone = "123" };

        _mockDbContext.Setup(c => c.Usuarios.FindAsync(userId)).ReturnsAsync((Usuario)null);

        // --- Act (Actuar) ---
        Func<Task> act = async () => await _profileService.ActualizarPerfilAsync(userId: userId,
            perfilDto: updateDto);

        // --- Assert (Verificar) ---
        // 1. Verificamos que lanza la excepción
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Usuario no encontrado.");

        // 2. Se verifica que NUNCA se intentó guardar cambios.
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            times: Times.Never);
    }

    #endregion

    #region Pruebas para CambiarPasswordAsync

    /// <summary>
    /// Prueba el "camino feliz": un usuario existente con la contraseña antigua correcta
    /// debería poder actualizar su contraseña.
    /// </summary>
    [Fact]
    public async Task CambiarPasswordAsync_WhenOldPasswordIsCorrect_ShouldUpdatePassword()
    {
        // --- Arrange (Preparar) ---
        var userId = Guid.NewGuid();
        var oldPassword = "PasswordAntigua123!";
        var newPassword = "PasswordNueva456!";
        var user = new Usuario("Test User",
            "test@email.com",
            "123",
            RolUsuario.User);

        // Configurar Mocks
        var oldHash = "EncryptedOld";
        var newHash = "EncryptedNew";
        
        user.EstablecerPasswordHash(oldHash);
        
        _mockEncryptionService.Setup(e => e.Decrypt(oldHash)).Returns(oldPassword);
        _mockEncryptionService.Setup(e => e.Encrypt(newPassword)).Returns(newHash);


        var dto = new CambiarPasswordDto
        {
            OldPassword = oldPassword,
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        };

        // Configura el mock para que devuelva el usuario
        _mockDbContext.Setup(c => c.Usuarios.FindAsync(userId)).ReturnsAsync(user);

        // --- Act (Actuar) ---
        var result = await _profileService.CambiarPasswordAsync(userId,
            dto);

        // --- Assert (Verificar) ---
        // 1. Verifica que la operación fue exitosa
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Contraseña actualizada exitosamente.");

        // 2. Verifica que el hash de la contraseña en la entidad fue actualizado
        user.PasswordHash.Should().Be(newHash);

        // 3. Verifica que se llamó a SaveChanges
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Prueba el caso de error: si el usuario proporciona una contraseña antigua incorrecta,
    /// la operación debe fallar lanzando una ValidationException.
    /// </summary>
    [Fact]
    public async Task CambiarPasswordAsync_WhenOldPasswordIsIncorrect_ShouldThrowValidationException() // <-- CAMBIADO
    {
        // --- Arrange (Preparar) ---
        var userId = Guid.NewGuid();
        var correctOldPassword = "PasswordAntigua123!";
        var wrongOldPassword = "PasswordEquivocadaXXX";

        var user = new Usuario("Test User",
            "test@email.com",
            "123",
            RolUsuario.User);
        
        var oldHash = "EncryptedOld";
        user.EstablecerPasswordHash(oldHash);
        
        // Decrypt devuelve la correcta
        _mockEncryptionService.Setup(e => e.Decrypt(oldHash)).Returns(correctOldPassword);

        var dto = new CambiarPasswordDto
        {
            OldPassword = wrongOldPassword, // <- Contraseña incorrecta
            NewPassword = "newPassword456!",
            ConfirmPassword = "newPassword456!"
        };

        _mockDbContext.Setup(c => c.Usuarios.FindAsync(userId)).ReturnsAsync(user);

        // --- Act (Actuar) ---
        Func<Task> act = async () => await _profileService.CambiarPasswordAsync(userId,
            dto);

        // --- Assert (Verificar) ---
        // 1. Verifica que la operación falló con la excepción y mensaje correctos
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("La contraseña actual es incorrecta.");

        // 2. Verifica que NO se llamó a SaveChanges
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Pruebas para UploadAvatarAsync

    [Fact]
    public async Task UploadAvatarAsync_WhenUserExists_ShouldSaveFileAndUpdateUser()
    {
        // --- Arrange (Preparar) ---
        var userId = Guid.NewGuid();
        var fakeFileUrl = "/uploads/avatars/fake-guid.jpg";
        var usuario = new Usuario(name: "Test User",
            email: "test@email.com",
            phone: "123",
            rol: RolUsuario.User);

        // Simula un IFormFile
        var mockFileStream = new MemoryStream("fake-image-data"u8.ToArray());
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.jpg");
        mockFile.Setup(f => f.Length).Returns(mockFileStream.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(mockFileStream);

        // Configura el DbContext para encontrar al usuario
        _mockDbContext.Setup(c => c.Usuarios.FindAsync(userId)).ReturnsAsync(usuario);

        // Configura el FileStorage para que devuelva la URL falsa
        _mockFileStorage.Setup(s => s.SaveFileAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>())) // Acepta cualquier stream y nombre de archivo
            .ReturnsAsync(fakeFileUrl);

        // --- Act (Actuar) ---
        var resultUrl = await _profileService.UploadAvatarAsync(userId: userId,
            file: mockFile.Object);

        // --- Assert (Verificar) ---
        // 1. Verifica que la URL devuelta sea la esperada
        resultUrl.Should().Be(fakeFileUrl);

        // 2. Verifica que la entidad Usuario fue actualizada con la nueva URL
        usuario.AvatarUrl.Should().Be(fakeFileUrl);

        // 3. Verifica que el servicio de almacenamiento fue llamado
        _mockFileStorage.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(),
                It.IsAny<string>()),
            Times.Once);

        // 4. Verifica que los cambios se guardaron en la BD
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}