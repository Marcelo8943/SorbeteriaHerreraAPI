using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.OrderDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HerreraSystem.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly HerreraSystemContext _context;

        public OrderRepository(HerreraSystemContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<int> CountByYearAsync(int year)
        {
            return await _context.Orders
                .CountAsync(o => o.RegistrationDate.HasValue &&
                                 o.RegistrationDate.Value.Year == year);
        }

        public async Task<PagedResponse<OrderListItemDto>> GetAllAsync(
            OrderQueryParams queryParams)
        {
            var query = _context.Orders.AsNoTracking().AsQueryable();

            if (queryParams.FromDate.HasValue)
                query = query.Where(o => o.RegistrationDate >= queryParams.FromDate.Value.Date);

            if (queryParams.ToDate.HasValue)
            {
                var toDateExclusive = queryParams.ToDate.Value.Date.AddDays(1);
                query = query.Where(o => o.RegistrationDate < toDateExclusive);
            }

            if (queryParams.OrderStatusId.HasValue)
                query = query.Where(o => o.OrderStatusId == queryParams.OrderStatusId.Value);

            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var search = queryParams.Search.Trim();
                query = query.Where(o =>
                    o.OrderCode.Contains(search) ||
                    o.Customer.FirstName.Contains(search) ||
                    o.Customer.LastName.Contains(search));
            }

            return await query
                .OrderByDescending(o => o.RegistrationDate)
                .ThenByDescending(o => o.Id)
                .Select(o => new OrderListItemDto
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    CustomerName = (o.Customer.FirstName + " " + o.Customer.LastName).Trim(),
                    RegistrationDate = o.RegistrationDate,
                    EstimatedDeliveryDate = o.EstimatedDeliveryDate,
                    OrderStatusName = o.OrderStatus.OrderStatusName,
                    TotalOrder = o.TotalOrder ?? 0m
                })
                .ToPagedResponseAsync(queryParams);
        }

        public async Task<OrderDetailDto?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Select(o => new OrderDetailDto
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    CustomerId = o.CustomerId,
                    CustomerName = (o.Customer.FirstName + " " + o.Customer.LastName).Trim(),
                    OrderStatusId = o.OrderStatusId,
                    OrderStatusName = o.OrderStatus.OrderStatusName,
                    RegistrationDate = o.RegistrationDate,
                    EstimatedDeliveryDate = o.EstimatedDeliveryDate,
                    ActualDeliveryDate = o.ActualDeliveryDate,
                    TotalOrder = o.TotalOrder ?? 0m,
                    CreatedByUserName = o.CreatedByNavigation.UserName,
                    Details = o.OrderDetails
                        .OrderBy(d => d.Id)
                        .Select(d => new OrderDetailItemDto
                        {
                            Id = d.Id,
                            ProductId = d.ProductId,
                            ProductName = d.Product.ProductName,
                            QuantityRequested = d.QuantityRequested,
                            ProductPriceId = d.ProductPriceId,
                            AppliedPrice = d.ProductPrice.Price,
                            BatchId = d.BatchId,
                            BatchCode = d.Batch == null ? null : d.Batch.BatchCode,
                            LineSubtotal = d.QuantityRequested * d.ProductPrice.Price
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Order?> GetEntityWithDetailsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<OrderStatus?> GetByStatusNameAsync(string statusName)
        {
            return await _context.OrderStatuses
                .FirstOrDefaultAsync(s => s.OrderStatusName == statusName);
        }

        public async Task<OrderStatus?> GetStatusByIdAsync(int statusId)
        {
            return await _context.OrderStatuses
                .FirstOrDefaultAsync(s => s.Id == statusId);
        }

        public async Task<IReadOnlyList<ProductPrice>> GetProductPricesByIdsAsync(
            IReadOnlyCollection<int> productPriceIds)
        {
            return await _context.ProductPrices
                .Where(p => productPriceIds.Contains(p.Id))
                .ToListAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}
