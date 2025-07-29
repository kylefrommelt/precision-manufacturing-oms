using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrecisionOMS.Core.Models;

public class ProductionOrder
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string OrderNumber { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string PartNumber { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string PartDescription { get; set; } = string.Empty;
    
    public int Quantity { get; set; }
    
    public int QuantityCompleted { get; set; } = 0;
    
    public ProductionStatus Status { get; set; } = ProductionStatus.Planned;
    
    public Priority Priority { get; set; } = Priority.Medium;
    
    public DateTime ScheduledStartDate { get; set; }
    
    public DateTime ScheduledEndDate { get; set; }
    
    public DateTime? ActualStartDate { get; set; }
    
    public DateTime? ActualEndDate { get; set; }
    
    [Required]
    public int FacilityId { get; set; }
    
    public virtual Facility Facility { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string CustomerOrderNumber { get; set; } = string.Empty;
    
    public MaterialType MaterialType { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedCost { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ActualCost { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;
    
    public virtual ICollection<QualityInspection> QualityInspections { get; set; } = new List<QualityInspection>();
    public virtual ICollection<ProductionMetric> ProductionMetrics { get; set; } = new List<ProductionMetric>();
}

public enum ProductionStatus
{
    Planned = 1,
    Released = 2,
    InProgress = 3,
    OnHold = 4,
    Completed = 5,
    Cancelled = 6,
    Shipped = 7
}

public enum Priority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum MaterialType
{
    InconelAlloy = 1,
    TitaniumAlloy = 2,
    NickelAlloy = 3,
    SteelAlloy = 4,
    AluminumAlloy = 5,
    SuperAlloy = 6,
    CarbonSteel = 7,
    StainlessSteel = 8
} 