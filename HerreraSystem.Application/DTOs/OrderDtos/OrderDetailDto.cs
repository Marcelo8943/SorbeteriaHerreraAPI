namespace HerreraSystem.Application.DTOs.OrderDtos
{
    public class OrderDetailDto
    {
        public int Id { get; set; }

        public string OrderCode { get; set; } = null!;

        public int CustomerId { get; set; }

        public string CustomerName { get; set; } = null!;

        public int OrderStatusId { get; set; }

        public string OrderStatusName { get; set; } = null!;

        public DateTime? RegistrationDate { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }

        public DateTime? ActualDeliveryDate { get; set; }

        public decimal TotalOrder { get; set; }

        public string CreatedByUserName { get; set; } = null!;

        public List<OrderDetailItemDto> Details { get; set; } = new();
    }
}
