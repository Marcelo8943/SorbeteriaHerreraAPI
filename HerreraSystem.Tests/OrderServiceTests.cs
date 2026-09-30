using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.InventoryMovementDtos;
using HerreraSystem.Application.DTOs.OrderDtos;
using HerreraSystem.Application.Interfaces.Services;
using HerreraSystem.Application.Services;
using HerreraSystem.Domain.Entities;
using HerreraSystem.Infrastructure.Data;
using HerreraSystem.Infrastructure.Persistence;
using HerreraSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HerreraSystem.Tests
{
    [TestFixture]
    public class OrderServiceTests
    {
        private HerreraSystemContext _context = null!;
        private UnitOfWork _unitOfWork = null!;
        private OrderService _orderService = null!;
        private InventoryMovementService _inventoryMovementService = null!;
        private List<int> _createdOrderIds = new();
        private List<int> _createdMovementIds = new();
        private List<int> _createdBatchIds = new();
        private Batch _testBatch = null!;
        private BatchLocation _bodegaBatchLocation = null!;
        private int _initialBodegaStock = 50;

        private class FakeCurrentUserService : ICurrentUserService
        {
            public int? CurrentUserId => 1;
            public string? CurrentUsername => "Admin";
            public string? CurrentRole => "Admin";
            public bool IsAuthenticated => true;
        }

        [SetUp]
        public async Task SetUp()
        {
            var connectionString = "Server=.;" +
                                   "Database=HerreraSystem;" +
                                   "Trusted_Connection=True;" +
                                   "TrustServerCertificate=True;";

            var options = new DbContextOptionsBuilder<HerreraSystemContext>()
                .UseSqlServer(connectionString)
                .Options;

            _context = new HerreraSystemContext(options);
            _unitOfWork = new UnitOfWork(_context);

            var dateTimeService = new NicaraguaDateTimeService();
            var orderRepo = new OrderRepository(_context);
            var customerRepo = new CustomerRepository(_context);
            var productRepo = new ProductRepository(_context, dateTimeService);
            var batchRepo = new BatchRepository(_context);
            var batchLocationRepo = new BatchLocationRepository(_context);
            var saleRepo = new SaleRepository(_context, dateTimeService);
            var saleDetailRepo = new SaleDetailRepository(_context);
            var paymentRepo = new PaymentRepository(_context);
            var movementRepo = new InventoryMovementRepository(_context, dateTimeService);
            var movementDetailRepo = new MovementDetailRepository(_context);
            var currentUserService = new FakeCurrentUserService();

            _inventoryMovementService = new InventoryMovementService(
                _unitOfWork,
                batchRepo,
                batchLocationRepo,
                movementRepo,
                movementDetailRepo,
                dateTimeService);

            _orderService = new OrderService(
                orderRepo,
                customerRepo,
                productRepo,
                batchRepo,
                batchLocationRepo,
                saleRepo,
                saleDetailRepo,
                paymentRepo,
                movementRepo,
                movementDetailRepo,
                _inventoryMovementService,
                _unitOfWork,
                currentUserService,
                dateTimeService);

            // Obtener o preparar lote activo para el Producto 1 en Bodega (LocationId = 1)
            var batch = await _context.Batches
                .FirstOrDefaultAsync(b => b.ProductId == 1 && b.BatchStatusId == 1);

            if (batch == null)
            {
                Assert.Inconclusive("No se encontró lote activo para Producto 1 en la base de datos.");
                return;
            }

            _testBatch = batch;

            var blBodega = await _context.BatchLocations
                .FirstOrDefaultAsync(l => l.BatchId == batch.Id && l.LocationId == 1);

            if (blBodega == null)
            {
                blBodega = new BatchLocation
                {
                    BatchId = batch.Id,
                    LocationId = 1,
                    CurrentStock = _initialBodegaStock
                };
                _context.BatchLocations.Add(blBodega);
                await _context.SaveChangesAsync();
            }
            else
            {
                blBodega.CurrentStock = _initialBodegaStock;
                await _context.SaveChangesAsync();
            }

            _bodegaBatchLocation = blBodega;

            // Asegurar que la ubicación Reservado (LocationId = 3) para este lote exista y esté en 0
            var blReservado = await _context.BatchLocations
                .FirstOrDefaultAsync(l => l.BatchId == batch.Id && l.LocationId == 3);

            if (blReservado == null)
            {
                blReservado = new BatchLocation
                {
                    BatchId = batch.Id,
                    LocationId = 3,
                    CurrentStock = 0
                };
                _context.BatchLocations.Add(blReservado);
                await _context.SaveChangesAsync();
            }
            else
            {
                blReservado.CurrentStock = 0;
                await _context.SaveChangesAsync();
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_createdOrderIds.Any())
            {
                var movs = await _context.InventoryMovements
                    .Where(m => m.OrderId.HasValue && _createdOrderIds.Contains(m.OrderId.Value))
                    .ToListAsync();

                foreach (var mov in movs)
                {
                    var details = await _context.MovementDetails.Where(md => md.MovementId == mov.Id).ToListAsync();
                    _context.MovementDetails.RemoveRange(details);
                    _context.InventoryMovements.Remove(mov);
                }

                var sales = await _context.Sales
                    .Where(s => s.OrderId.HasValue && _createdOrderIds.Contains(s.OrderId.Value))
                    .ToListAsync();

                foreach (var sale in sales)
                {
                    var payments = await _context.Payments.Where(p => p.SaleId == sale.Id).ToListAsync();
                    _context.Payments.RemoveRange(payments);

                    var saleDetails = await _context.SaleDetails.Where(sd => sd.SaleId == sale.Id).ToListAsync();
                    _context.SaleDetails.RemoveRange(saleDetails);

                    _context.Sales.Remove(sale);
                }

                foreach (var orderId in _createdOrderIds)
                {
                    var orderDetails = await _context.OrderDetails.Where(od => od.OrderId == orderId).ToListAsync();
                    _context.OrderDetails.RemoveRange(orderDetails);

                    var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
                    if (order != null) _context.Orders.Remove(order);
                }

                await _context.SaveChangesAsync();
                _createdOrderIds.Clear();
            }

            if (_createdBatchIds.Any())
            {
                var locs = await _context.BatchLocations.Where(bl => _createdBatchIds.Contains(bl.BatchId)).ToListAsync();
                _context.BatchLocations.RemoveRange(locs);

                var batches = await _context.Batches.Where(b => _createdBatchIds.Contains(b.Id)).ToListAsync();
                _context.Batches.RemoveRange(batches);

                await _context.SaveChangesAsync();
                _createdBatchIds.Clear();
            }

            // Restaurar stock en Bodega y Reservado
            if (_testBatch != null)
            {
                var blBodega = await _context.BatchLocations
                    .FirstOrDefaultAsync(l => l.BatchId == _testBatch.Id && l.LocationId == 1);
                if (blBodega != null)
                {
                    blBodega.CurrentStock = _initialBodegaStock;
                }

                var blReservado = await _context.BatchLocations
                    .FirstOrDefaultAsync(l => l.BatchId == _testBatch.Id && l.LocationId == 3);
                if (blReservado != null)
                {
                    blReservado.CurrentStock = 0;
                }

                await _context.SaveChangesAsync();
            }

            await _context.DisposeAsync();
        }

        [Test]
        public async Task CreateOrder_DebeCrearPedidoEnEstadoPendienteSinModificarInventario()
        {
            var stockBodegaAntes = _bodegaBatchLocation.CurrentStock;

            var createDto = new CreateOrderDto
            {
                CustomerId = 1,
                CreatedBy = 1,
                EstimatedDeliveryDate = DateTime.Now.AddDays(3),
                Details = new List<CreateOrderDetailDto>
                {
                    new CreateOrderDetailDto
                    {
                        ProductId = 1,
                        ProductPriceId = 3, // Precio mayoreo para LinePresentationId 1
                        QuantityRequested = 5,
                        BatchId = null // Sin lote preasignado (formulario frontend)
                    }
                }
            };

            var result = await _orderService.CreateAsync(createDto);

            Assert.That(result.Success, Is.True, result.ErrorMessage);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.GreaterThan(0));
            Assert.That(result.Data.OrderStatusName, Is.EqualTo("Pendiente"));

            _createdOrderIds.Add(result.Data.Id);

            // Verificar que no se creó ningún movimiento de inventario
            var movements = await _context.InventoryMovements
                .Where(m => m.OrderId == result.Data.Id)
                .ToListAsync();

            Assert.That(movements, Is.Empty);

            // Verificar que el stock en Bodega no se modificó
            var blBodegaDespues = await _context.BatchLocations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == _bodegaBatchLocation.Id);

            Assert.That(blBodegaDespues!.CurrentStock, Is.EqualTo(stockBodegaAntes));
        }

        [Test]
        public async Task Pendiente_A_EnProceso_DebeTransferirStockDeBodegaAReservadoYAsociarOrderId()
        {
            int cantidadReserva = 10;

            var createDto = new CreateOrderDto
            {
                CustomerId = 1,
                CreatedBy = 1,
                EstimatedDeliveryDate = DateTime.Now.AddDays(3),
                Details = new List<CreateOrderDetailDto>
                {
                    new CreateOrderDetailDto
                    {
                        ProductId = 1,
                        ProductPriceId = 3,
                        QuantityRequested = cantidadReserva,
                        BatchId = _testBatch.Id
                    }
                }
            };

            var orderResult = await _orderService.CreateAsync(createDto);
            Assert.That(orderResult.Success, Is.True, orderResult.ErrorMessage);
            _createdOrderIds.Add(orderResult.Data!.Id);

            // Transición a En Proceso (Reserva)
            var updateStatusDto = new UpdateOrderStatusDto
            {
                OrderStatusId = 2 // En Proceso
            };

            var updateResult = await _orderService.UpdateStatusAsync(orderResult.Data.Id, updateStatusDto);

            Assert.That(updateResult.Success, Is.True, updateResult.ErrorMessage);
            Assert.That(updateResult.Data!.OrderStatusName, Is.EqualTo("En Proceso"));

            // Verificar que se creó el InventoryMovement tipo Transferencia (1002) ligado al OrderId
            var movement = await _context.InventoryMovements
                .Include(m => m.MovementDetails)
                .FirstOrDefaultAsync(m => m.OrderId == orderResult.Data.Id);

            Assert.That(movement, Is.Not.Null);
            Assert.That(movement!.MovementTypeId, Is.EqualTo(1002)); // Transferencia
            Assert.That(movement.OrderId, Is.EqualTo(orderResult.Data.Id));
            Assert.That(movement.MovementDetails.Count, Is.EqualTo(1));

            var detail = movement.MovementDetails.First();
            Assert.That(detail.SourceLocationId, Is.EqualTo(1)); // Bodega
            Assert.That(detail.DestinationLocationId, Is.EqualTo(3)); // Reservado
            Assert.That(detail.Quantity, Is.EqualTo(cantidadReserva));
            Assert.That(detail.BatchId, Is.EqualTo(_testBatch.Id));

            // Verificar actualización de stocks en BatchLocations
            var blBodega = await _context.BatchLocations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.BatchId == _testBatch.Id && l.LocationId == 1);

            var blReservado = await _context.BatchLocations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.BatchId == _testBatch.Id && l.LocationId == 3);

            Assert.That(blBodega!.CurrentStock, Is.EqualTo(_initialBodegaStock - cantidadReserva));
            Assert.That(blReservado!.CurrentStock, Is.EqualTo(cantidadReserva));
        }

        [Test]
        public async Task Reserva_StockInsuficiente_DebeHacerRollbackCompletoYSinCambios()
        {
            int stockDisponible = _bodegaBatchLocation.CurrentStock;
            int cantidadExcesiva = stockDisponible + 100;

            var createDto = new CreateOrderDto
            {
                CustomerId = 1,
                CreatedBy = 1,
                EstimatedDeliveryDate = DateTime.Now.AddDays(3),
                Details = new List<CreateOrderDetailDto>
                {
                    new CreateOrderDetailDto
                    {
                        ProductId = 1,
                        ProductPriceId = 3,
                        QuantityRequested = cantidadExcesiva,
                        BatchId = _testBatch.Id
                    }
                }
            };

            var orderResult = await _orderService.CreateAsync(createDto);
            Assert.That(orderResult.Success, Is.True, orderResult.ErrorMessage);
            _createdOrderIds.Add(orderResult.Data!.Id);

            // Intentar reservar stock superior al disponible
            var updateStatusDto = new UpdateOrderStatusDto
            {
                OrderStatusId = 2 // En Proceso
            };

            var updateResult = await _orderService.UpdateStatusAsync(orderResult.Data.Id, updateStatusDto);

            // Debe fallar
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.ErrorMessage, Does.Contain("Stock insuficiente").IgnoreCase);

            // Verificar que la orden sigue en estado Pendiente (1)
            var orderInDb = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderResult.Data.Id);

            Assert.That(orderInDb!.OrderStatusId, Is.EqualTo(1)); // Pendiente

            // Verificar que no se creó ningún movimiento
            var movements = await _context.InventoryMovements
                .Where(m => m.OrderId == orderResult.Data.Id)
                .ToListAsync();

            Assert.That(movements, Is.Empty);

            // Verificar que los stocks en Bodega y Reservado no cambiaron
            var blBodega = await _context.BatchLocations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.BatchId == _testBatch.Id && l.LocationId == 1);

            var blReservado = await _context.BatchLocations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.BatchId == _testBatch.Id && l.LocationId == 3);

            Assert.That(blBodega!.CurrentStock, Is.EqualTo(stockDisponible));
            Assert.That(blReservado!.CurrentStock, Is.EqualTo(0));
        }

        [Test]
        public async Task EnProceso_A_Cancelado_DebeTransferirStockInversoDeReservadoABodega()
        {
            int cantidad = 15;

            var createDto = new CreateOrderDto
            {
                CustomerId = 1,
                CreatedBy = 1,
                EstimatedDeliveryDate = DateTime.Now.AddDays(3),
                Details = new List<CreateOrderDetailDto>
                {
                    new CreateOrderDetailDto
                    {
                        ProductId = 1,
                        ProductPriceId = 3,
                        QuantityRequested = cantidad,
                        BatchId = _testBatch.Id
                    }
                }
            };

            var orderResult = await _orderService.CreateAsync(createDto);
            _createdOrderIds.Add(orderResult.Data!.Id);

            // 1. Reservar (Pendiente -> En Proceso)
            var resResult = await _orderService.UpdateStatusAsync(orderResult.Data.Id, new UpdateOrderStatusDto { OrderStatusId = 2 });
            Assert.That(resResult.Success, Is.True, resResult.ErrorMessage);

            // 2. Cancelar (En Proceso -> Cancelado)
            var cancelResult = await _orderService.UpdateStatusAsync(orderResult.Data.Id, new UpdateOrderStatusDto { OrderStatusId = 4 });

            Assert.That(cancelResult.Success, Is.True, cancelResult.ErrorMessage);
            Assert.That(cancelResult.Data!.OrderStatusName, Is.EqualTo("Cancelado"));

            // Verificar movimientos: deben existir 2 (Reserva y Retorno por Cancelación)
            var movements = await _context.InventoryMovements
                .Include(m => m.MovementDetails)
                .Where(m => m.OrderId == orderResult.Data.Id)
                .OrderBy(m => m.Id)
                .ToListAsync();

            Assert.That(movements.Count, Is.EqualTo(2));

            // Movimiento 1: Bodega (1) -> Reservado (3)
            var movReserva = movements[0];
            Assert.That(movReserva.MovementDetails.First().SourceLocationId, Is.EqualTo(1));
            Assert.That(movReserva.MovementDetails.First().DestinationLocationId, Is.EqualTo(3));

            // Movimiento 2: Reservado (3) -> Bodega (1)
            var movCancel = movements[1];
            Assert.That(movCancel.MovementDetails.First().SourceLocationId, Is.EqualTo(3));
            Assert.That(movCancel.MovementDetails.First().DestinationLocationId, Is.EqualTo(1));
            Assert.That(movCancel.MovementDetails.First().Quantity, Is.EqualTo(cantidad));

            // Verificar que el stock en Bodega retornó al valor original y Reservado quedó en 0
            var blBodega = await _context.BatchLocations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.BatchId == _testBatch.Id && l.LocationId == 1);

            var blReservado = await _context.BatchLocations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.BatchId == _testBatch.Id && l.LocationId == 3);

            Assert.That(blBodega!.CurrentStock, Is.EqualTo(_initialBodegaStock));
            Assert.That(blReservado!.CurrentStock, Is.EqualTo(0));
        }

        [Test]
        public async Task TransicionesInvalidas_DebenSerRechazadasCorrectamente()
        {
            var createDto = new CreateOrderDto
            {
                CustomerId = 1,
                CreatedBy = 1,
                EstimatedDeliveryDate = DateTime.Now.AddDays(3),
                Details = new List<CreateOrderDetailDto>
                {
                    new CreateOrderDetailDto
                    {
                        ProductId = 1,
                        ProductPriceId = 3,
                        QuantityRequested = 2,
                        BatchId = _testBatch.Id
                    }
                }
            };

            var orderResult = await _orderService.CreateAsync(createDto);
            _createdOrderIds.Add(orderResult.Data!.Id);

            // 1. Intento de salto directo: Pendiente (1) -> Entregado (3)
            var saltoEntregado = await _orderService.UpdateStatusAsync(orderResult.Data.Id, new UpdateOrderStatusDto { OrderStatusId = 3 });
            Assert.That(saltoEntregado.Success, Is.False);
            Assert.That(saltoEntregado.ErrorMessage, Does.Contain("no puede pasar directamente a 'Entregado'").IgnoreCase);

            // 2. Intento de mismo estado: Pendiente (1) -> Pendiente (1)
            var mismoEstado = await _orderService.UpdateStatusAsync(orderResult.Data.Id, new UpdateOrderStatusDto { OrderStatusId = 1 });
            Assert.That(mismoEstado.Success, Is.False);
            Assert.That(mismoEstado.ErrorMessage, Does.Contain("ya se encuentra en estado").IgnoreCase);

            // 3. Cancelar pedido desde Pendiente (válido)
            var cancelacion = await _orderService.UpdateStatusAsync(orderResult.Data.Id, new UpdateOrderStatusDto { OrderStatusId = 4 });
            Assert.That(cancelacion.Success, Is.True);

            // 4. Intento de modificar un pedido ya cancelado: Cancelado (4) -> En Proceso (2)
            var reactivar = await _orderService.UpdateStatusAsync(orderResult.Data.Id, new UpdateOrderStatusDto { OrderStatusId = 2 });
            Assert.That(reactivar.Success, Is.False);
            Assert.That(reactivar.ErrorMessage, Does.Contain("No se puede cambiar el estado de un pedido cancelado").IgnoreCase);
        }

        [Test]
        public async Task Pendiente_A_EnProceso_ConFifoSplitEntreMultiplesLotes_DebeGenerarMultiplesDetallesYTransferirCorrectamente()
        {
            // 1. Configurar lote 1 (_testBatch) con 6 unidades en Bodega
            _bodegaBatchLocation.CurrentStock = 6;
            await _context.SaveChangesAsync();

            // 2. Crear lote 2 temporal en Bodega con 8 unidades
            var batch2 = new Batch
            {
                RestockId = 7,
                ProductId = 1,
                BatchStatusId = 1, // Activo
                InitialQuantity = 20,
                UnitProductionCost = 35.00m,
                ExpirationDate = new DateOnly(2026, 12, 31),
                BatchCode = "TEST-FIFO-SPLIT-L2"
            };
            _context.Batches.Add(batch2);
            await _context.SaveChangesAsync();
            _createdBatchIds.Add(batch2.Id);

            var bl2Bodega = new BatchLocation
            {
                BatchId = batch2.Id,
                LocationId = 1, // Bodega
                CurrentStock = 8
            };
            _context.BatchLocations.Add(bl2Bodega);

            var bl2Reservado = new BatchLocation
            {
                BatchId = batch2.Id,
                LocationId = 3, // Reservado
                CurrentStock = 0
            };
            _context.BatchLocations.Add(bl2Reservado);
            await _context.SaveChangesAsync();

            // Total disponible en Bodega: 6 (Lote 1) + 8 (Lote 2) = 14 unidades
            // Solicitamos 10 unidades sin preasignar BatchId (dispara FIFO)
            int cantidadSolicitada = 10;

            var createDto = new CreateOrderDto
            {
                CustomerId = 1,
                CreatedBy = 1,
                EstimatedDeliveryDate = DateTime.Now.AddDays(3),
                Details = new List<CreateOrderDetailDto>
                {
                    new CreateOrderDetailDto
                    {
                        ProductId = 1,
                        ProductPriceId = 3,
                        QuantityRequested = cantidadSolicitada,
                        BatchId = null // Sin lote fijo: split FIFO
                    }
                }
            };

            var orderResult = await _orderService.CreateAsync(createDto);
            Assert.That(orderResult.Success, Is.True, orderResult.ErrorMessage);
            var orderId = orderResult.Data!.Id;
            _createdOrderIds.Add(orderId);

            // Transición: Pendiente -> En Proceso (Reserva con split)
            var updateResult = await _orderService.UpdateStatusAsync(orderId, new UpdateOrderStatusDto { OrderStatusId = 2 });
            Assert.That(updateResult.Success, Is.True, updateResult.ErrorMessage);
            Assert.That(updateResult.Data!.OrderStatusName, Is.EqualTo("En Proceso"));

            // 1. Verificar OrderDetails generados: deben ser exactamente 2 líneas
            var orderInDb = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            Assert.That(orderInDb, Is.Not.Null);
            Assert.That(orderInDb!.OrderDetails.Count, Is.EqualTo(2), "Debe haber generado 2 líneas de OrderDetail por el split FIFO");

            var d1 = orderInDb.OrderDetails.FirstOrDefault(d => d.BatchId == _testBatch.Id);
            var d2 = orderInDb.OrderDetails.FirstOrDefault(d => d.BatchId == batch2.Id);

            Assert.That(d1, Is.Not.Null, "Debe existir detalle asociado al Lote 1");
            Assert.That(d1!.QuantityRequested, Is.EqualTo(6), "El Lote 1 debe tomar sus 6 unidades completas disponibles");

            Assert.That(d2, Is.Not.Null, "Debe existir detalle asociado al Lote 2");
            Assert.That(d2!.QuantityRequested, Is.EqualTo(4), "El Lote 2 debe tomar las 4 unidades restantes");

            // 2. Verificar que la suma es exactamente igual a la cantidad original solicitada
            var sumaCantidades = orderInDb.OrderDetails.Sum(d => d.QuantityRequested);
            Assert.That(sumaCantidades, Is.EqualTo(cantidadSolicitada), "La suma de cantidades de los OrderDetails debe coincidir exactamente con la solicitada");

            // 3. Verificar MovementDetails creados: deben existir 2 detalles en el movimiento de transferencia
            var movReserva = await _context.InventoryMovements
                .Include(m => m.MovementDetails)
                .FirstOrDefaultAsync(m => m.OrderId == orderId && m.MovementTypeId == 1002);

            Assert.That(movReserva, Is.Not.Null);
            Assert.That(movReserva!.MovementDetails.Count, Is.EqualTo(2), "El movimiento de reserva debe contener detalles para ambos lotes");

            var md1 = movReserva.MovementDetails.FirstOrDefault(md => md.BatchId == _testBatch.Id);
            var md2 = movReserva.MovementDetails.FirstOrDefault(md => md.BatchId == batch2.Id);

            Assert.That(md1, Is.Not.Null);
            Assert.That(md1!.SourceLocationId, Is.EqualTo(1)); // Bodega
            Assert.That(md1.DestinationLocationId, Is.EqualTo(3)); // Reservado
            Assert.That(md1.Quantity, Is.EqualTo(6));

            Assert.That(md2, Is.Not.Null);
            Assert.That(md2!.SourceLocationId, Is.EqualTo(1)); // Bodega
            Assert.That(md2.DestinationLocationId, Is.EqualTo(3)); // Reservado
            Assert.That(md2.Quantity, Is.EqualTo(4));

            // 4. Verificar actualización de stock en Bodega y Reservado para ambos lotes
            var stockL1Bodega = await _context.BatchLocations.AsNoTracking().FirstOrDefaultAsync(bl => bl.BatchId == _testBatch.Id && bl.LocationId == 1);
            var stockL2Bodega = await _context.BatchLocations.AsNoTracking().FirstOrDefaultAsync(bl => bl.BatchId == batch2.Id && bl.LocationId == 1);
            var stockL1Reservado = await _context.BatchLocations.AsNoTracking().FirstOrDefaultAsync(bl => bl.BatchId == _testBatch.Id && bl.LocationId == 3);
            var stockL2Reservado = await _context.BatchLocations.AsNoTracking().FirstOrDefaultAsync(bl => bl.BatchId == batch2.Id && bl.LocationId == 3);

            Assert.That(stockL1Bodega!.CurrentStock, Is.EqualTo(0)); // 6 - 6
            Assert.That(stockL2Bodega!.CurrentStock, Is.EqualTo(4)); // 8 - 4
            Assert.That(stockL1Reservado!.CurrentStock, Is.EqualTo(6));
            Assert.That(stockL2Reservado!.CurrentStock, Is.EqualTo(4));
            Assert.That(stockL1Reservado.CurrentStock + stockL2Reservado.CurrentStock, Is.EqualTo(10));

            // 5. Verificar Cancelación inversa de pedido con split (En Proceso -> Cancelado)
            var cancelResult = await _orderService.UpdateStatusAsync(orderId, new UpdateOrderStatusDto { OrderStatusId = 4 });
            Assert.That(cancelResult.Success, Is.True, cancelResult.ErrorMessage);
            Assert.That(cancelResult.Data!.OrderStatusName, Is.EqualTo("Cancelado"));

            var movCancel = await _context.InventoryMovements
                .Include(m => m.MovementDetails)
                .Where(m => m.OrderId == orderId && m.MovementTypeId == 1002)
                .OrderByDescending(m => m.Id)
                .FirstOrDefaultAsync();

            Assert.That(movCancel, Is.Not.Null);
            Assert.That(movCancel!.MovementDetails.Count, Is.EqualTo(2), "El movimiento de cancelación debe devolver todos los lotes usados");

            var mdCancel1 = movCancel.MovementDetails.FirstOrDefault(md => md.BatchId == _testBatch.Id);
            var mdCancel2 = movCancel.MovementDetails.FirstOrDefault(md => md.BatchId == batch2.Id);

            Assert.That(mdCancel1!.SourceLocationId, Is.EqualTo(3)); // Reservado
            Assert.That(mdCancel1.DestinationLocationId, Is.EqualTo(1)); // Bodega
            Assert.That(mdCancel1.Quantity, Is.EqualTo(6));

            Assert.That(mdCancel2!.SourceLocationId, Is.EqualTo(3)); // Reservado
            Assert.That(mdCancel2.DestinationLocationId, Is.EqualTo(1)); // Bodega
            Assert.That(mdCancel2.Quantity, Is.EqualTo(4));

            // Verificar restauración completa de stocks
            var stockL1BodegaFinal = await _context.BatchLocations.AsNoTracking().FirstOrDefaultAsync(bl => bl.BatchId == _testBatch.Id && bl.LocationId == 1);
            var stockL2BodegaFinal = await _context.BatchLocations.AsNoTracking().FirstOrDefaultAsync(bl => bl.BatchId == batch2.Id && bl.LocationId == 1);
            var stockL1ResFinal = await _context.BatchLocations.AsNoTracking().FirstOrDefaultAsync(bl => bl.BatchId == _testBatch.Id && bl.LocationId == 3);
            var stockL2ResFinal = await _context.BatchLocations.AsNoTracking().FirstOrDefaultAsync(bl => bl.BatchId == batch2.Id && bl.LocationId == 3);

            Assert.That(stockL1BodegaFinal!.CurrentStock, Is.EqualTo(6));
            Assert.That(stockL2BodegaFinal!.CurrentStock, Is.EqualTo(8));
            Assert.That(stockL1ResFinal!.CurrentStock, Is.EqualTo(0));
            Assert.That(stockL2ResFinal!.CurrentStock, Is.EqualTo(0));
        }

        [Test]
        public async Task EnProceso_A_Entregado_ConFifoSplit_DebeGenerarVentaMayoristaConMultiplesSaleDetailsYMovementDetails()
        {
            // 1. Configurar lote 1 (_testBatch) con 6 unidades en Bodega
            _bodegaBatchLocation.CurrentStock = 6;
            await _context.SaveChangesAsync();

            // 2. Crear lote 2 temporal en Bodega con 8 unidades
            var batch2 = new Batch
            {
                RestockId = 7,
                ProductId = 1,
                BatchStatusId = 1, // Activo
                InitialQuantity = 20,
                UnitProductionCost = 35.00m,
                ExpirationDate = new DateOnly(2026, 12, 31),
                BatchCode = "TEST-FIFO-SPLIT-SALE-L2"
            };
            _context.Batches.Add(batch2);
            await _context.SaveChangesAsync();
            _createdBatchIds.Add(batch2.Id);

            var bl2Bodega = new BatchLocation
            {
                BatchId = batch2.Id,
                LocationId = 1, // Bodega
                CurrentStock = 8
            };
            _context.BatchLocations.Add(bl2Bodega);

            var bl2Reservado = new BatchLocation
            {
                BatchId = batch2.Id,
                LocationId = 3, // Reservado
                CurrentStock = 0
            };
            _context.BatchLocations.Add(bl2Reservado);
            await _context.SaveChangesAsync();

            // Total disponible en Bodega: 6 + 8 = 14. Pedimos 10 unidades a precio Mayoreo (40.00 C$, ProductPriceId = 3)
            int cantidadSolicitada = 10;
            decimal precioMayoreo = 40.00m;
            decimal totalEsperado = cantidadSolicitada * precioMayoreo; // 400.00 C$

            var createDto = new CreateOrderDto
            {
                CustomerId = 1,
                CreatedBy = 1,
                EstimatedDeliveryDate = DateTime.Now.AddDays(3),
                Details = new List<CreateOrderDetailDto>
                {
                    new CreateOrderDetailDto
                    {
                        ProductId = 1,
                        ProductPriceId = 3, // Precio Mayoreo
                        QuantityRequested = cantidadSolicitada,
                        BatchId = null // Dispara FIFO split
                    }
                }
            };

            var orderResult = await _orderService.CreateAsync(createDto);
            Assert.That(orderResult.Success, Is.True, orderResult.ErrorMessage);
            var orderId = orderResult.Data!.Id;
            _createdOrderIds.Add(orderId);

            // Transición 1: Pendiente -> En Proceso (Reserva)
            var resResult = await _orderService.UpdateStatusAsync(orderId, new UpdateOrderStatusDto { OrderStatusId = 2 });
            Assert.That(resResult.Success, Is.True, resResult.ErrorMessage);
            Assert.That(resResult.Data!.OrderStatusName, Is.EqualTo("En Proceso"));

            // Transición 2: En Proceso -> Entregado (Venta Mayorista)
            var deliverResult = await _orderService.UpdateStatusAsync(orderId, new UpdateOrderStatusDto { OrderStatusId = 3 });
            Assert.That(deliverResult.Success, Is.True, deliverResult.ErrorMessage);
            Assert.That(deliverResult.Data!.OrderStatusName, Is.EqualTo("Entregado"));

            // 1. Verificar Order en BD
            var orderInDb = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            Assert.That(orderInDb, Is.Not.Null);
            Assert.That(orderInDb!.OrderStatusId, Is.EqualTo(3)); // Entregado
            Assert.That(orderInDb.ActualDeliveryDate, Is.Not.Null);

            // 2. Verificar Sale generada (Venta Mayorista)
            var sale = await _context.Sales
                .Include(s => s.SaleDetails)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.OrderId == orderId);

            Assert.That(sale, Is.Not.Null, "Debe existir una venta asociada al OrderId");
            Assert.That(sale!.SaleTypeId, Is.EqualTo(2), "El tipo de venta debe ser Mayoreo (2)");
            Assert.That(sale.PaymentStatus, Is.EqualTo("Pagado"));
            Assert.That(sale.TotalSale, Is.EqualTo(totalEsperado), "El total de la venta debe ser exactamente 400.00 C$");

            // 3. Verificar SaleDetails: deben existir exactamente 2 líneas correspondientes a los 2 lotes
            Assert.That(sale.SaleDetails.Count, Is.EqualTo(2), "La venta debe tener 2 SaleDetails correspondientes a los 2 OrderDetails");

            var sd1 = sale.SaleDetails.FirstOrDefault(sd => sd.BatchId == _testBatch.Id);
            var sd2 = sale.SaleDetails.FirstOrDefault(sd => sd.BatchId == batch2.Id);

            Assert.That(sd1, Is.Not.Null, "Debe existir SaleDetail para el Lote 1");
            Assert.That(sd1!.Quantity, Is.EqualTo(6));
            Assert.That(sd1.AppliedPrice, Is.EqualTo(precioMayoreo));
            Assert.That(sd1.LineSubtotal, Is.EqualTo(6 * precioMayoreo)); // 240.00

            Assert.That(sd2, Is.Not.Null, "Debe existir SaleDetail para el Lote 2");
            Assert.That(sd2!.Quantity, Is.EqualTo(4));
            Assert.That(sd2.AppliedPrice, Is.EqualTo(precioMayoreo));
            Assert.That(sd2.LineSubtotal, Is.EqualTo(4 * precioMayoreo)); // 160.00

            Assert.That(sale.SaleDetails.Sum(sd => sd.LineSubtotal), Is.EqualTo(totalEsperado));

            // 4. Verificar Payment generado
            Assert.That(sale.Payments.Count, Is.EqualTo(1));
            var payment = sale.Payments.First();
            Assert.That(payment.AmountReceived, Is.EqualTo(totalEsperado));

            // 5. Verificar InventoryMovement de tipo Venta (MovementTypeId = 2)
            var movement = await _context.InventoryMovements
                .Include(m => m.MovementDetails)
                .FirstOrDefaultAsync(m => m.OrderId == orderId && m.MovementTypeId == 2);

            Assert.That(movement, Is.Not.Null, "Debe existir movimiento de inventario tipo Venta (2)");
            Assert.That(movement!.SaleId, Is.EqualTo(sale.Id));
            Assert.That(movement.OrderId, Is.EqualTo(orderId));
            Assert.That(movement.MovementDetails.Count, Is.EqualTo(2), "El movimiento de venta debe tener 2 MovementDetails");

            var md1 = movement.MovementDetails.FirstOrDefault(md => md.BatchId == _testBatch.Id);
            var md2 = movement.MovementDetails.FirstOrDefault(md => md.BatchId == batch2.Id);

            Assert.That(md1, Is.Not.Null);
            Assert.That(md1!.SourceLocationId, Is.EqualTo(3), "El origen debe ser Reservado (3)");
            Assert.That(md1.DestinationLocationId, Is.Null, "El destino debe ser null (salida del sistema)");
            Assert.That(md1.Quantity, Is.EqualTo(6));

            Assert.That(md2, Is.Not.Null);
            Assert.That(md2!.SourceLocationId, Is.EqualTo(3), "El origen debe ser Reservado (3)");
            Assert.That(md2.DestinationLocationId, Is.Null, "El destino debe ser null (salida del sistema)");
            Assert.That(md2.Quantity, Is.EqualTo(4));

            // 6. Verificar que el stock en Reservado (3) quedó en 0 para ambos lotes
            var stockL1ResFinal = await _context.BatchLocations.AsNoTracking().FirstOrDefaultAsync(bl => bl.BatchId == _testBatch.Id && bl.LocationId == 3);
            var stockL2ResFinal = await _context.BatchLocations.AsNoTracking().FirstOrDefaultAsync(bl => bl.BatchId == batch2.Id && bl.LocationId == 3);

            Assert.That(stockL1ResFinal!.CurrentStock, Is.EqualTo(0));
            Assert.That(stockL2ResFinal!.CurrentStock, Is.EqualTo(0));
        }
    }
}
