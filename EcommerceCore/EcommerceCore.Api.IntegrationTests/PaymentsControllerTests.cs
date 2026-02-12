using System.Net;
using System.Net.Http.Json;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Domain.Models;
using FluentAssertions;

namespace EcommerceCore.Api.IntegrationTests;

public class PaymentsControllerTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;
    private readonly TestApiFactory _factory;

    public PaymentsControllerTests(TestApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreatePaymentIntent_WithEmptyCart_ShouldReturnBadRequest()
    {
        // Arrange
        var (user, token) = await _factory.CreateUserAndGetTokenAsync("PaymentUser",
            "pay@test.com");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
            token);

        // Act
        var response = await _client.PostAsync("/api/v1/payments/create-intent",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Note: Testing success path requires mocking Stripe or having a valid cart. 
    // Since we use a real database in Integration Tests, we can add items to the cart.
    // However, calling Stripe API in tests might fail if keys are not set or if we don't want to hit real Stripe.
    // For this environment, we'll test the "Empty Cart" scenario which validates the endpoint is reachable and secured.
}
