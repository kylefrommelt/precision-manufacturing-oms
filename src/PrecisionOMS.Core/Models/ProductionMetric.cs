using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrecisionOMS.Core.Models;

public class ProductionMetric
{
    public int Id { get; set; }
    
    [Required]
    public int ProductionOrderId { get; set; }
    
    public virtual ProductionOrder ProductionOrder { get; set; } = null!;
    
    public MetricType Type { get; set; }
    
    [Required]
    [StringLength(100)]
    public string MetricName { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal Value { get; set; }
    
    [StringLength(50)]
    public string Unit { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal? TargetValue { get; set; }
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal? MinValue { get; set; }
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal? MaxValue { get; set; }
    
    public bool IsWithinTolerance { get; set; } = true;
    
    public DateTime MeasuredDate { get; set; } = DateTime.UtcNow;
    
    [StringLength(100)]
    public string MeasuredBy { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

public enum MetricType
{
    CycleTime = 1,
    ThroughputRate = 2,
    QualityRate = 3,
    ScrapRate = 4,
    ReworkRate = 5,
    EquipmentEfficiency = 6,
    MaterialUtilization = 7,
    LaborEfficiency = 8,
    EnergyConsumption = 9,
    DefectRate = 10,
    OnTimeDelivery = 11,
    ProductionVolume = 12,
    MachineDowmtime = 13,
    YieldRate = 14,
    FirstPassYield = 15
} 