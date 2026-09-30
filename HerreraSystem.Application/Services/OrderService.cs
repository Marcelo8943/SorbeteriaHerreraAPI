using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.InventoryMovementDtos;
using HerreraSystem.Application.DTOs.OrderDtos;
using HerreraSystem.Application.Interfaces.Repositories;
using HerreraSystem.Application.Interfaces.Services;
using HerreraSystem.Domain.Entities;

namespace HerreraSystem.Application.Services
{
    public class OrderService : IOrderService
    {
        private const int StatusPendiente = 1;
        private const int StatusEnProceso = 2;
        private const int StatusEntregado = 3;
        private const int StatusCancelado = 4;

        private const int BodegaLocationId = 1;
        private const int MostradorLocationId = 2;
        private const int ReservadoLocationId = 3;

        private const int SaleTypeMayoreo = 2;
        private const int MovementTypeVenta = 2;
        private const int PaymentTypeContado = 1;

        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IBatchLocationRepository _batchLocationRepository;
        private readonly ISaleRepository _saleRepository;
        private readonly ISaleDetailRepository _saleDetailRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IInventoryMovementRepository _inventoryMovementRepository;
        private readonly IMovementDetailRepository _movementDetailRepository;
        private readonly IInventoryMovementService _inventoryMovementService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INicaraguaDateTimeService _dateTimeService;

        public OrderService(
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IBatchRepository batchRepository,
            IBatchLocationRepository batchLocationRepository,
            ISaleRepository saleRepository,
            ISaleDetailRepository saleDetailRepository,
            IPaymentRepository paymentRepository,
            IInventoryMovementRepository inventoryMovementRepository,
            IMovementDetailRepository movementDetailRepository,
            IInventoryMovementService inventoryMovementService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INicaraguaDateTimeService dateTimeService)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _batchRepository = batchRepository;
            _batchLocationRepository = batchLocationRepository;
            _saleRepository = saleRepository;
            _saleDetailRepository = saleDetailRepository;
            _paymentRepository = paymentRepository;
            _inventoryMovementRepository = inventoryMovementRepository;
            _movementDetailRepository = movementDetailRepository;
            _inventoryMovementService = inventoryMovementService;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _dateTimeService = dateTimeService;
        }

        public async Task<PagedResponse<OrderListItemDto>> GetAllAsync(
            OrderQueryParams queryParams)
            => await _orderRepository.GetAllAsync(queryParams);

        public async Task<OrderDetailDto?> GetByIdAsync(int id)
            => await _orderRepository.GetByIdAsync(id);

