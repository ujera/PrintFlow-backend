using AutoMapper;
using PrintFlow.Application.DTOs.Orders;
using PrintFlow.Domain.Entities;

namespace PrintFlow.Application.Mappings;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        // ── Order ──
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.PaymentMethod, opt => opt.MapFrom(s =>
                s.PaymentMethod.HasValue ? s.PaymentMethod.Value.ToString() : null))
            .ForMember(d => d.PaymentStatus, opt => opt.MapFrom(s => s.PaymentStatus.ToString()));

        CreateMap<Order, OrderDetailDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.PaymentMethod, opt => opt.MapFrom(s =>
                s.PaymentMethod.HasValue ? s.PaymentMethod.Value.ToString() : null))
            .ForMember(d => d.PaymentStatus, opt => opt.MapFrom(s => s.PaymentStatus.ToString()))
            .ForMember(d => d.Customer, opt => opt.MapFrom(s => s.User))
            .ForMember(d => d.Payments, opt => opt.MapFrom(s => s.Payments))
            .ForMember(d => d.StatusHistory, opt => opt.MapFrom(s => s.StatusHistory));

        CreateMap<Order, OrderListDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s =>
                s.User != null ? s.User.Name : string.Empty))
            .ForMember(d => d.CustomerEmail, opt => opt.MapFrom(s =>
                s.User != null ? s.User.Email : string.Empty))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.PaymentStatus, opt => opt.MapFrom(s => s.PaymentStatus.ToString()))
            .ForMember(d => d.ItemCount, opt => opt.MapFrom(s => s.Items.Count));

        // ── Order Item ──
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s =>
                s.Product != null ? s.Product.Name : string.Empty));

        // ── Customer Info ──
        CreateMap<User, CustomerInfoDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email ?? string.Empty));

        // ── Payment ──
        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.Method, opt => opt.MapFrom(s => s.Method.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        // ── Status History ──
        CreateMap<OrderStatusHistory, StatusHistoryDto>()
            .ForMember(d => d.OldStatus, opt => opt.MapFrom(s => s.OldStatus.ToString()))
            .ForMember(d => d.NewStatus, opt => opt.MapFrom(s => s.NewStatus.ToString()))
            .ForMember(d => d.ChangedBy, opt => opt.MapFrom(s =>
                s.ChangedBy != null ? s.ChangedBy.Name : string.Empty));
    }
}