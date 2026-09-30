using System.ComponentModel.DataAnnotations;

namespace HerreraSystem.Application.DTOs.OrderDtos
{
    public class CreateOrderDto
    {
        [Required]
        public int CustomerId { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un detalle")]
        public List<CreateOrderDetailDto> Details { get; set; } = new();
    }
}
