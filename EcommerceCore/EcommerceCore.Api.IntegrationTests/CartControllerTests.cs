using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceCore.Api.IntegrationTests;

public class CartControllerTests : IClassFixture<TestApiFactory>, IAsyncLifetime
{
    private readonly TestApiFactory _factory;
    private readonly HttpClient _client;
    private const string ApiVersion = "v1";

    public CartControllerTests(TestApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<(string Token, int ProductId)> SetupUserAndProductAsync()
    {
        // Create User
        var (_, token) = await _factory.CreateUserAndGetTokenAsync("User",
            "user@test.com",
            RolUsuario.User);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            token);

        // Create Product (need admin token temporarily or bypass controller)
        // We'll use a separate scope to add product directly to DB to avoid auth dance
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Infrastructure.Data.ApplicationDbContext>();
        var product = new Product("Test Prod",
            "Desc",
            100,
            10,
            "url",
            "Cat");
        context.Products.Add(product);
        await context.SaveChangesAsync();

        return (token, product.Id);
    }

    [Fact]
    public async Task GetCart_ShouldReturnOk()
    {
        // Arrange
        var (token, _) = await SetupUserAndProductAsync();

        // Act
        var response = await _client.GetAsync($"/api/{ApiVersion}/cart");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await response.Content.ReadFromJsonAsync<CartDto>();
        cart.Should().NotBeNull();
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task AddItem_ShouldReturnUpdatedCart()
    {
        // Arrange
        var (token, productId) = await SetupUserAndProductAsync();
        var dto = new AddToCartDto { ProductId = productId, Quantity = 2 };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/{ApiVersion}/cart/items",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await response.Content.ReadFromJsonAsync<CartDto>();
        cart.Should().NotBeNull();
        cart.Items.Should().HaveCount(1);
        cart.Items.First().ProductId.Should().Be(productId);
        cart.Items.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task RemoveItem_ShouldReturnUpdatedCart()
    {
        // Arrange
        var (token, productId) = await SetupUserAndProductAsync();
        // Add item first
        await _client.PostAsJsonAsync($"/api/{ApiVersion}/cart/items",
            new AddToCartDto
            {
                ProductId = productId,
                Quantity = 1
            });

        // Act
        var response = await _client.DeleteAsync($"/api/{ApiVersion}/cart/items/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await response.Content.ReadFromJsonAsync<CartDto>();
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task SyncCart_ShouldReturnMergedCart()
    {
        // Arrange
        var (token, productId) = await SetupUserAndProductAsync();
        var localItems = new List<AddToCartDto>
        {
            new() { ProductId = productId, Quantity = 5 }
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/{ApiVersion}/cart/sync",
            localItems);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await response.Content.ReadFromJsonAsync<CartDto>();
        cart.Items.Should().HaveCount(1);
        cart.Items.First().Quantity.Should().Be(5);
    }
}
