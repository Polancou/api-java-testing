using AutoMapper;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Domain.Models;

namespace EcommerceCore.Application.Mappings;

public class CartProfile : Profile
{
    public CartProfile()
    {
        CreateMap<Cart, CartDto>();
        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.ProductName,
                opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Price,
                opt => opt.MapFrom(src => src.Product.Price))
            .ForMember(dest => dest.ImageUrl,
                opt => opt.MapFrom(src => src.Product.ImageUrl));
    }
}
