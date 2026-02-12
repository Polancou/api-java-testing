using AutoMapper;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Domain.Models;

namespace EcommerceCore.Application.Mappings;

public class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.NombreCompleto));
    }
}
