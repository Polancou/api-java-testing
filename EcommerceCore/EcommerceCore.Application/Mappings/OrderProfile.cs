using AutoMapper;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Domain.Models;

namespace EcommerceCore.Application.Mappings;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderDto>();
        CreateMap<OrderItem, OrderItemDto>();
    }
}
