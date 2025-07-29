using Microsoft.EntityFrameworkCore;
using PrecisionOMS.Core.Models;

namespace PrecisionOMS.Data;

public class ManufacturingContext : DbContext
{
    public ManufacturingContext(DbContextOptions<ManufacturingContext> options) : base(options)
    {
    }

    public DbSet<Facility> Facilities { get; set; }
    public DbSet<ProductionOrder> ProductionOrders { get; set; }
    public DbSet<QualityInspection> QualityInspections { get; set; }
    public DbSet<Equipment> Equipment { get; set; }
    public DbSet<ProductionMetric> ProductionMetrics { get; set; }
    public DbSet<QualityDocument> QualityDocuments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Facility
        modelBuilder.Entity<Facility>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
        });

        // Configure ProductionOrder
        modelBuilder.Entity<ProductionOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PartNumber).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.Facility)
                  .WithMany(f => f.ProductionOrders)
                  .HasForeignKey(e => e.FacilityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure QualityInspection
        modelBuilder.Entity<QualityInspection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.InspectionNumber).IsUnique();
            entity.Property(e => e.InspectionNumber).IsRequired().HasMaxLength(50);
            
            entity.HasOne(e => e.ProductionOrder)
                  .WithMany(po => po.QualityInspections)
                  .HasForeignKey(e => e.ProductionOrderId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Facility)
                  .WithMany(f => f.QualityInspections)
                  .HasForeignKey(e => e.FacilityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Equipment
        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EquipmentNumber).IsUnique();
            entity.Property(e => e.EquipmentNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.Facility)
                  .WithMany(f => f.Equipment)
                  .HasForeignKey(e => e.FacilityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ProductionMetric
        modelBuilder.Entity<ProductionMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MetricName).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.ProductionOrder)
                  .WithMany(po => po.ProductionMetrics)
                  .HasForeignKey(e => e.ProductionOrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure QualityDocument
        modelBuilder.Entity<QualityDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.QualityInspection)
                  .WithMany(qi => qi.QualityDocuments)
                  .HasForeignKey(e => e.QualityInspectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
} 