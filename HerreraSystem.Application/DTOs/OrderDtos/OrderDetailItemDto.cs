namespace HerreraSystem.Application.DTOs.OrderDtos
{
    public class OrderDetailItemDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public int QuantityRequested { get; set; }

        public int ProductPriceId { get; set; }

        public decimal AppliedPrice { get; set; }

        public int? BatchId { get; set; }

        public string? BatchCode { get; set; }

        public decimal LineSubtotal { get; set; }
    }
}
