using EcommerceCore.Domain.Models;

namespace EcommerceCore.Application.DTOs;

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}
