using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrecisionOMS.Core.Models;

public class Equipment
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string EquipmentNumber { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;
    
    public EquipmentType Type { get; set; }
    
    [StringLength(100)]
    public string Manufacturer { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string SerialNumber { get; set; } = string.Empty;
    
    public DateTime InstallationDate { get; set; }
    
    public DateTime? LastMaintenanceDate { get; set; }
    
    public DateTime? NextMaintenanceDate { get; set; }
    
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Available;
    
    [Required]
    public int FacilityId { get; set; }
    
    public virtual Facility Facility { get; set; } = null!;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PurchaseCost { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal OperatingHours { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal MaintenanceHours { get; set; }
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal EfficiencyRating { get; set; } = 100.0m;
    
    [StringLength(500)]
    public string MaintenanceNotes { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

public enum EquipmentType
{
    CastingFurnace = 1,
    WaxInjectionMachine = 2,
    DippingRobot = 3,
    AutoClave = 4,
    VacuumFurnace = 5,
    CNCMachine = 6,
    Grinder = 7,
    MillingMachine = 8,
    Lathe = 9,
    PressureTester = 10,
    FlowTester = 11,
    XRayMachine = 12,
    CoordinateMeasuringMachine = 13,
    SurfaceGrinder = 14,
    HeatTreatmentFurnace = 15,
    QualityScanner = 16
}

public enum EquipmentStatus
{
    Available = 1,
    InUse = 2,
    Maintenance = 3,
    Breakdown = 4,
    Scheduled = 5,
    Retired = 6
} 