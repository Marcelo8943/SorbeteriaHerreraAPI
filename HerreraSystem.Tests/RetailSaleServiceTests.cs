using HerreraSystem.Application.Common;
using HerreraSystem.Application.DTOs.SaleDtos;
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
    public class RetailSaleServiceTests
    {
        private HerreraSystemContext _context = null!;
        private UnitOfWork _unitOfWork = null!;
        private RetailSaleService _service = null!;
        private int _createdSaleId = 0;
        private int _createdMovementId = 0;
        private BatchLocation _testBatchLocation = null!;
        private int _initialStock = 20;

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
            var productRepo = new ProductRepository(_context, dateTimeService);
            var priceRepo = new ProductPriceRepository(_context, dateTimeService);
            var saleRepo = new SaleRepository(_context, dateTimeService);
            var saleDetailRepo = new SaleDetailRepository(_context);
            var paymentRepo = new PaymentRepository(_context);
            var batchLocationRepo = new BatchLocationRepository(_context);
            var inventoryMovementRepo = new InventoryMovementRepository(_context, dateTimeService);
            var movementDetailRepo = new MovementDetailRepository(_context);
            var currentUserService = new FakeCurrentUserService();

            _service = new RetailSaleService(
                _unitOfWork,
                productRepo,
                priceRepo,
                saleRepo,
                saleDetailRepo,
                paymentRepo,
                batchLocationRepo,
                inventoryMovementRepo,
                movementDetailRepo,
                currentUserService,
                dateTimeService);

            // Obtener el primer lote activo del producto 1
            var batch = await _context.Batches
                .FirstOrDefaultAsync(b => b.ProductId == 1 && b.BatchStatusId == 1);

            if (batch == null)
            {
                Assert.Inconclusive("No hay lote activo para producto 1 en la BD.");
                return;
            }

            // Asegurar que existe BatchLocation en Mostrador (LocationId = 2) con stock suficiente
            var bl = await _context.BatchLocations
                .FirstOrDefaultAsync(l => l.BatchId == batch.Id && l.LocationId == 2);

            if (bl == null)
            {
                bl = new BatchLocation
                {
                    BatchId = batch.Id,
                    LocationId = 2,
                    CurrentStock = _initialStock
                };
                _context.BatchLocations.Add(bl);
                await _context.SaveChangesAsync();
            }
            else
            {
                bl.CurrentStock += _initialStock;
                await _context.SaveChangesAsync();
            }

            _testBatchLocation = bl;
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_createdSaleId > 0)
            {
                var payments = await _context.Payments.Where(p => p.SaleId == _createdSaleId).ToListAsync();
                _context.Payments.RemoveRange(payments);

                var saleDetails = await _context.SaleDetails.Where(sd => sd.SaleId == _createdSaleId).ToListAsync();
                _context.SaleDetails.RemoveRange(saleDetails);

                if (_createdMovementId > 0)
                {
                    var movDetails = await _context.MovementDetails.Where(md => md.MovementId == _createdMovementId).ToListAsync();
                    _context.MovementDetails.RemoveRange(movDetails);

                    var mov = await _context.InventoryMovements.FirstOrDefaultAsync(m => m.Id == _createdMovementId);
                    if (mov != null) _context.InventoryMovements.Remove(mov);
                }

                var sale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == _createdSaleId);
                if (sale != null) _context.Sales.Remove(sale);

                await _context.SaveChangesAsync();
            }

            if (_testBatchLocation != null)
            {
                var bl = await _context.BatchLocations.FirstOrDefaultAsync(l => l.Id == _testBatchLocation.Id);
                if (bl != null)
                {
                    bl.CurrentStock -= _initialStock;
                    if (bl.CurrentStock < 0) bl.CurrentStock = 0;
                    await _context.SaveChangesAsync();
                }
            }

            await _context.DisposeAsync();
        }

        [Test]
        public async Task CreateRetailSaleAsync_DebeCrearVentaMovimientoYDescontarStockConUnitOfWork()
        {
            var stockAntes = _testBatchLocation.CurrentStock;
            int cantidadVenta = 2;

            var dto = new CreateRetailSaleDto
            {
                PaymentMethodId = 1,
                Notes = "Prueba de venta retail automatizada",
                Items = new List<CreateSaleItemDto>
                {
                    new CreateSaleItemDto
                    {
                        ProductId = 1,
                        Quantity = cantidadVenta
                    }
                }
            };

            var result = await _service.CreateRetailSaleAsync(dto);

            Assert.That(result.Success, Is.True, result.ErrorMessage);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.SaleId, Is.GreaterThan(0));
            Assert.That(result.Data.InventoryMovementId, Is.GreaterThan(0));

            _createdSaleId = result.Data.SaleId;
            _createdMovementId = result.Data.InventoryMovementId;

            // Verificar Sale
            var sale = await _context.Sales
                .AsNoTracking()
                .Include(s => s.SaleDetails)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == _createdSaleId);

            Assert.That(sale, Is.Not.Null);
            Assert.That(sale!.PaymentStatus, Is.EqualTo("Pagado"));
            Assert.That(sale.SaleTypeId, Is.EqualTo(1)); // Detalle
            Assert.That(sale.SaleDetails.Count, Is.EqualTo(1));
            Assert.That(sale.Payments.Count, Is.EqualTo(1));

            // Verificar InventoryMovement
            var movement = await _context.InventoryMovements
                .AsNoTracking()
                .Include(m => m.MovementDetails)
                .FirstOrDefaultAsync(m => m.Id == _createdMovementId);

            Assert.That(movement, Is.Not.Null);
            Assert.That(movement!.MovementTypeId, Is.EqualTo(2)); // Salida por venta al detalle
            Assert.That(movement.SaleId, Is.EqualTo(_createdSaleId));
            Assert.That(movement.MovementDetails.Count, Is.EqualTo(1));
            Assert.That(movement.MovementDetails.First().SourceLocationId, Is.EqualTo(2)); // Mostrador
            Assert.That(movement.MovementDetails.First().DestinationLocationId, Is.Null); // Salida del sistema

            // Verificar descuento de stock en BatchLocation
            var blDespues = await _context.BatchLocations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == _testBatchLocation.Id);

            Assert.That(blDespues, Is.Not.Null);
            Assert.That(blDespues!.CurrentStock, Is.EqualTo(stockAntes - cantidadVenta));
        }
    }
}
