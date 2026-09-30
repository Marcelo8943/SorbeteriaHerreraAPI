using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.OrderDtos;
using HerreraSystem.Domain.Entities;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateAsync(Order order);

        Task<int> CountByYearAsync(int year);

        Task<PagedResponse<OrderListItemDto>> GetAllAsync(OrderQueryParams queryParams);

        Task<OrderDetailDto?> GetByIdAsync(int id);

        Task<Order?> GetEntityWithDetailsAsync(int id);

        Task<OrderStatus?> GetByStatusNameAsync(string statusName);

        Task<OrderStatus?> GetStatusByIdAsync(int statusId);

        Task<IReadOnlyList<ProductPrice>> GetProductPricesByIdsAsync(
            IReadOnlyCollection<int> productPriceIds);

        Task UpdateAsync(Order order);
    }
}
