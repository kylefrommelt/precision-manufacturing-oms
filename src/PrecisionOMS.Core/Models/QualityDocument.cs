using System.ComponentModel.DataAnnotations;

namespace PrecisionOMS.Core.Models;

public class QualityDocument
{
    public int Id { get; set; }
    
    [Required]
    public int QualityInspectionId { get; set; }
    
    public virtual QualityInspection QualityInspection { get; set; } = null!;
    
    [Required]
    [StringLength(200)]
    public string DocumentName { get; set; } = string.Empty;
    
    public DocumentType Type { get; set; }
    
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string FileExtension { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    [StringLength(100)]
    public string ApprovedBy { get; set; } = string.Empty;
    
    public DateTime? ApprovedDate { get; set; }
    
    public bool IsApproved { get; set; }
    
    [StringLength(50)]
    public string Version { get; set; } = "1.0";
    
    public bool IsActive { get; set; } = true;
}

public enum DocumentType
{
    InspectionReport = 1,
    CertificationDocument = 2,
    TestResults = 3,
    MaterialCertificate = 4,
    DrawingRevision = 5,
    NonConformanceReport = 6,
    CorrectiveActionReport = 7,
    ProcessSheet = 8,
    QualityManual = 9,
    CalibrationCertificate = 10,
    TechnicalSpecification = 11,
    ComplianceDocument = 12
} 