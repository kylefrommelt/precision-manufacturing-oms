using Microsoft.EntityFrameworkCore;
using PrecisionOMS.Core.Models;
using PrecisionOMS.Data;

namespace PrecisionOMS.Core.Services;

public class ProductionOrderService : IProductionOrderService
{
    private readonly ManufacturingContext _context;

    public ProductionOrderService(ManufacturingContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductionOrder>> GetAllOrdersAsync()
    {
        return await _context.ProductionOrders
            .Include(po => po.Facility)
            .Include(po => po.QualityInspections)
            .Include(po => po.ProductionMetrics)
            .OrderByDescending(po => po.CreatedDate)
            .ToListAsync();
    }

    public async Task<ProductionOrder?> GetOrderByIdAsync(int id)
    {
        return await _context.ProductionOrders
            .Include(po => po.Facility)
            .Include(po => po.QualityInspections)
            .Include(po => po.ProductionMetrics)
            .FirstOrDefaultAsync(po => po.Id == id);
    }

    public async Task<ProductionOrder?> GetOrderByNumberAsync(string orderNumber)
    {
        return await _context.ProductionOrders
            .Include(po => po.Facility)
            .Include(po => po.QualityInspections)
            .Include(po => po.ProductionMetrics)
            .FirstOrDefaultAsync(po => po.OrderNumber == orderNumber);
    }

    public async Task<IEnumerable<ProductionOrder>> GetOrdersByFacilityAsync(int facilityId)
    {
        return await _context.ProductionOrders
            .Include(po => po.Facility)
            .Where(po => po.FacilityId == facilityId)
            .OrderBy(po => po.ScheduledStartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrder>> GetOrdersByStatusAsync(ProductionStatus status)
    {
        return await _context.ProductionOrders
            .Include(po => po.Facility)
            .Where(po => po.Status == status)
            .OrderBy(po => po.ScheduledStartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrder>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ProductionOrders
            .Include(po => po.Facility)
            .Where(po => po.ScheduledStartDate >= startDate && po.ScheduledEndDate <= endDate)
            .OrderBy(po => po.ScheduledStartDate)
            .ToListAsync();
    }

    public async Task<ProductionOrder> CreateOrderAsync(ProductionOrder order)
    {
        order.CreatedDate = DateTime.UtcNow;
        
        // Auto-generate order number if not provided
        if (string.IsNullOrEmpty(order.OrderNumber))
        {
            order.OrderNumber = await GenerateOrderNumberAsync(order.FacilityId);
        }

        _context.ProductionOrders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<ProductionOrder> UpdateOrderAsync(ProductionOrder order)
    {
        _context.ProductionOrders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        var order = await _context.ProductionOrders.FindAsync(id);
        if (order == null) return false;

        _context.ProductionOrders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateOrderStatusAsync(int id, ProductionStatus status)
    {
        var order = await _context.ProductionOrders.FindAsync(id);
        if (order == null) return false;

        order.Status = status;
        
        // Update actual dates based on status
        switch (status)
        {
            case ProductionStatus.InProgress when order.ActualStartDate == null:
                order.ActualStartDate = DateTime.UtcNow;
                break;
            case ProductionStatus.Completed:
                order.ActualEndDate = DateTime.UtcNow;
                order.QuantityCompleted = order.Quantity;
                break;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> CalculateEfficiencyRatingAsync(int orderId)
    {
        var order = await GetOrderByIdAsync(orderId);
        if (order?.ActualStartDate == null || order.ActualEndDate == null)
            return 0;

        var scheduledDuration = (order.ScheduledEndDate - order.ScheduledStartDate).TotalHours;
        var actualDuration = (order.ActualEndDate.Value - order.ActualStartDate.Value).TotalHours;
        
        var timeEfficiency = (decimal)(scheduledDuration / actualDuration) * 100;
        var qualityEfficiency = order.QuantityCompleted / (decimal)order.Quantity * 100;
        var costEfficiency = order.EstimatedCost / Math.Max(order.ActualCost, 1) * 100;

        return (timeEfficiency + qualityEfficiency + costEfficiency) / 3;
    }

    public async Task<ProductionAnalytics> GetProductionAnalyticsAsync(int facilityId, DateTime startDate, DateTime endDate)
    {
        var orders = await _context.ProductionOrders
            .Include(po => po.ProductionMetrics)
            .Where(po => po.FacilityId == facilityId && 
                        po.CreatedDate >= startDate && 
                        po.CreatedDate <= endDate)
            .ToListAsync();

        var analytics = new ProductionAnalytics
        {
            TotalOrders = orders.Count,
            CompletedOrders = orders.Count(o => o.Status == ProductionStatus.Completed),
            DelayedOrders = orders.Count(o => o.ActualEndDate > o.ScheduledEndDate),
            TotalProductionCost = orders.Sum(o => o.ActualCost)
        };

        analytics.OnTimeDeliveryRate = analytics.TotalOrders > 0 
            ? (analytics.CompletedOrders - analytics.DelayedOrders) / (decimal)analytics.TotalOrders * 100 
            : 0;

        analytics.OrdersByStatus = orders.GroupBy(o => o.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        return analytics;
    }

    public async Task<IEnumerable<ProductionOrder>> GetCriticalOrdersAsync()
    {
        var criticalDate = DateTime.UtcNow.AddDays(7); // Orders due within 7 days
        
        return await _context.ProductionOrders
            .Include(po => po.Facility)
            .Where(po => po.Priority == Priority.Critical || 
                        (po.Status != ProductionStatus.Completed && 
                         po.ScheduledEndDate <= criticalDate))
            .OrderBy(po => po.ScheduledEndDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionOrder>> GetDelayedOrdersAsync()
    {
        return await _context.ProductionOrders
            .Include(po => po.Facility)
            .Where(po => po.Status != ProductionStatus.Completed && 
                        po.ScheduledEndDate < DateTime.UtcNow)
            .OrderBy(po => po.ScheduledEndDate)
            .ToListAsync();
    }

    public async Task<bool> OptimizeProductionScheduleAsync(int facilityId)
    {
        var pendingOrders = await _context.ProductionOrders
            .Where(po => po.FacilityId == facilityId && 
                        po.Status == ProductionStatus.Planned)
            .OrderBy(po => po.Priority)
            .ThenBy(po => po.ScheduledStartDate)
            .ToListAsync();

        var currentDate = DateTime.UtcNow;
        
        foreach (var order in pendingOrders)
        {
            // Simple optimization: reschedule based on priority and estimated duration
            var estimatedDuration = (order.ScheduledEndDate - order.ScheduledStartDate).TotalDays;
            
            order.ScheduledStartDate = currentDate;
            order.ScheduledEndDate = currentDate.AddDays(estimatedDuration);
            
            currentDate = order.ScheduledEndDate.AddDays(0.5); // Add buffer time
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<string> GenerateOrderNumberAsync(int facilityId)
    {
        var facility = await _context.Facilities.FindAsync(facilityId);
        var facilityCode = facility?.Code ?? "FAC";
        var dateCode = DateTime.UtcNow.ToString("yyyyMMdd");
        
        var orderCount = await _context.ProductionOrders
            .CountAsync(po => po.FacilityId == facilityId && 
                             po.CreatedDate.Date == DateTime.UtcNow.Date);
        
        return $"{facilityCode}-{dateCode}-{(orderCount + 1):D3}";
    }
} 