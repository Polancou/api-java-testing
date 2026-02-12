using AutoMapper;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Domain.Models;

namespace EcommerceCore.Application.Mappings;

public class ShippingProfile : Profile
{
    public ShippingProfile()
    {
        CreateMap<ShippingAddress, ShippingAddressDto>();
    }
}
