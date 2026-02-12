using AutoMapper;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Application.Services;
using EcommerceCore.Domain.Models;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace EcommerceCore.Application.UnitTests;

public class OrderServiceTests
{
    private readonly Mock<IApplicationDbContext> _mockDbContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _mockDbContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockEmailService = new Mock<IEmailService>();
        _orderService = new OrderService(_mockDbContext.Object, _mockMapper.Object, _mockEmailService.Object);
    }

    [Fact]
    public async Task GetUserOrdersAsync_ShouldReturnOrdersForUser()
    {
        // Arrange
        var userId = 1;
        var orders = new List<Order>
        {
            new(userId,
                100),
            new(2,
                200) // Other user
        };
        _mockDbContext.Setup(c => c.Orders).ReturnsDbSet(orders);
        _mockMapper.Setup(m => m.Map<IEnumerable<OrderDto>>(It.IsAny<List<Order>>()))
            .Returns(new List<OrderDto> { new() { TotalAmount = 100 } });

        // Act
        var result = await _orderService.GetUserOrdersAsync(userId);

        // Assert
        result.Should().HaveCount(1);
        result.First().TotalAmount.Should().Be(100);
    }

    [Fact]
    public async Task CreateOrderAsync_WithEmptyCart_ShouldThrowException()
    {
        // Arrange
        var userId = 1;
        var cart = new Cart(userId); // Empty items
        var carts = new List<Cart> { cart };
        _mockDbContext.Setup(c => c.Carts).ReturnsDbSet(carts);

        // Act
        Func<Task> act = async () => await _orderService.CreateOrderAsync(userId, new ShippingAddressDto());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("El carrito está vacío.");
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidCart_ShouldCreateOrderAndReduceStock()
    {
        // Arrange
        var userId = 1;
        var product = new Product("Prod 1",
            "Desc",
            100,
            10,
            "url",
            "Cat");

        var cart = new Cart(userId);
        // Reflection to add items directly or mock Include properly (Mocking Include is hard with Moq.EntityFrameworkCore, 
        // but ReturnsDbSet handles basic enumerables. For Includes, we might need more setup or rely on the fact that 
        // the service uses the navigation properties which are objects in memory).

        // Let's manually construct the object graph
        var cartItem = new CartItem(1,
            2); // 2 qty
        cartItem.GetType().GetProperty("Product")!.SetValue(cartItem,
            product);

        // Add item to cart using reflection or internal list if exposed, but Cart has private collection.
        // Cart.AddItem adds to internal list. But we need to ensure 'Items' property is populated.
        // Since we are mocking DbSet, we return a Cart that HAS items populated.

        // Re-creating cart with items populated via reflection or just setting up the object graph
        var cartWithItems = new Cart(userId);
        cartWithItems.Items.Add(cartItem);

        var carts = new List<Cart> { cartWithItems };
        _mockDbContext.Setup(c => c.Carts).ReturnsDbSet(carts);
        _mockDbContext.Setup(c => c.Orders).ReturnsDbSet(new List<Order>());

        var user = new Usuario("Test User", "test@example.com", "1234567890", RolUsuario.User);
        user.GetType().GetProperty("Id")!.SetValue(user, userId);

        _mockDbContext.Setup(c => c.Usuarios).ReturnsDbSet(new List<Usuario> { user });
        _mockDbContext.Setup(c => c.Usuarios.FindAsync(userId)).ReturnsAsync(user);

        _mockMapper.Setup(m => m.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(new OrderDto { TotalAmount = 200 });

        // Act
        var result = await _orderService.CreateOrderAsync(userId, new ShippingAddressDto());

        // Assert
        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(200); // 100 * 2
        product.Stock.Should().Be(8); // 10 - 2

        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithValidId_ShouldUpdateStatus()
    {
        // Arrange
        var orderId = 1;
        var order = new Order(1,
            100);
        // Reflection to set ID since it's private set
        order.GetType().GetProperty("Id")!.SetValue(order,
            orderId);

        var orders = new List<Order> { order };
        _mockDbContext.Setup(c => c.Orders.FindAsync(orderId)).ReturnsAsync(order);

        // Act
        await _orderService.UpdateOrderStatusAsync(orderId,
            OrderStatus.Shipped);

        // Assert
        order.Status.Should().Be(OrderStatus.Shipped);
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var orderId = 99;
        _mockDbContext.Setup(c => c.Orders.FindAsync(orderId)).ReturnsAsync((Order?)null);

        // Act
        Func<Task> act = async () => await _orderService.UpdateOrderStatusAsync(orderId,
            OrderStatus.Delivered);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Pedido no encontrado.");
    }
}
