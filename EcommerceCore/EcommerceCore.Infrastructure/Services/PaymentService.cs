using EcommerceCore.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace EcommerceCore.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly string _secretKey;

    public PaymentService(IConfiguration configuration)
    {
        _secretKey = configuration[key: "Stripe:SecretKey"] ??
                     throw new InvalidOperationException(message: "Stripe SecretKey not found.");
        StripeConfiguration.ApiKey = _secretKey;
    }

    public async Task<string> CreatePaymentIntentAsync(decimal amount, string currency = "usd")
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // Stripe uses cents
            Currency = currency,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options: options);

        return paymentIntent.ClientSecret;
    }

    public async Task<string> GetPaymentIntentStatusAsync(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        var intent = await service.GetAsync(id: paymentIntentId);
        return intent.Status;
    }
}
