using AutoMapper;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Application.Services;
using EcommerceCore.Domain.Models;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;

namespace EcommerceCore.Application.UnitTests;

public class CartServiceTests
{
    private readonly Mock<IApplicationDbContext> _mockDbContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CartService _cartService;

    public CartServiceTests()
    {
        _mockDbContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _cartService = new CartService(_mockDbContext.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetCartAsync_WhenCartExists_ShouldReturnCartDto()
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

        // Setup cart with item
        var cartItem = new CartItem(1,
            1);
        cartItem.GetType().GetProperty("Product")!.SetValue(cartItem,
            product);

        cart.Items.Add(cartItem);

        _mockDbContext.Setup(c => c.Carts).ReturnsDbSet(new List<Cart> { cart });

        _mockMapper.Setup(m => m.Map<CartDto>(It.IsAny<Cart>()))
            .Returns(new CartDto
            {
                Items = new List<CartItemDto>
                {
                    new() { ProductName = "Prod 1", Quantity = 1 }
                }
            });

        // Act
        var result = await _cartService.GetCartAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().ProductName.Should().Be("Prod 1");
    }

    [Fact]
    public async Task AddItemAsync_ShouldAddItemAndReturnUpdatedCart()
    {
        // Arrange
        var userId = 1;
        var dto = new AddToCartDto { ProductId = 1, Quantity = 1 };
        var product = new Product("Prod 1", "Desc", 100, 10, "url", "Cat");

        var cart = new Cart(userId);
        var carts = new List<Cart> { cart };

        _mockDbContext.Setup(c => c.Carts).ReturnsDbSet(carts);
        _mockDbContext.Setup(c => c.Products.FindAsync(1)).ReturnsAsync(product);

        _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback((CancellationToken ct) =>
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == 1);
                if (item != null)
                {
                    item.GetType().GetProperty("Product")!.SetValue(item, product);
                }
            })
            .Returns(Task.FromResult(1));

        _mockMapper.Setup(m => m.Map<CartDto>(It.IsAny<Cart>()))
            .Returns(new CartDto
            {
                Items = new List<CartItemDto>
                {
                    new() { ProductName = "Prod 1", Quantity = 1 }
                }
            });

        // Act
        var result = await _cartService.AddItemAsync(userId, dto);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().ProductName.Should().Be("Prod 1");
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldRemoveItem()
    {
        // Arrange
        var userId = 1;
        var cart = new Cart(userId);
        var product = new Product("Prod 1", "Desc", 100, 10, "url", "Cat");

        var cartItem = new CartItem(1, 1);
        cartItem.GetType().GetProperty("Product")!.SetValue(cartItem, product);
        cart.Items.Add(cartItem);

        _mockDbContext.Setup(c => c.Carts).ReturnsDbSet(new List<Cart> { cart });

        _mockMapper.Setup(m => m.Map<CartDto>(It.IsAny<Cart>()))
            .Returns(new CartDto { Items = new List<CartItemDto>() });

        // Act
        var result = await _cartService.RemoveItemAsync(userId, 1);

        // Assert
        result.Items.Should().BeEmpty();
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
