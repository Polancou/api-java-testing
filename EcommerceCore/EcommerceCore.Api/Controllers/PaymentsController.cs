using Asp.Versioning;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCore.Api.Controllers;

[Authorize]
[ApiVersion(version: "1.0")]
public class PaymentsController(IPaymentService paymentService, ICartService cartService, IOrderService orderService)
    : BaseApiController
{
    /// <summary>
    /// Crea un Payment Intent para iniciar el proceso de pago.
    /// Requiere autenticación.
    /// </summary>
    /// <returns>Un objeto anónimo que contiene el client secret del Payment Intent.</returns>
    /// <response code="200">Retorna el client secret si el carrito no está vacío.</response>
    /// <response code="400">Si el carrito del usuario está vacío.</response>
    [HttpPost(template: "create-intent")]
    public async Task<IActionResult> CreatePaymentIntent()
    {
        // Obtener el carrito del usuario para calcular el total
        var cart = await cartService.GetCartAsync(userId: UserId);
        if (!cart.Items.Any())
        {
            // Si el carrito está vacío, no se puede crear un Payment Intent
            return BadRequest(error: new { message = "El carrito está vacío." });
        }

        // Crear el Payment Intent a través del servicio de pagos
        var clientSecret = await paymentService.CreatePaymentIntentAsync(amount: cart.TotalPrice);

        // Retornar el client secret para que el frontend pueda confirmar el pago
        return Ok(value: new { clientSecret });
    }

    /// <summary>
    /// Confirma un pago después de que el Payment Intent ha sido procesado por el frontend.
    /// Requiere autenticación.
    /// </summary>
    /// <param name="dto">Objeto que contiene el ID del Payment Intent y la dirección de envío.</param>
    /// <returns>Un objeto OrderDto si el pago es exitoso y la orden se crea.</returns>
    /// <response code="200">Retorna los detalles de la orden creada.</response>
    /// <response code="400">Si el pago no se ha completado exitosamente o hay un error al crear la orden.</response>
    [HttpPost(template: "confirm")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentDto dto)
    {
        // 1. Verificar el estado del pago con el proveedor de pagos (ej. Stripe)
        var status = await paymentService.GetPaymentIntentStatusAsync(paymentIntentId: dto.PaymentIntentId);

        if (status != "succeeded")
        {
            // Si el pago no fue exitoso, retornar un error
            return BadRequest(error: new { message = "El pago no se ha completado exitosamente." });
        }

        // 2. Crear la orden en la base de datos
        try
        {
            var order = await orderService.CreateOrderAsync(userId: UserId,
                shippingAddress: dto.ShippingAddress);
            // Retornar la orden creada
            return Ok(value: order);
        }
        catch (InvalidOperationException ex)
        {
            // Capturar errores específicos de lógica de negocio (ej. carrito vacío)
            return BadRequest(error: new { message = ex.Message });
        }
        catch (Exception)
        {
            // Capturar cualquier otro error inesperado durante la creación de la orden
            return BadRequest(error: new { message = "Ocurrió un error inesperado durante la confirmación del pago." });
        }
    }

    /// <summary>
    /// DTO para la confirmación de un pago.
    /// </summary>
    public class ConfirmPaymentDto
    {
        /// <summary>
        /// El ID del Payment Intent generado por el proveedor de pagos.
        /// </summary>
        public string PaymentIntentId { get; set; } = string.Empty;

        /// <summary>
        /// La dirección de envío para la orden.
        /// </summary>
        public ShippingAddressDto ShippingAddress { get; set; } = null!;
    }
}
