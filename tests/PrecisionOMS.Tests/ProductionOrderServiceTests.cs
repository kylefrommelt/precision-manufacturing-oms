using Microsoft.EntityFrameworkCore;
using PrecisionOMS.Core.Models;
using PrecisionOMS.Core.Services;
using PrecisionOMS.Data;
using Xunit;

namespace PrecisionOMS.Tests;

public class ProductionOrderServiceTests
{
    private ManufacturingContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ManufacturingContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ManufacturingContext(options);
        
        // Seed test data
        SeedTestData(context);
        
        return context;
    }

    private void SeedTestData(ManufacturingContext context)
    {
        var facility = new Facility
        {
            Id = 1,
            Name = "Test Facility",
            Code = "TST",
            Type = FacilityType.InvestmentCasting,
            IsActive = true
        };

        var testOrders = new List<ProductionOrder>
        {
            new ProductionOrder
            {
                Id = 1,
                OrderNumber = "TST-001",
                PartNumber = "TEST-PART-001",
                PartDescription = "Test Aerospace Component",
                Quantity = 10,
                QuantityCompleted = 10,
                Status = ProductionStatus.Completed,
                Priority = Priority.Medium,
                ScheduledStartDate = DateTime.UtcNow.AddDays(-10),
                ScheduledEndDate = DateTime.UtcNow.AddDays(-5),
                ActualStartDate = DateTime.UtcNow.AddDays(-10),
                ActualEndDate = DateTime.UtcNow.AddDays(-6),
                FacilityId = 1,
                CustomerName = "Test Customer",
                MaterialType = MaterialType.InconelAlloy,
                EstimatedCost = 50000m,
                ActualCost = 48000m,
                CreatedDate = DateTime.UtcNow.AddDays(-15)
            },
            new ProductionOrder
            {
                Id = 2,
                OrderNumber = "TST-002",
                PartNumber = "TEST-PART-002",
                PartDescription = "Test Engine Component",
                Quantity = 5,
                QuantityCompleted = 0,
                Status = ProductionStatus.InProgress,
                Priority = Priority.High,
                ScheduledStartDate = DateTime.UtcNow.AddDays(-2),
                ScheduledEndDate = DateTime.UtcNow.AddDays(3),
                ActualStartDate = DateTime.UtcNow.AddDays(-2),
                ActualEndDate = null,
                FacilityId = 1,
                CustomerName = "Test Customer 2",
                MaterialType = MaterialType.TitaniumAlloy,
                EstimatedCost = 75000m,
                ActualCost = 0m,
                CreatedDate = DateTime.UtcNow.AddDays(-7)
            },
            new ProductionOrder
            {
                Id = 3,
                OrderNumber = "TST-003",
                PartNumber = "TEST-PART-003",
                PartDescription = "Test Delayed Component",
                Quantity = 8,
                QuantityCompleted = 6,
                Status = ProductionStatus.InProgress,
                Priority = Priority.Critical,
                ScheduledStartDate = DateTime.UtcNow.AddDays(-8),
                ScheduledEndDate = DateTime.UtcNow.AddDays(-2), // Past due
                ActualStartDate = DateTime.UtcNow.AddDays(-8),
                ActualEndDate = null,
                FacilityId = 1,
                CustomerName = "Test Customer 3",
                MaterialType = MaterialType.NickelAlloy,
                EstimatedCost = 60000m,
                ActualCost = 65000m,
                CreatedDate = DateTime.UtcNow.AddDays(-12)
            }
        };

        context.Facilities.Add(facility);
        context.ProductionOrders.AddRange(testOrders);
        context.SaveChanges();
    }

    [Fact]
    public async Task GetAllOrdersAsync_ReturnsAllOrders()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new ProductionOrderService(context);

        // Act
        var result = await service.GetAllOrdersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetOrderByIdAsync_ValidId_ReturnsOrder()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new ProductionOrderService(context);

        // Act
        var result = await service.GetOrderByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TST-001", result.OrderNumber);
        Assert.Equal("TEST-PART-001", result.PartNumber);
    }

    [Fact]
    public async Task GetOrderByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new ProductionOrderService(context);

        // Act
        var result = await service.GetOrderByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrderByNumberAsync_ValidNumber_ReturnsOrder()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new ProductionOrderService(context);

        // Act
        var result = await service.GetOrderByNumberAsync("TST-002");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal("TEST-PART-002", result.PartNumber);
    }

    [Fact]
    public async Task GetOrdersByStatusAsync_ReturnsFilteredOrders()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new ProductionOrderService(context);

        // Act
        var result = await service.GetOrdersByStatusAsync(ProductionStatus.InProgress);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, order => Assert.Equal(ProductionStatus.InProgress, order.Status));
    }

    [Fact]
    public async Task CreateOrderAsync_ValidOrder_CreatesSuccessfully()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new ProductionOrderService(context);
        
        var newOrder = new ProductionOrder
        {
            PartNumber = "NEW-PART-001",
            PartDescription = "New Test Component",
            Quantity = 15,
            Status = ProductionStatus.Planned,
            Priority = Priority.Medium,
            ScheduledStartDate = DateTime.UtcNow.AddDays(1),
            ScheduledEndDate = DateTime.UtcNow.AddDays(10),
            FacilityId = 1,
            CustomerName = "New Customer",
            MaterialType = MaterialType.SteelAlloy,
            EstimatedCost = 85000m
        };

        // Act
        var result = await service.CreateOrderAsync(newOrder);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.False(string.IsNullOrEmpty(result.OrderNumber));
        Assert.Contains("TST-", result.OrderNumber); // Should use facility code
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ValidIdAndStatus_UpdatesSuccessfully()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new ProductionOrderService(context);

        // Act
        var result = await service.UpdateOrderStatusAsync(2, ProductionStatus.Completed);

        // Assert
        Assert.True(result);
        
        var updatedOrder = await service.GetOrderByIdAsync(2);
        Assert.Equal(ProductionStatus.Completed, updatedOrder.Status);
        Assert.NotNull(updatedOrder.ActualEndDate);
        Assert.Equal(updatedOrder.Quantity, updatedOrder.QuantityCompleted);
    }

    [Fact]
    public async Task CalculateEfficiencyRatingAsync_CompletedOrder_ReturnsValidRating()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new ProductionOrderService(context);

        // Act
        var result = await service.CalculateEfficiencyRatingAsync(1);

        // Assert
        Assert.True(result > 0);
        Assert.True(result <= 200); // Should be a reasonable efficiency rating
    }

    [Fact]
    public async Task GetCriticalOrdersAsync_ReturnsCriticalAndUrgentOrders()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new ProductionOrderService(context);

        // Act
        var result = await service.GetCriticalOrdersAsync();

        // Assert
        Assert.NotNull(result);
        var criticalOrders = result.ToList();
        
        // Should include orders with Critical priority or those due soon
        Assert.Contains(criticalOrders, o => o.Priority == Priority.Critical);
    }

    [Fact]
    public async Task GetDelayedOrdersAsync_ReturnsOverdueOrders()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new ProductionOrderService(context);

        // Act
        var result = await service.GetDelayedOrdersAsync();

        // Assert
        Assert.NotNull(result);
        var delayedOrders = result.ToList();
        
        // Should include order TST-003 which is past its scheduled end date
        Assert.Contains(delayedOrders, o => o.OrderNumber == "TST-003");
        Assert.All(delayedOrders, order => 
        {
            Assert.NotEqual(ProductionStatus.Completed, order.Status);
            Assert.True(order.ScheduledEndDate < DateTime.UtcNow);
        });
    }

    [Fact]
    public async Task GetProductionAnalyticsAsync_ReturnsValidAnalytics()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new ProductionOrderService(context);
        var startDate = DateTime.UtcNow.AddDays(-20);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await service.GetProductionAnalyticsAsync(1, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalOrders);
        Assert.Equal(1, result.CompletedOrders);
        Assert.True(result.TotalProductionCost > 0);
        Assert.NotNull(result.OrdersByStatus);
        Assert.True(result.OrdersByStatus.Count > 0);
    }

    [Fact]
    public async Task DeleteOrderAsync_ValidId_DeletesSuccessfully()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new ProductionOrderService(context);

        // Act
        var result = await service.DeleteOrderAsync(1);

        // Assert
        Assert.True(result);
        
        var deletedOrder = await service.GetOrderByIdAsync(1);
        Assert.Null(deletedOrder);
    }

    [Fact]
    public async Task DeleteOrderAsync_InvalidId_ReturnsFalse()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new ProductionOrderService(context);

        // Act
        var result = await service.DeleteOrderAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task OptimizeProductionScheduleAsync_ReschedulesOrdersByPriority()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new ProductionOrderService(context);

        // Set some orders to Planned status for optimization
        await service.UpdateOrderStatusAsync(2, ProductionStatus.Planned);
        await service.UpdateOrderStatusAsync(3, ProductionStatus.Planned);

        // Act
        var result = await service.OptimizeProductionScheduleAsync(1);

        // Assert
        Assert.True(result);
        
        // Verify that orders have been rescheduled
        var orders = await service.GetOrdersByFacilityAsync(1);
        var plannedOrders = orders.Where(o => o.Status == ProductionStatus.Planned).ToList();
        
        // Critical priority orders should be scheduled first
        var criticalOrder = plannedOrders.FirstOrDefault(o => o.Priority == Priority.Critical);
        var highOrder = plannedOrders.FirstOrDefault(o => o.Priority == Priority.High);
        
        if (criticalOrder != null && highOrder != null)
        {
            Assert.True(criticalOrder.ScheduledStartDate <= highOrder.ScheduledStartDate);
        }
    }

    [Theory]
    [InlineData(ProductionStatus.Planned)]
    [InlineData(ProductionStatus.InProgress)]
    [InlineData(ProductionStatus.OnHold)]
    [InlineData(ProductionStatus.Completed)]
    public async Task UpdateOrderStatusAsync_VariousStatuses_UpdatesCorrectly(ProductionStatus status)
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new ProductionOrderService(context);

        // Act
        var result = await service.UpdateOrderStatusAsync(2, status);

        // Assert
        Assert.True(result);
        
        var updatedOrder = await service.GetOrderByIdAsync(2);
        Assert.Equal(status, updatedOrder.Status);
        
        // Verify status-specific behavior
        if (status == ProductionStatus.InProgress && updatedOrder.ActualStartDate == null)
        {
            Assert.NotNull(updatedOrder.ActualStartDate);
        }
        else if (status == ProductionStatus.Completed)
        {
            Assert.NotNull(updatedOrder.ActualEndDate);
            Assert.Equal(updatedOrder.Quantity, updatedOrder.QuantityCompleted);
        }
    }
} 