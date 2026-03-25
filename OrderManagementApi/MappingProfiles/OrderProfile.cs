using AutoMapper;
using MessageContracts;
using OrderManagementApi.Features.Orders;
using OrderManagementApi.Models;

namespace OrderManagementApi.MappingProfiles
{
	public class OrderProfile : Profile
	{
		public OrderProfile()
		{
			CreateMap<Order, OrderDto>()
				.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

			CreateMap<OrderItem, OrderItemDto>();
		}
	}
}