        public async Task<ServiceResult<OrderDetailDto>> CreateAsync(CreateOrderDto dto)
        {
            if (!dto.Details.Any())
                return ServiceResult<OrderDetailDto>.Fail("Debe incluir al menos un detalle");

            var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
            if (customer is null || customer.IsActive != true)
                return ServiceResult<OrderDetailDto>.Fail(
                    $"Cliente con Id {dto.CustomerId} no encontrado o inactivo");

            var now = _dateTimeService.Now;
            var priceIds = dto.Details.Select(d => d.ProductPriceId).Distinct().ToList();
            var prices = await _orderRepository.GetProductPricesByIdsAsync(priceIds);

            if (prices.Count != priceIds.Count)
                return ServiceResult<OrderDetailDto>.Fail("Uno o más precios no existen");

            var products = new Dictionary<int, DTOs.ProductDtos.ProductDto>();

            foreach (var detail in dto.Details)
            {
                var product = await _productRepository.GetByIdAsync(detail.ProductId);
                if (product is null || product.IsActive != true)
                    return ServiceResult<OrderDetailDto>.Fail(
                        $"Producto con Id {detail.ProductId} no encontrado o inactivo");

                products[detail.ProductId] = product;

                var price = prices.Single(p => p.Id == detail.ProductPriceId);
                var isCurrent = price.IsActive == true &&
                                price.ValidFrom <= now &&
                                (price.ValidTo is null || price.ValidTo >= now);
                var belongsToProduct = price.ProductId == detail.ProductId ||
                    (price.ProductId is null &&
                     price.LinePresentationId == product.LinePresentationId);

                if (!isCurrent || !belongsToProduct)
                    return ServiceResult<OrderDetailDto>.Fail(
                        $"El precio con Id {detail.ProductPriceId} no es válido para el producto Id {detail.ProductId}");
            }

            // El estado se resuelve por su nombre de catálogo, nunca por un Id fijo.
            var pendingStatus = await _orderRepository.GetByStatusNameAsync("Pendiente");
            int pendingStatusId = pendingStatus?.Id ?? StatusPendiente;

            var currentUserId = _currentUserService.CurrentUserId ?? dto.CreatedBy;
            var orderCount = await _orderRepository.CountByYearAsync(now.Year);
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                OrderStatusId = pendingStatusId,
                RegistrationDate = now,
                EstimatedDeliveryDate = dto.EstimatedDeliveryDate,
                TotalOrder = dto.Details.Sum(detail =>
                    detail.QuantityRequested * prices.Single(p => p.Id == detail.ProductPriceId).Price),
                CreatedBy = currentUserId,
                OrderCode = $"P-{now.Year}-{(orderCount + 1):D4}",
                OrderDetails = dto.Details.Select(detail => new OrderDetail
                {
                    ProductId = detail.ProductId,
                    QuantityRequested = detail.QuantityRequested,
                    ProductPriceId = detail.ProductPriceId,
                    BatchId = detail.BatchId
                }).ToList()
            };

            await _orderRepository.CreateAsync(order);

