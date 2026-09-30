using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.OrderDtos;

namespace HerreraSystem.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<PagedResponse<OrderListItemDto>> GetAllAsync(OrderQueryParams queryParams);

        Task<OrderDetailDto?> GetByIdAsync(int id);

        Task<ServiceResult<OrderDetailDto>> CreateAsync(CreateOrderDto dto);

        Task<ServiceResult<OrderDetailDto>> UpdateStatusAsync(int id, UpdateOrderStatusDto dto);
    }
}
