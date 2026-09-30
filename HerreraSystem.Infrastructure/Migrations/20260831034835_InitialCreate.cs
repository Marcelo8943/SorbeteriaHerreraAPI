using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HerreraSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatchStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchStatusName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BatchSta__3214EC0755474FB9", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ruc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Departme__3214EC0742C9F50F", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Flavors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlavorName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    ImageURL = table.Column<string>(type: "varchar(2048)", unicode: false, maxLength: 2048, nullable: true),
                    FlavorColor = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Flavors__3214EC072A0E89A0", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Lines__3214EC0738136C76", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Location__3214EC075DA65C6F", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovementTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovementTypeName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Sign = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Movement__3214EC077C997F22", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderStatusName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OrderSta__3214EC073A31751A", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentMethodName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentM__3214EC07E82D4CDB", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentTypeName = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentT__3214EC07D013BFE6", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Presentations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PresentationName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Presenta__3214EC07994D0105", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PriceName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PriceTyp__3214EC074AFD7546", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    RoleDescription = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__3214EC072E1E2D97", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaleTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleTypeName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    SaleTypeDescription = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SaleType__3214EC0723E150E3", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    ResetToken = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ResetTokenExpiry = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__3214EC072725DF1E", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Municipalities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    MunicipalityName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Municipa__3214EC072203A186", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Department_Municipality",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LinePresentations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineId = table.Column<int>(type: "int", nullable: false),
                    PresentationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LinePres__3214EC07A37CA463", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Line",
                        column: x => x.LineId,
                        principalTable: "Lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Presentation",
                        column: x => x.PresentationId,
                        principalTable: "Presentations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Restocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestockDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    RestockCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Producti__3214EC073165B437", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Restock_User_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserRole__3214EC07346CF698", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Role",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MunicipalityId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    LastName = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    PointOfSale = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true),
                    POSAddress = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Customer__3214EC07061A3DB2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customer_Municipality",
                        column: x => x.MunicipalityId,
                        principalTable: "Municipalities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LinePresentationId = table.Column<int>(type: "int", nullable: false),
                    FlavorId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    ImageURL = table.Column<string>(type: "varchar(2048)", unicode: false, maxLength: 2048, nullable: true),
                    MinimumStock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Products__3214EC078717DBEB", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flavor",
                        column: x => x.FlavorId,
                        principalTable: "Flavors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LinePresentation",
                        column: x => x.LinePresentationId,
                        principalTable: "LinePresentations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    OrderStatusId = table.Column<int>(type: "int", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ActualDeliveryDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    TotalOrder = table.Column<decimal>(type: "decimal(10,2)", nullable: true, defaultValue: 0m),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    OrderCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Orders__3214EC07D1B14B5E", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Order_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Order_Status",
                        column: x => x.OrderStatusId,
                        principalTable: "OrderStatuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Order_User_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Batches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestockId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    BatchStatusId = table.Column<int>(type: "int", nullable: false),
                    InitialQuantity = table.Column<int>(type: "int", nullable: false),
                    UnitProductionCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BatchCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Batches__3214EC07F0175079", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Batch_Product",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Batch_Restock",
                        column: x => x.RestockId,
                        principalTable: "Restocks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Batch_Status",
                        column: x => x.BatchStatusId,
                        principalTable: "BatchStatuses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PriceTypeId = table.Column<int>(type: "int", nullable: false),
                    LinePresentationId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    ValidTo = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ProductP__3214EC0795BEEC37", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinePresentation_Price",
                        column: x => x.LinePresentationId,
                        principalTable: "LinePresentations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PriceType",
                        column: x => x.PriceTypeId,
                        principalTable: "PriceTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Product_Price",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_Price_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    SaleDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    TotalSale = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    PendingBalance = table.Column<decimal>(type: "decimal(10,2)", nullable: true, defaultValue: 0m),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    PaymentTypeId = table.Column<int>(type: "int", nullable: false),
                    SaleTypeId = table.Column<int>(type: "int", nullable: false),
                    SaleCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Sales__3214EC070AAA74F8", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sale_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sale_Order",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sale_PaymentType",
                        column: x => x.PaymentTypeId,
                        principalTable: "PaymentTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sale_SaleType",
                        column: x => x.SaleTypeId,
                        principalTable: "SaleTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sale_User_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BatchLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    CurrentStock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BatchLoc__3214EC072F9B81E0", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchLocation_Batch",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BatchLocation_Location",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    QuantityRequested = table.Column<int>(type: "int", nullable: false),
                    ProductPriceId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OrderDet__3214EC0723A315DA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdDet_Batch",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrdDet_Order",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrdDet_Price",
                        column: x => x.ProductPriceId,
                        principalTable: "ProductPrices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrdDet_Product",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InventoryMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovementTypeId = table.Column<int>(type: "int", nullable: false),
                    SaleId = table.Column<int>(type: "int", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    MovementDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Notes = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Inventor__3214EC07F67FBA38", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvMov_MovementType",
                        column: x => x.MovementTypeId,
                        principalTable: "MovementTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvMov_Order",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvMov_Sale",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvMov_User_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    AmountReceived = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    TransactionReference = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    RegisteredBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Payments__3214EC07A9E7F76F", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payment_Method",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payment_Sale",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payment_User_RegisteredBy",
                        column: x => x.RegisteredBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SaleDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    AppliedPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    LineSubtotal = table.Column<decimal>(type: "decimal(21,2)", nullable: true, computedColumnSql: "([Quantity]*[AppliedPrice])", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SaleDeta__3214EC07A5B246F4", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleDet_Batch",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SaleDet_Product",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SaleDet_Sale",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MovementDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovementId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    SourceLocationId = table.Column<int>(type: "int", nullable: true),
                    DestinationLocationId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    UnitCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Movement__3214EC076F6ED98A", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovDet_Batch",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MovDet_DestLoc",
                        column: x => x.DestinationLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MovDet_Movement",
                        column: x => x.MovementId,
                        principalTable: "InventoryMovements",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MovDet_SourceLoc",
                        column: x => x.SourceLocationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MovDet_User_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Batches_BatchStatusId",
                table: "Batches",
                column: "BatchStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_ProductId",
                table: "Batches",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_RestockId",
                table: "Batches",
                column: "RestockId");

            migrationBuilder.CreateIndex(
                name: "UQ__Batches__B22ADA8E7340E5C3",
                table: "Batches",
                column: "BatchCode",
                unique: true,
                filter: "[BatchCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BatchLocations_LocationId",
                table: "BatchLocations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "UQ_BatchLocation",
                table: "BatchLocations",
                columns: new[] { "BatchId", "LocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_MunicipalityId",
                table: "Customers",
                column: "MunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_CreatedBy",
                table: "InventoryMovements",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_MovementTypeId",
                table: "InventoryMovements",
                column: "MovementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_OrderId",
                table: "InventoryMovements",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_SaleId",
                table: "InventoryMovements",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_LinePresentations_PresentationId",
                table: "LinePresentations",
                column: "PresentationId");

            migrationBuilder.CreateIndex(
                name: "UQ_Line_Presentation",
                table: "LinePresentations",
                columns: new[] { "LineId", "PresentationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovementDetails_BatchId",
                table: "MovementDetails",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementDetails_CreatedBy",
                table: "MovementDetails",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MovementDetails_DestinationLocationId",
                table: "MovementDetails",
                column: "DestinationLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementDetails_MovementId",
                table: "MovementDetails",
                column: "MovementId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementDetails_SourceLocationId",
                table: "MovementDetails",
                column: "SourceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_DepartmentId",
                table: "Municipalities",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_BatchId",
                table: "OrderDetails",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductPriceId",
                table: "OrderDetails",
                column: "ProductPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedBy",
                table: "Orders",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderStatusId",
                table: "Orders",
                column: "OrderStatusId");

            migrationBuilder.CreateIndex(
                name: "UQ_OrderCode",
                table: "Orders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentMethodId",
                table: "Payments",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_RegisteredBy",
                table: "Payments",
                column: "RegisteredBy");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SaleId",
                table: "Payments",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_CreatedBy",
                table: "ProductPrices",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_LinePresentationId",
                table: "ProductPrices",
                column: "LinePresentationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_PriceTypeId",
                table: "ProductPrices",
                column: "PriceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_ProductId",
                table: "ProductPrices",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedBy",
                table: "Products",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Products_FlavorId",
                table: "Products",
                column: "FlavorId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_LinePresentationId",
                table: "Products",
                column: "LinePresentationId");

            migrationBuilder.CreateIndex(
                name: "IX_Restocks_CreatedBy",
                table: "Restocks",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "UQ_RestockCode",
                table: "Restocks",
                column: "RestockCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleDetails_BatchId",
                table: "SaleDetails",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleDetails_ProductId",
                table: "SaleDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleDetails_SaleId",
                table: "SaleDetails",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_CreatedBy",
                table: "Sales",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_CustomerId",
                table: "Sales",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_OrderId",
                table: "Sales",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_PaymentTypeId",
                table: "Sales",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_SaleTypeId",
                table: "Sales",
                column: "SaleTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_SaleCode",
                table: "Sales",
                column: "SaleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UQ_User_Role",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D105345433F74E",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Users__C9F2845638A1D3E6",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchLocations");

            migrationBuilder.DropTable(
                name: "BusinessProfiles");

            migrationBuilder.DropTable(
                name: "MovementDetails");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "SaleDetails");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "InventoryMovements");

            migrationBuilder.DropTable(
                name: "ProductPrices");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "Batches");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "MovementTypes");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropTable(
                name: "PriceTypes");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Restocks");

            migrationBuilder.DropTable(
                name: "BatchStatuses");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "PaymentTypes");

            migrationBuilder.DropTable(
                name: "SaleTypes");

            migrationBuilder.DropTable(
                name: "Flavors");

            migrationBuilder.DropTable(
                name: "LinePresentations");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "OrderStatuses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Lines");

            migrationBuilder.DropTable(
                name: "Presentations");

            migrationBuilder.DropTable(
                name: "Municipalities");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
