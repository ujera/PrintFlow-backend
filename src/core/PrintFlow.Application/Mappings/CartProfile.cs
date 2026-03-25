using AutoMapper;
using PrintFlow.Application.DTOs.Cart;
using PrintFlow.Domain.Entities;

namespace PrintFlow.Application.Mappings;

public class CartProfile : Profile
{
    public CartProfile()
    {
        CreateMap<CartItem, CartItemDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s =>
                s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.BasePrice, opt => opt.MapFrom(s =>
                s.Product != null ? s.Product.BasePrice : 0))
            .ForMember(d => d.Subtotal, opt => opt.Ignore()); // calculated in service

        CreateMap<AddCartItemRequest, CartItem>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.UserId, opt => opt.Ignore())
            .ForMember(d => d.User, opt => opt.Ignore())
            .ForMember(d => d.Product, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore());
    }
}