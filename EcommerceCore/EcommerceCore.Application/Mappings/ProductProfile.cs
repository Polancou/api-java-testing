using AutoMapper;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Domain.Models;

namespace EcommerceCore.Application.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();
    }
}
