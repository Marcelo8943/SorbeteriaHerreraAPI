using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.OrderDtos;
using HerreraSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerreraSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] OrderQueryParams queryParams)
        {
            var data = await _orderService.GetAllAsync(queryParams);

            return Ok(ApiResponse<PagedResponse<OrderListItemDto>>.Ok(data));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _orderService.GetByIdAsync(id);

            if (data is null)
                return NotFound(ApiResponse<OrderDetailDto>.Fail(
                    $"Pedido con Id {id} no encontrado"));

            return Ok(ApiResponse<OrderDetailDto>.Ok(data));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            var result = await _orderService.CreateAsync(dto);

            if (!result.Success)
                return BadRequest(ApiResponse<OrderDetailDto>.Fail(result.ErrorMessage!));

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data!.Id },
                ApiResponse<OrderDetailDto>.Ok(result.Data, "Pedido creado exitosamente"));
        }

        [HttpPatch("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var result = await _orderService.UpdateStatusAsync(id, dto);

            if (!result.Success)
                return BadRequest(ApiResponse<OrderDetailDto>.Fail(result.ErrorMessage!));

            return Ok(ApiResponse<OrderDetailDto>.Ok(result.Data!, "Estado del pedido actualizado exitosamente"));
        }
    }
}
