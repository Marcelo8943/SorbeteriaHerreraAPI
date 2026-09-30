namespace HerreraSystem.Application.DTOs.OrderDtos
{
    public class OrderListItemDto
    {
        public int Id { get; set; }

        public string OrderCode { get; set; } = null!;

        public string CustomerName { get; set; } = null!;

        public DateTime? RegistrationDate { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }

        public string OrderStatusName { get; set; } = null!;

        public decimal TotalOrder { get; set; }
    }
}
