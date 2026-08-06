using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.SaleDtos;
using HerreraSystem.Domain.Entities;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface ISaleRepository
    {
        Task<Sale> CreateAsync(Sale sale);

        Task<int> CountByYearAsync(int year);

        Task<SalesStatsDto> GetStatsAsync();

        Task<PagedResponse<SaleListItemDto>> GetAllAsync(SaleQueryParams queryParams);

        Task<SaleHeaderDetailDto?> GetByIdAsync(int id);

        Task<IReadOnlyList<SaleDetailItemDto>> GetDetailsAsync(int id);

        Task<IReadOnlyList<SalePaymentDto>> GetPaymentsAsync(int id);
    }
}
