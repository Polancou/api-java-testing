using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceCore.Api.IntegrationTests;

public class OrdersControllerTests : IClassFixture<TestApiFactory>, IAsyncLifetime
{
    private readonly TestApiFactory _factory;
    private readonly HttpClient _client;
    private const string ApiVersion = "v1";
    private readonly JsonSerializerOptions _jsonOptions;

    public OrdersControllerTests(TestApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<(string Token, int ProductId)> SetupUserAndProductAsync()
    {
        var (_, token) = await _factory.CreateUserAndGetTokenAsync("User",
            "user@test.com",
            RolUsuario.User);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            token);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<EcommerceCore.Infrastructure.Data.ApplicationDbContext>();
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
    public async Task CreateOrder_WithValidCart_ShouldReturnCreated()
    {
        // Arrange
        var (token, productId) = await SetupUserAndProductAsync();

        // Add item to cart
        var addItemResponse = await _client.PostAsJsonAsync($"/api/{ApiVersion}/cart/items",
            new AddToCartDto { ProductId = productId, Quantity = 2 });
        addItemResponse.EnsureSuccessStatusCode();

        // Act
        // Act
        var address = new ShippingAddressDto
        {
            Name = "Test User",
            AddressLine1 = "123 Test St",
            City = "Test City",
            State = "TS",
            PostalCode = "12345",
            Country = "Test Country"
        };
        var response = await _client.PostAsJsonAsync($"/api/{ApiVersion}/orders", address);

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var error = await response.Content.ReadAsStringAsync();
            // Force failure with error message
            response.StatusCode.Should().Be(HttpStatusCode.Created, $"Error: {error}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
        order.Should().NotBeNull();
        order.TotalAmount.Should().Be(200); // 100 * 2
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateOrder_WithEmptyCart_ShouldReturnBadRequest()
    {
        // Arrange
        var (token, _) = await SetupUserAndProductAsync();
        // Cart is empty by default

        // Act
        // Act
        var address = new ShippingAddressDto
        {
            Name = "Test User",
            AddressLine1 = "123 Test St",
            City = "Test City",
            State = "TS",
            PostalCode = "12345",
            Country = "Test Country"
        };
        var response = await _client.PostAsJsonAsync($"/api/{ApiVersion}/orders", address);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserOrders_ShouldReturnList()
    {
        // Arrange
        var (token, productId) = await SetupUserAndProductAsync();

        // Create an order first
        await _client.PostAsJsonAsync($"/api/{ApiVersion}/cart/items",
            new AddToCartDto { ProductId = productId, Quantity = 1 });
        var address = new ShippingAddressDto
        {
            Name = "Test User",
            AddressLine1 = "123 Test St",
            City = "Test City",
            State = "TS",
            PostalCode = "12345",
            Country = "Test Country"
        };
        await _client.PostAsJsonAsync($"/api/{ApiVersion}/orders", address);

        // Act
        var response = await _client.GetAsync($"/api/{ApiVersion}/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>(_jsonOptions);
        orders.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateOrderStatus_AsAdmin_ShouldUpdateStatus()
    {
        // Arrange
        var (_, productId) = await SetupUserAndProductAsync();

        // Create order as User
        await _client.PostAsJsonAsync($"/api/{ApiVersion}/cart/items",
            new AddToCartDto { ProductId = productId, Quantity = 1 });
        var address = new ShippingAddressDto
        {
            Name = "Test User",
            AddressLine1 = "123 Test St",
            City = "Test City",
            State = "TS",
            PostalCode = "12345",
            Country = "Test Country"
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/{ApiVersion}/orders", address);
        var order = await createResponse.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);

        // Login as Admin
        var (_, adminToken) = await _factory.CreateUserAndGetTokenAsync("Admin",
            "admin@test.com",
            RolUsuario.Admin);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            adminToken);

        // Act
        var updateDto = new UpdateOrderStatusDto { Status = OrderStatus.Shipped };
        var response = await _client.PutAsJsonAsync($"/api/{ApiVersion}/orders/{order!.Id}/status",
            updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await _client.GetAsync($"/api/{ApiVersion}/orders/{order.Id}");
        var updatedOrder = await getResponse.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
        updatedOrder!.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public async Task UpdateOrderStatus_AsUser_ShouldReturnForbidden()
    {
        // Arrange
        var (userToken, productId) = await SetupUserAndProductAsync();

        // Create order
        await _client.PostAsJsonAsync($"/api/{ApiVersion}/cart/items",
            new AddToCartDto { ProductId = productId, Quantity = 1 });
        // Act
        var address = new ShippingAddressDto
        {
            AddressLine1 = "123 Test St",
            City = "Test City",
            State = "TS",
            PostalCode = "12345",
            Country = "Test Country"
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/{ApiVersion}/orders", address);
        var order = await createResponse.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);

        // Act (Try to update as User)
        var updateDto = new UpdateOrderStatusDto { Status = OrderStatus.Shipped };
        var response = await _client.PutAsJsonAsync($"/api/{ApiVersion}/orders/{order!.Id}/status",
            updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