            var created = await _orderRepository.GetByIdAsync(order.Id);
            return ServiceResult<OrderDetailDto>.Ok(created!);
        }

        public async Task<ServiceResult<OrderDetailDto>> UpdateStatusAsync(int id, UpdateOrderStatusDto dto)
        {
            var order = await _orderRepository.GetEntityWithDetailsAsync(id);
            if (order is null)
                return ServiceResult<OrderDetailDto>.Fail($"Pedido con Id {id} no encontrado");

            var targetStatus = await _orderRepository.GetStatusByIdAsync(dto.OrderStatusId);
            if (targetStatus is null)
                return ServiceResult<OrderDetailDto>.Fail($"El estado con Id {dto.OrderStatusId} no es válido");

            if (order.OrderStatusId == dto.OrderStatusId)
                return ServiceResult<OrderDetailDto>.Fail(
                    $"El pedido ya se encuentra en estado '{targetStatus.OrderStatusName}'");

            if (order.OrderStatusId == StatusCancelado)
                return ServiceResult<OrderDetailDto>.Fail(
                    "No se puede cambiar el estado de un pedido cancelado");

            if (order.OrderStatusId == StatusEntregado)
                return ServiceResult<OrderDetailDto>.Fail(
                    "No se puede cambiar el estado de un pedido que ya ha sido entregado");

            var currentUserId = _currentUserService.CurrentUserId ?? order.CreatedBy;

            // Transiciones desde Pendiente
            if (order.OrderStatusId == StatusPendiente)
            {
                if (dto.OrderStatusId == StatusEntregado)
                    return ServiceResult<OrderDetailDto>.Fail(
                        "Un pedido en estado 'Pendiente' no puede pasar directamente a 'Entregado'. Debe prepararse primero ('En Proceso')");

                if (dto.OrderStatusId == StatusCancelado)
                {
                    order.OrderStatusId = StatusCancelado;
                    await _orderRepository.UpdateAsync(order);

                    var result = await _orderRepository.GetByIdAsync(order.Id);
                    return ServiceResult<OrderDetailDto>.Ok(result!);
                }

                if (dto.OrderStatusId == StatusEnProceso)
                {
                    return await ReserveStockAsync(order, currentUserId);
                }

                return ServiceResult<OrderDetailDto>.Fail(
                    $"Transición no permitida de 'Pendiente' a '{targetStatus.OrderStatusName}'");
            }

            // Transiciones desde En Proceso
            if (order.OrderStatusId == StatusEnProceso)
            {
                if (dto.OrderStatusId == StatusPendiente)
                    return ServiceResult<OrderDetailDto>.Fail(
                        "Un pedido 'En Proceso' no puede volver a estado 'Pendiente'");

                if (dto.OrderStatusId == StatusCancelado)
                {
                    return await CancelReservedOrderAsync(order, currentUserId);
                }

                if (dto.OrderStatusId == StatusEntregado)
                {
                    return await DeliverAndSellWholesaleOrderAsync(order, currentUserId);
                }

                return ServiceResult<OrderDetailDto>.Fail(
                    $"Transición no permitida de 'En Proceso' a '{targetStatus.OrderStatusName}'");
            }

            return ServiceResult<OrderDetailDto>.Fail("Transición de estado no soportada");
        }

        private async Task<ServiceResult<OrderDetailDto>> DeliverAndSellWholesaleOrderAsync(Order order, int currentUserId)
        {
            if (order.OrderDetails == null || !order.OrderDetails.Any())
                return ServiceResult<OrderDetailDto>.Fail("El pedido no contiene detalles");

            var now = _dateTimeService.Now;

            // 1. Obtener precios para validar
            var priceIds = order.OrderDetails.Select(d => d.ProductPriceId).Distinct().ToList();
            var prices = await _orderRepository.GetProductPricesByIdsAsync(priceIds);
            if (prices.Count != priceIds.Count)
                return ServiceResult<OrderDetailDto>.Fail("Uno o más precios de los detalles no existen");

            // 2. Validar que cada OrderDetail tenga BatchId y stock suficiente en Reservado (3)
            foreach (var detail in order.OrderDetails)
            {
                if (!detail.BatchId.HasValue)
                    return ServiceResult<OrderDetailDto>.Fail(
                        $"El detalle con producto Id {detail.ProductId} no tiene un lote asignado para la entrega");

                var batch = await _batchRepository.GetByIdAsync(detail.BatchId.Value);
                if (batch is null)
                    return ServiceResult<OrderDetailDto>.Fail(
                        $"El lote con Id {detail.BatchId.Value} no existe");

                var reservedLocation = await _batchLocationRepository
                    .GetByBatchAndLocationAsync(detail.BatchId.Value, ReservadoLocationId);

                var currentReserved = reservedLocation?.CurrentStock ?? 0;
                if (currentReserved < detail.QuantityRequested)
                {
                    return ServiceResult<OrderDetailDto>.Fail(
                        $"Stock insuficiente en Reservado para el lote '{batch.BatchCode ?? batch.Id.ToString()}'. " +
                        $"Disponible: {currentReserved}, Solicitado para entrega: {detail.QuantityRequested}");
                }
            }

            // 3. Calcular total de la venta
            decimal totalSale = order.OrderDetails.Sum(detail =>
                detail.QuantityRequested * prices.Single(p => p.Id == detail.ProductPriceId).Price);

            int saleCount = await _saleRepository.CountByYearAsync(now.Year);
            string saleCode = $"VTA-{now.Year}-{(saleCount + 1):D4}";

            // 4. Ejecutar transacción atómica con UnitOfWork
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Paso A: Crear Sale (SaleTypeId = 2: Mayoreo)
                var sale = await _saleRepository.CreateAsync(new Sale
                {
                    OrderId = order.Id,
                    CustomerId = order.CustomerId,
                    SaleDate = now,
                    TotalSale = totalSale,
                    PaymentStatus = "Pagado",
                    PendingBalance = 0,
                    CreatedBy = currentUserId,
                    PaymentTypeId = PaymentTypeContado,
                    SaleTypeId = SaleTypeMayoreo,
                    SaleCode = saleCode
                });

                // Paso B: Crear Payment
                await _paymentRepository.CreateAsync(new Payment
                {
                    SaleId = sale.Id,
                    PaymentMethodId = PaymentTypeContado,
                    AmountReceived = totalSale,
                    PaymentDate = now,
                    TransactionReference = $"Pago pedido {order.OrderCode}",
                    RegisteredBy = currentUserId
                });

                // Paso C: Crear InventoryMovement (tipo 2: Venta, egreso)
                var movement = await _inventoryMovementRepository.CreateAsync(new InventoryMovement
                {
                    MovementTypeId = MovementTypeVenta,
                    SaleId = sale.Id,
                    OrderId = order.Id,
                    MovementDate = now,
                    Notes = $"Salida por venta mayorista (Pedido {order.OrderCode})",
                    CreatedBy = currentUserId,
                    IsActive = true
                });

                // Paso D: Generar SaleDetail, MovementDetail y descontar BatchLocation por CADA OrderDetail
                foreach (var detail in order.OrderDetails)
                {
                    var price = prices.Single(p => p.Id == detail.ProductPriceId);
                    decimal unitPrice = price.Price;
                    decimal lineSubtotal = detail.QuantityRequested * unitPrice;

                    // SaleDetail (1 por cada OrderDetail, con su respectivo BatchId)
                    await _saleDetailRepository.CreateAsync(new SaleDetail
                    {
                        SaleId = sale.Id,
                        ProductId = detail.ProductId,
                        BatchId = detail.BatchId!.Value,
                        Quantity = detail.QuantityRequested,
                        AppliedPrice = unitPrice,
                        LineSubtotal = lineSubtotal
                    });

                    // Descontar stock de Reservado (LocationId = 3)
                    var reservedLocation = await _batchLocationRepository
                        .GetByBatchAndLocationAsync(detail.BatchId.Value, ReservadoLocationId);

                    reservedLocation!.CurrentStock -= detail.QuantityRequested;
                    await _batchLocationRepository.UpdateStockAsync(reservedLocation);

                    // MovementDetail (origen Reservado = 3, destino null: egreso del sistema)
                    var batch = await _batchRepository.GetByIdAsync(detail.BatchId.Value);
                    await _movementDetailRepository.CreateAsync(new MovementDetail
                    {
                        MovementId = movement.Id,
                        BatchId = detail.BatchId.Value,
                        SourceLocationId = ReservadoLocationId,
                        DestinationLocationId = null,
                        Quantity = detail.QuantityRequested,
                        UnitPrice = unitPrice,
                        UnitCost = batch?.UnitProductionCost ?? 0,
                        CreatedBy = currentUserId,
                        CreatedAt = now
                    });
                }

                // Paso E: Actualizar Order a Entregado y registrar fecha real de entrega
                order.OrderStatusId = StatusEntregado;
                order.ActualDeliveryDate = now;
                await _orderRepository.UpdateAsync(order);

                // Paso F: Commit atómico
                await _unitOfWork.CommitAsync();

                var updated = await _orderRepository.GetByIdAsync(order.Id);
                return ServiceResult<OrderDetailDto>.Ok(updated!);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ServiceResult<OrderDetailDto>.Fail($"Error al registrar la entrega y venta mayorista del pedido: {ex.Message}");
            }
        }

        private async Task<ServiceResult<OrderDetailDto>> CancelReservedOrderAsync(Order order, int currentUserId)
        {
            if (order.OrderDetails == null || !order.OrderDetails.Any())
                return ServiceResult<OrderDetailDto>.Fail("El pedido no contiene detalles");

            var transferDetails = new List<TransferDetailDto>();

            foreach (var detail in order.OrderDetails)
            {
                if (detail.QuantityRequested <= 0)
                    return ServiceResult<OrderDetailDto>.Fail(
                        $"La cantidad para el producto Id {detail.ProductId} debe ser mayor a 0");

                if (!detail.BatchId.HasValue)
                    return ServiceResult<OrderDetailDto>.Fail(
                        $"El detalle con producto Id {detail.ProductId} no tiene un lote asignado para revertir la reserva");

                var batch = await _batchRepository.GetByIdAsync(detail.BatchId.Value);
                if (batch is null)
                    return ServiceResult<OrderDetailDto>.Fail(
                        $"El lote con Id {detail.BatchId.Value} no existe");

                var reservedLocation = await _batchLocationRepository
                    .GetByBatchAndLocationAsync(detail.BatchId.Value, ReservadoLocationId);

                var currentReservedStock = reservedLocation?.CurrentStock ?? 0;
                if (currentReservedStock < detail.QuantityRequested)
                {
                    return ServiceResult<OrderDetailDto>.Fail(
                        $"Stock insuficiente en Reservado para el lote '{batch.BatchCode ?? batch.Id.ToString()}'. " +
                        $"Disponible: {currentReservedStock}, Solicitado para devolución: {detail.QuantityRequested}");
                }

                // Transferencia inversa: Reservado (3) -> Bodega (1)
                transferDetails.Add(new TransferDetailDto
                {
                    BatchId = detail.BatchId.Value,
                    SourceLocationId = ReservadoLocationId,
                    DestinationLocationId = BodegaLocationId,
                    Quantity = detail.QuantityRequested
                });
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var transferDto = new CreateTransferDto
                {
                    Notes = $"Cancelación de pedido {order.OrderCode}: retorno de stock reservado a Bodega",
                    CreatedBy = currentUserId,
                    OrderId = order.Id,
                    Details = transferDetails
                };

                var transferResult = await _inventoryMovementService.TransferAsync(transferDto);
                if (!transferResult.Success)
                {
                    await _unitOfWork.RollbackAsync();
                    return ServiceResult<OrderDetailDto>.Fail(transferResult.ErrorMessage!);
                }

                order.OrderStatusId = StatusCancelado;
                await _orderRepository.UpdateAsync(order);

                await _unitOfWork.CommitAsync();

                var updated = await _orderRepository.GetByIdAsync(order.Id);
                return ServiceResult<OrderDetailDto>.Ok(updated!);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ServiceResult<OrderDetailDto>.Fail($"Error al cancelar el pedido reservado: {ex.Message}");
            }
        }

        private async Task<ServiceResult<OrderDetailDto>> ReserveStockAsync(Order order, int currentUserId)
        {
            if (order.OrderDetails == null || !order.OrderDetails.Any())
                return ServiceResult<OrderDetailDto>.Fail("El pedido no contiene detalles");

            var transferDetails = new List<TransferDetailDto>();
            var originalDetails = order.OrderDetails.ToList();
            var detailsToAdd = new List<OrderDetail>();

            foreach (var detail in originalDetails)
            {
                if (detail.QuantityRequested <= 0)
                    return ServiceResult<OrderDetailDto>.Fail(
                        $"La cantidad solicitada para el producto Id {detail.ProductId} debe ser mayor a 0");

                var product = await _productRepository.GetByIdAsync(detail.ProductId);
                if (product is null || product.IsActive != true)
                    return ServiceResult<OrderDetailDto>.Fail(
                        $"Producto con Id {detail.ProductId} no encontrado o inactivo");

                if (detail.BatchId.HasValue)
                {
                    var batch = await _batchRepository.GetByIdAsync(detail.BatchId.Value);
                    if (batch is null)
                        return ServiceResult<OrderDetailDto>.Fail(
                            $"El lote con Id {detail.BatchId.Value} no existe");

                    var batchLocation = await _batchLocationRepository
                        .GetByBatchAndLocationAsync(detail.BatchId.Value, BodegaLocationId);

                    var currentStock = batchLocation?.CurrentStock ?? 0;
                    if (currentStock < detail.QuantityRequested)
                    {
                        return ServiceResult<OrderDetailDto>.Fail(
                            $"Stock insuficiente en Bodega para el lote '{batch.BatchCode ?? batch.Id.ToString()}'. " +
                            $"Disponible: {currentStock}, Solicitado: {detail.QuantityRequested}");
                    }

                    transferDetails.Add(new TransferDetailDto
                    {
                        BatchId = detail.BatchId.Value,
                        SourceLocationId = BodegaLocationId,
                        DestinationLocationId = ReservadoLocationId,
                        Quantity = detail.QuantityRequested
                    });
                }
                else
                {
                    var availableLots = await _batchLocationRepository
                        .GetAvailableStockFifoAsync(detail.ProductId, BodegaLocationId);

                    var totalAvailable = availableLots.Sum(l => l.CurrentStock);
                    if (totalAvailable < detail.QuantityRequested)
                    {
                        return ServiceResult<OrderDetailDto>.Fail(
                            $"Stock insuficiente en Bodega para el producto '{product.ProductName}'. " +
                            $"Disponible: {totalAvailable}, Solicitado: {detail.QuantityRequested}");
                    }

                    // Caso A: Existe un lote individual con stock suficiente
                    var singleSufficientLot = availableLots.FirstOrDefault(l => l.CurrentStock >= detail.QuantityRequested);
                    if (singleSufficientLot != null)
                    {
                        detail.BatchId = singleSufficientLot.BatchId;
                        transferDetails.Add(new TransferDetailDto
                        {
                            BatchId = singleSufficientLot.BatchId,
                            SourceLocationId = BodegaLocationId,
                            DestinationLocationId = ReservadoLocationId,
                            Quantity = detail.QuantityRequested
                        });
                    }
                    else
                    {
                        // Caso B: El pedido requiere unidades de múltiples lotes (FIFO split)
                        int pendingQty = detail.QuantityRequested;
                        bool isFirst = true;

                        foreach (var lot in availableLots)
                        {
                            if (pendingQty <= 0) break;
                            int takeQty = Math.Min(pendingQty, lot.CurrentStock);

                            if (isFirst)
                            {
                                detail.BatchId = lot.BatchId;
                                detail.QuantityRequested = takeQty;
                                isFirst = false;
                            }
                            else
                            {
                                var additionalDetail = new OrderDetail
                                {
                                    OrderId = order.Id,
                                    ProductId = detail.ProductId,
                                    ProductPriceId = detail.ProductPriceId,
                                    QuantityRequested = takeQty,
                                    BatchId = lot.BatchId
                                };
                                detailsToAdd.Add(additionalDetail);
                            }

                            transferDetails.Add(new TransferDetailDto
                            {
                                BatchId = lot.BatchId,
                                SourceLocationId = BodegaLocationId,
                                DestinationLocationId = ReservadoLocationId,
                                Quantity = takeQty
                            });

                            pendingQty -= takeQty;
                        }
                    }
                }
            }

            if (detailsToAdd.Any())
            {
                foreach (var addDetail in detailsToAdd)
                {
                    order.OrderDetails.Add(addDetail);
                }
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var transferDto = new CreateTransferDto
                {
                    Notes = $"Reserva de inventario para pedido {order.OrderCode}",
                    CreatedBy = currentUserId,
                    OrderId = order.Id,
                    Details = transferDetails
                };

                var transferResult = await _inventoryMovementService.TransferAsync(transferDto);
                if (!transferResult.Success)
                {
                    await _unitOfWork.RollbackAsync();
                    return ServiceResult<OrderDetailDto>.Fail(transferResult.ErrorMessage!);
                }

                order.OrderStatusId = StatusEnProceso;
                await _orderRepository.UpdateAsync(order);

                await _unitOfWork.CommitAsync();

                var updated = await _orderRepository.GetByIdAsync(order.Id);
                return ServiceResult<OrderDetailDto>.Ok(updated!);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ServiceResult<OrderDetailDto>.Fail($"Error al reservar el pedido: {ex.Message}");
            }
        }
    }
}
