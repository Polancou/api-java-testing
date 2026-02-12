using ApiBaseCore.Application.DTOs;
using AutoMapper;
using ApiBaseCore.Domain.Models;

namespace ApiBaseCore.Application.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<RegistroUsuarioDto, Usuario>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Addresses, opt => opt.Ignore());

        CreateMap<Usuario, PerfilUsuarioDto>()
            .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol.ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss")));

        CreateMap<Address, AddressDto>();
    }
}