using System.ComponentModel.DataAnnotations;

namespace HerreraSystem.Application.DTOs.OrderDtos
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public int OrderStatusId { get; set; }
    }
}
