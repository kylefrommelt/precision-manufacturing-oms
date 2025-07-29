using System.ComponentModel.DataAnnotations;

namespace PrecisionOMS.Core.Models;

public class Facility
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(10)]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string City { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string State { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string Country { get; set; } = string.Empty;
    
    public FacilityType Type { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<ProductionOrder> ProductionOrders { get; set; } = new List<ProductionOrder>();
    public virtual ICollection<QualityInspection> QualityInspections { get; set; } = new List<QualityInspection>();
    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
}

public enum FacilityType
{
    InvestmentCasting = 1,
    AirfoilCasting = 2,
    Forging = 3,
    Machining = 4,
    Assembly = 5,
    QualityControl = 6,
    Warehouse = 7
} 