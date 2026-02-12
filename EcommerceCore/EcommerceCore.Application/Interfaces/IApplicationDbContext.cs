using EcommerceCore.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EcommerceCore.Application.Interfaces;

public interface IApplicationDbContext
{
    // Expone solo las colecciones de datos que la aplicación necesita
    DbSet<Usuario> Usuarios { get; }
    DbSet<UserLogin> UserLogins { get; }
    DbSet<Product> Products { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<ShippingAddress> ShippingAddresses { get; }
    DbSet<Review> Reviews { get; }
    DbSet<WishlistItem> WishlistItems { get; }

    // Expone la habilidad de guardar cambios
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // Soporte para transactions
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}