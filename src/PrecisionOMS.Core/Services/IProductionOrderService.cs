using PrecisionOMS.Core.Models;

namespace PrecisionOMS.Core.Services;

public interface IProductionOrderService
{
    Task<IEnumerable<ProductionOrder>> GetAllOrdersAsync();
    Task<ProductionOrder?> GetOrderByIdAsync(int id);
    Task<ProductionOrder?> GetOrderByNumberAsync(string orderNumber);
    Task<IEnumerable<ProductionOrder>> GetOrdersByFacilityAsync(int facilityId);
    Task<IEnumerable<ProductionOrder>> GetOrdersByStatusAsync(ProductionStatus status);
    Task<IEnumerable<ProductionOrder>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<ProductionOrder> CreateOrderAsync(ProductionOrder order);
    Task<ProductionOrder> UpdateOrderAsync(ProductionOrder order);
    Task<bool> DeleteOrderAsync(int id);
    Task<bool> UpdateOrderStatusAsync(int id, ProductionStatus status);
    Task<decimal> CalculateEfficiencyRatingAsync(int orderId);
    Task<ProductionAnalytics> GetProductionAnalyticsAsync(int facilityId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<ProductionOrder>> GetCriticalOrdersAsync();
    Task<IEnumerable<ProductionOrder>> GetDelayedOrdersAsync();
    Task<bool> OptimizeProductionScheduleAsync(int facilityId);
}

public class ProductionAnalytics
{
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int DelayedOrders { get; set; }
    public decimal OnTimeDeliveryRate { get; set; }
    public decimal AverageEfficiency { get; set; }
    public decimal TotalProductionCost { get; set; }
    public decimal CostVariance { get; set; }
    public Dictionary<ProductionStatus, int> OrdersByStatus { get; set; } = new();
    public Dictionary<string, decimal> EfficiencyByPartType { get; set; } = new();
} 