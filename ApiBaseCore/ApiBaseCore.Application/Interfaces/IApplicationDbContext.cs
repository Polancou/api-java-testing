using ApiBaseCore.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ApiBaseCore.Application.Interfaces;

public interface IApplicationDbContext
{
    // Expone solo las colecciones de datos que la aplicación necesita
    DbSet<Usuario> Usuarios { get; }
    DbSet<UserLogin> UserLogins { get; }
    DbSet<Address> Addresses { get; }

    // Expone la habilidad de guardar cambios
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Soporte para transactions
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}