namespace EcommerceCore.Application.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Crea un nuevo intento de pago con la cantidad y moneda especificadas.
    /// </summary>
    /// <param name="amount">La cantidad total del pago.</param>
    /// <param name="currency">La moneda del pago (por defecto es "usd").</param>
    /// <returns>Un Task que representa la operación asíncrona, conteniendo el ID del intento de pago.</returns>
    Task<string> CreatePaymentIntentAsync(decimal amount, string currency = "usd");
    /// <summary>
    /// Obtiene el estado de un intento de pago existente.
    /// </summary>
    /// <param name="paymentIntentId">El ID del intento de pago a consultar.</param>
    /// <returns>Un Task que representa la operación asíncrona, conteniendo el estado del intento de pago.</returns>
    Task<string> GetPaymentIntentStatusAsync(string paymentIntentId);
}
