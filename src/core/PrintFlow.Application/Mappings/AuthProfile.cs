using AutoMapper;
using PrintFlow.Application.DTOs.Auth;
using PrintFlow.Domain.Entities;

namespace PrintFlow.Application.Mappings;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email ?? string.Empty));
    }
}