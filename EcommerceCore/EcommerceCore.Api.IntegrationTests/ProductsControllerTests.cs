using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Domain.Models;
using FluentAssertions;

namespace EcommerceCore.Api.IntegrationTests;

public class ProductsControllerTests : IClassFixture<TestApiFactory>, IAsyncLifetime
{
    private readonly TestApiFactory _factory;
    private readonly HttpClient _client;
    private const string ApiVersion = "v1";

    public ProductsControllerTests(TestApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAllProducts_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync($"/api/{ApiVersion}/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<ProductDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProduct_WithAdminToken_ShouldReturnCreated()
    {
        // Arrange
        var (_, adminToken) = await _factory.CreateUserAndGetTokenAsync("Admin",
            "admin@test.com",
            RolUsuario.Admin);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            adminToken);

        var dto = new CreateProductDto
        {
            Name = "New Product",
            Description = "Desc",
            Price = 100,
            Stock = 10,
            ImageUrl = "url",
            Category = "Cat"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/{ApiVersion}/products",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product.Name.Should().Be("New Product");
    }

    [Fact]
    public async Task CreateProduct_WithUserToken_ShouldReturnForbidden()
    {
        // Arrange
        var (_, userToken) = await _factory.CreateUserAndGetTokenAsync("User",
            "user@test.com",
            RolUsuario.User);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            userToken);

        var dto = new CreateProductDto
        {
            Name = "New Product",
            Description = "Desc",
            Price = 100,
            Stock = 10,
            ImageUrl = "url",
            Category = "Cat"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/{ApiVersion}/products",
            dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateProduct_WithAdminToken_ShouldReturnNoContent()
    {
        // Arrange
        var (_, adminToken) = await _factory.CreateUserAndGetTokenAsync("Admin",
            "admin@test.com",
            RolUsuario.Admin);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            adminToken);

        // Create product first
        var createDto = new CreateProductDto
            { Name = "P1", Description = "D", Price = 10, Stock = 5, ImageUrl = "u", Category = "C" };
        var createResponse = await _client.PostAsJsonAsync($"/api/{ApiVersion}/products",
            createDto);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var updateDto = new UpdateProductDto
        {
            Name = "Updated P1",
            Description = "D",
            Price = 20,
            Stock = 5,
            ImageUrl = "u",
            Category = "C"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/{ApiVersion}/products/{createdProduct!.Id}",
            updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await _client.GetAsync($"/api/{ApiVersion}/products/{createdProduct.Id}");
        var updatedProduct = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        updatedProduct!.Name.Should().Be("Updated P1");
        updatedProduct.Price.Should().Be(20);
    }

    [Fact]
    public async Task DeleteProduct_WithAdminToken_ShouldReturnNoContent()
    {
        // Arrange
        var (_, adminToken) = await _factory.CreateUserAndGetTokenAsync("Admin",
            "admin@test.com",
            RolUsuario.Admin);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            adminToken);

        // Create product first
        var createDto = new CreateProductDto
            { Name = "P1", Description = "D", Price = 10, Stock = 5, ImageUrl = "u", Category = "C" };
        var createResponse = await _client.PostAsJsonAsync($"/api/{ApiVersion}/products",
            createDto);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/{ApiVersion}/products/{createdProduct!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify delete
        var getResponse = await _client.GetAsync($"/api/{ApiVersion}/products/{createdProduct.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
