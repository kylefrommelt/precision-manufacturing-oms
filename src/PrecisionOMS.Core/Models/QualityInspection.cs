using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrecisionOMS.Core.Models;

public class QualityInspection
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string InspectionNumber { get; set; } = string.Empty;
    
    [Required]
    public int ProductionOrderId { get; set; }
    
    public virtual ProductionOrder ProductionOrder { get; set; } = null!;
    
    [Required]
    public int FacilityId { get; set; }
    
    public virtual Facility Facility { get; set; } = null!;
    
    public InspectionType Type { get; set; }
    
    public InspectionStatus Status { get; set; } = InspectionStatus.Pending;
    
    public DateTime ScheduledDate { get; set; }
    
    public DateTime? ActualDate { get; set; }
    
    [Required]
    [StringLength(100)]
    public string InspectorName { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string InspectorBadge { get; set; } = string.Empty;
    
    public bool Passed { get; set; }
    
    [StringLength(1000)]
    public string Results { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string NonConformanceNotes { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string CorrectiveActions { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(10,4)")]
    public decimal? DimensionTolerance { get; set; }
    
    [Column(TypeName = "decimal(10,4)")]
    public decimal? MeasuredDimension { get; set; }
    
    [StringLength(100)]
    public string MeasurementUnit { get; set; } = string.Empty;
    
    public int? DefectCount { get; set; }
    
    [StringLength(200)]
    public string CertificationRequired { get; set; } = string.Empty;
    
    public bool CertificationCompleted { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedDate { get; set; }
    
    public virtual ICollection<QualityDocument> QualityDocuments { get; set; } = new List<QualityDocument>();
}

public enum InspectionType
{
    Incoming = 1,
    InProcess = 2,
    Final = 3,
    Dimensional = 4,
    Visual = 5,
    Metallurgical = 6,
    NonDestructiveTest = 7,
    PressureTest = 8,
    FlowTest = 9,
    Radiographic = 10,
    DyePenetrant = 11,
    MagneticParticle = 12
}

public enum InspectionStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    OnHold = 4,
    Failed = 5,
    Rework = 6,
    Approved = 7
} 