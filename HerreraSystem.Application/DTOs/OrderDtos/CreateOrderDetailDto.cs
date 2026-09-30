using System.ComponentModel.DataAnnotations;

namespace HerreraSystem.Application.DTOs.OrderDtos
{
    public class CreateOrderDetailDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int QuantityRequested { get; set; }

        [Required]
        public int ProductPriceId { get; set; }

        public int? BatchId { get; set; }
    }
}
