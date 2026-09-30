using HerreraSystem.Application.Common;

namespace HerreraSystem.Application.DTOs.OrderDtos
{
    public class OrderQueryParams : PaginationParams
    {
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int? OrderStatusId { get; set; }

        public string? Search { get; set; }
    }
}
