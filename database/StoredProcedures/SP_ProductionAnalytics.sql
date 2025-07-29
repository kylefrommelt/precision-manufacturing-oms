-- Advanced Production Analytics Stored Procedures
-- Demonstrates complex T-SQL, CTEs, window functions, and optimization

USE [PrecisionManufacturingOMS]
GO

-- =============================================
-- Production Performance Analytics
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetProductionPerformanceAnalytics]
    @FacilityId INT = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @MaterialType INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Set default date range if not provided
    IF @StartDate IS NULL SET @StartDate = DATEADD(MONTH, -3, GETUTCDATE())
    IF @EndDate IS NULL SET @EndDate = GETUTCDATE()
    
    -- Main analytics query using CTEs and window functions
    WITH ProductionData AS (
        SELECT 
            po.Id,
            po.OrderNumber,
            po.PartNumber,
            po.Quantity,
            po.QuantityCompleted,
            po.Status,
            po.Priority,
            po.FacilityId,
            f.Name AS FacilityName,
            f.Code AS FacilityCode,
            po.MaterialType,
            po.EstimatedCost,
            po.ActualCost,
            po.ScheduledStartDate,
            po.ScheduledEndDate,
            po.ActualStartDate,
            po.ActualEndDate,
            
            -- Calculate efficiency metrics
            CASE 
                WHEN po.ActualEndDate IS NOT NULL AND po.ActualStartDate IS NOT NULL
                THEN DATEDIFF(HOUR, po.ActualStartDate, po.ActualEndDate)
                ELSE NULL
            END AS ActualDurationHours,
            
            CASE 
                WHEN po.ScheduledEndDate IS NOT NULL AND po.ScheduledStartDate IS NOT NULL
                THEN DATEDIFF(HOUR, po.ScheduledStartDate, po.ScheduledEndDate)
                ELSE NULL
            END AS ScheduledDurationHours,
            
            -- Cost variance
            (po.ActualCost - po.EstimatedCost) AS CostVariance,
            
            -- Quality rating
            CASE 
                WHEN po.Quantity > 0 
                THEN CAST(po.QuantityCompleted AS DECIMAL(10,2)) / po.Quantity * 100
                ELSE 0
            END AS QualityCompletionRate,
            
            -- On-time delivery indicator
            CASE 
                WHEN po.Status = 5 AND po.ActualEndDate <= po.ScheduledEndDate THEN 1
                WHEN po.Status = 5 AND po.ActualEndDate > po.ScheduledEndDate THEN 0
                ELSE NULL
            END AS OnTimeDelivery
        FROM ProductionOrders po
        INNER JOIN Facilities f ON po.FacilityId = f.Id
        WHERE (@FacilityId IS NULL OR po.FacilityId = @FacilityId)
          AND po.CreatedDate >= @StartDate
          AND po.CreatedDate <= @EndDate
          AND (@MaterialType IS NULL OR po.MaterialType = @MaterialType)
    ),
    
    PerformanceMetrics AS (
        SELECT 
            FacilityId,
            FacilityName,
            FacilityCode,
            
            -- Order counts
            COUNT(*) AS TotalOrders,
            SUM(CASE WHEN Status = 5 THEN 1 ELSE 0 END) AS CompletedOrders,
            SUM(CASE WHEN Status = 6 THEN 1 ELSE 0 END) AS CancelledOrders,
            SUM(CASE WHEN OnTimeDelivery = 1 THEN 1 ELSE 0 END) AS OnTimeOrders,
            
            -- Financial metrics
            SUM(EstimatedCost) AS TotalEstimatedCost,
            SUM(ActualCost) AS TotalActualCost,
            SUM(CostVariance) AS TotalCostVariance,
            AVG(CostVariance) AS AvgCostVariance,
            
            -- Time metrics
            AVG(CAST(ActualDurationHours AS DECIMAL(10,2))) AS AvgActualDurationHours,
            AVG(CAST(ScheduledDurationHours AS DECIMAL(10,2))) AS AvgScheduledDurationHours,
            
            -- Quality metrics
            AVG(QualityCompletionRate) AS AvgQualityCompletionRate,
            SUM(Quantity) AS TotalQuantityOrdered,
            SUM(QuantityCompleted) AS TotalQuantityCompleted,
            
            -- Efficiency calculations
            CASE 
                WHEN SUM(CASE WHEN OnTimeDelivery IS NOT NULL THEN 1 ELSE 0 END) > 0
                THEN CAST(SUM(CASE WHEN OnTimeDelivery = 1 THEN 1 ELSE 0 END) AS DECIMAL(10,2)) / 
                     SUM(CASE WHEN OnTimeDelivery IS NOT NULL THEN 1 ELSE 0 END) * 100
                ELSE 0
            END AS OnTimeDeliveryRate,
            
            CASE 
                WHEN AVG(CAST(ScheduledDurationHours AS DECIMAL(10,2))) > 0
                THEN AVG(CAST(ActualDurationHours AS DECIMAL(10,2))) / 
                     AVG(CAST(ScheduledDurationHours AS DECIMAL(10,2))) * 100
                ELSE 0
            END AS TimeEfficiencyRate
            
        FROM ProductionData
        GROUP BY FacilityId, FacilityName, FacilityCode
    ),
    
    TrendAnalysis AS (
        SELECT 
            FacilityId,
            YEAR(ScheduledStartDate) AS OrderYear,
            MONTH(ScheduledStartDate) AS OrderMonth,
            COUNT(*) AS MonthlyOrderCount,
            SUM(ActualCost) AS MonthlyActualCost,
            AVG(CASE 
                WHEN ActualDurationHours > 0 AND ScheduledDurationHours > 0
                THEN CAST(ActualDurationHours AS DECIMAL(10,2)) / ScheduledDurationHours * 100
                ELSE NULL
            END) AS MonthlyEfficiency,
            
            -- Calculate month-over-month trends
            LAG(COUNT(*)) OVER (PARTITION BY FacilityId ORDER BY YEAR(ScheduledStartDate), MONTH(ScheduledStartDate)) AS PrevMonthOrderCount,
            LAG(SUM(ActualCost)) OVER (PARTITION BY FacilityId ORDER BY YEAR(ScheduledStartDate), MONTH(ScheduledStartDate)) AS PrevMonthCost
            
        FROM ProductionData
        WHERE ScheduledStartDate IS NOT NULL
        GROUP BY FacilityId, YEAR(ScheduledStartDate), MONTH(ScheduledStartDate)
    )
    
    -- Main result set
    SELECT 
        pm.*,
        
        -- Add trend indicators
        CASE 
            WHEN pm.TotalOrders > 0 THEN 'Active'
            ELSE 'Inactive'
        END AS FacilityStatus,
        
        -- Performance rating
        CASE 
            WHEN pm.OnTimeDeliveryRate >= 95 AND pm.TimeEfficiencyRate >= 95 THEN 'Excellent'
            WHEN pm.OnTimeDeliveryRate >= 85 AND pm.TimeEfficiencyRate >= 85 THEN 'Good'
            WHEN pm.OnTimeDeliveryRate >= 75 AND pm.TimeEfficiencyRate >= 75 THEN 'Average'
            ELSE 'Needs Improvement'
        END AS PerformanceRating,
        
        -- Cost efficiency
        CASE 
            WHEN pm.TotalEstimatedCost > 0 
            THEN (pm.TotalActualCost / pm.TotalEstimatedCost) * 100
            ELSE 0
        END AS CostEfficiencyRate
        
    FROM PerformanceMetrics pm
    ORDER BY pm.OnTimeDeliveryRate DESC, pm.TimeEfficiencyRate DESC;
    
    -- Return trend analysis as second result set
    SELECT 
        ta.FacilityId,
        ta.OrderYear,
        ta.OrderMonth,
        ta.MonthlyOrderCount,
        ta.MonthlyActualCost,
        ta.MonthlyEfficiency,
        
        -- Calculate growth rates
        CASE 
            WHEN ta.PrevMonthOrderCount > 0 
            THEN ((CAST(ta.MonthlyOrderCount AS DECIMAL(10,2)) - ta.PrevMonthOrderCount) / ta.PrevMonthOrderCount) * 100
            ELSE NULL
        END AS OrderGrowthRate,
        
        CASE 
            WHEN ta.PrevMonthCost > 0 
            THEN ((ta.MonthlyActualCost - ta.PrevMonthCost) / ta.PrevMonthCost) * 100
            ELSE NULL
        END AS CostGrowthRate
        
    FROM TrendAnalysis ta
    ORDER BY ta.FacilityId, ta.OrderYear DESC, ta.OrderMonth DESC;
    
END
GO

-- =============================================
-- Quality Control Analytics
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetQualityControlAnalytics]
    @FacilityId INT = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL,
    @InspectionType INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Set default date range if not provided
    IF @StartDate IS NULL SET @StartDate = DATEADD(MONTH, -6, GETUTCDATE())
    IF @EndDate IS NULL SET @EndDate = GETUTCDATE()
    
    WITH QualityData AS (
        SELECT 
            qi.Id,
            qi.InspectionNumber,
            qi.ProductionOrderId,
            qi.FacilityId,
            f.Name AS FacilityName,
            qi.Type AS InspectionType,
            qi.Status,
            qi.Passed,
            qi.DefectCount,
            qi.ScheduledDate,
            qi.ActualDate,
            qi.CompletedDate,
            po.PartNumber,
            po.MaterialType,
            po.CustomerName,
            
            -- Calculate inspection duration
            CASE 
                WHEN qi.CompletedDate IS NOT NULL AND qi.ActualDate IS NOT NULL
                THEN DATEDIFF(HOUR, qi.ActualDate, qi.CompletedDate)
                ELSE NULL
            END AS InspectionDurationHours,
            
            -- Calculate schedule adherence
            CASE 
                WHEN qi.ActualDate <= qi.ScheduledDate THEN 1
                ELSE 0
            END AS OnSchedule,
            
            -- Defect rate calculation
            CASE 
                WHEN qi.DefectCount IS NOT NULL AND po.Quantity > 0
                THEN CAST(qi.DefectCount AS DECIMAL(10,4)) / po.Quantity * 100
                ELSE 0
            END AS DefectRate
            
        FROM QualityInspections qi
        INNER JOIN Facilities f ON qi.FacilityId = f.Id
        INNER JOIN ProductionOrders po ON qi.ProductionOrderId = po.Id
        WHERE (@FacilityId IS NULL OR qi.FacilityId = @FacilityId)
          AND qi.CreatedDate >= @StartDate
          AND qi.CreatedDate <= @EndDate
          AND (@InspectionType IS NULL OR qi.Type = @InspectionType)
    ),
    
    QualitySummary AS (
        SELECT 
            FacilityId,
            FacilityName,
            
            -- Inspection counts
            COUNT(*) AS TotalInspections,
            SUM(CASE WHEN Status = 7 THEN 1 ELSE 0 END) AS ApprovedInspections,
            SUM(CASE WHEN Status = 5 THEN 1 ELSE 0 END) AS FailedInspections,
            SUM(CASE WHEN Passed = 1 THEN 1 ELSE 0 END) AS PassedInspections,
            SUM(CASE WHEN OnSchedule = 1 THEN 1 ELSE 0 END) AS OnScheduleInspections,
            
            -- Quality metrics
            AVG(DefectRate) AS AvgDefectRate,
            SUM(ISNULL(DefectCount, 0)) AS TotalDefects,
            AVG(CAST(InspectionDurationHours AS DECIMAL(10,2))) AS AvgInspectionDuration,
            
            -- Calculate pass rates
            CASE 
                WHEN COUNT(*) > 0 
                THEN CAST(SUM(CASE WHEN Passed = 1 THEN 1 ELSE 0 END) AS DECIMAL(10,2)) / COUNT(*) * 100
                ELSE 0
            END AS PassRate,
            
            -- Schedule adherence rate
            CASE 
                WHEN COUNT(*) > 0 
                THEN CAST(SUM(CASE WHEN OnSchedule = 1 THEN 1 ELSE 0 END) AS DECIMAL(10,2)) / COUNT(*) * 100
                ELSE 0
            END AS ScheduleAdherenceRate
            
        FROM QualityData
        GROUP BY FacilityId, FacilityName
    )
    
    -- Main quality summary
    SELECT 
        qs.*,
        
        -- Quality rating
        CASE 
            WHEN qs.PassRate >= 98 AND qs.AvgDefectRate <= 1 THEN 'Excellent'
            WHEN qs.PassRate >= 95 AND qs.AvgDefectRate <= 2 THEN 'Good'
            WHEN qs.PassRate >= 90 AND qs.AvgDefectRate <= 5 THEN 'Average'
            ELSE 'Needs Improvement'
        END AS QualityRating
        
    FROM QualitySummary qs
    ORDER BY qs.PassRate DESC, qs.ScheduleAdherenceRate DESC;
    
    -- Defect analysis by inspection type
    SELECT 
        InspectionType,
        COUNT(*) AS InspectionCount,
        SUM(ISNULL(DefectCount, 0)) AS TotalDefects,
        AVG(DefectRate) AS AvgDefectRate,
        CASE 
            WHEN COUNT(*) > 0 
            THEN CAST(SUM(CASE WHEN Passed = 1 THEN 1 ELSE 0 END) AS DECIMAL(10,2)) / COUNT(*) * 100
            ELSE 0
        END AS PassRateByType
    FROM QualityData
    GROUP BY InspectionType
    ORDER BY AvgDefectRate DESC;
    
END
GO

-- =============================================
-- Equipment Utilization and Maintenance Analytics
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_GetEquipmentAnalytics]
    @FacilityId INT = NULL,
    @EquipmentType INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    WITH EquipmentData AS (
        SELECT 
            e.Id,
            e.EquipmentNumber,
            e.Name,
            e.Type,
            e.Status,
            e.FacilityId,
            f.Name AS FacilityName,
            e.OperatingHours,
            e.MaintenanceHours,
            e.EfficiencyRating,
            e.PurchaseCost,
            e.InstallationDate,
            e.LastMaintenanceDate,
            e.NextMaintenanceDate,
            
            -- Calculate equipment age
            DATEDIFF(DAY, e.InstallationDate, GETUTCDATE()) AS EquipmentAgeDays,
            
            -- Maintenance metrics
            CASE 
                WHEN e.OperatingHours > 0 
                THEN (e.MaintenanceHours / e.OperatingHours) * 100
                ELSE 0
            END AS MaintenanceRatio,
            
            -- Determine maintenance status
            CASE 
                WHEN e.NextMaintenanceDate IS NULL THEN 'Not Scheduled'
                WHEN e.NextMaintenanceDate < GETUTCDATE() THEN 'Overdue'
                WHEN e.NextMaintenanceDate <= DATEADD(DAY, 7, GETUTCDATE()) THEN 'Due Soon'
                ELSE 'Scheduled'
            END AS MaintenanceStatus,
            
            -- Calculate utilization rate (assuming 8760 hours per year)
            CASE 
                WHEN DATEDIFF(DAY, e.InstallationDate, GETUTCDATE()) > 0
                THEN (e.OperatingHours / (DATEDIFF(DAY, e.InstallationDate, GETUTCDATE()) * 24.0)) * 100
                ELSE 0
            END AS UtilizationRate
            
        FROM Equipment e
        INNER JOIN Facilities f ON e.FacilityId = f.Id
        WHERE e.IsActive = 1
          AND (@FacilityId IS NULL OR e.FacilityId = @FacilityId)
          AND (@EquipmentType IS NULL OR e.Type = @EquipmentType)
    )
    
    -- Equipment summary by facility
    SELECT 
        FacilityId,
        FacilityName,
        COUNT(*) AS TotalEquipment,
        SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) AS AvailableEquipment,
        SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END) AS InUseEquipment,
        SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END) AS MaintenanceEquipment,
        SUM(CASE WHEN Status = 4 THEN 1 ELSE 0 END) AS BrokenEquipment,
        
        -- Performance metrics
        AVG(EfficiencyRating) AS AvgEfficiencyRating,
        AVG(UtilizationRate) AS AvgUtilizationRate,
        AVG(MaintenanceRatio) AS AvgMaintenanceRatio,
        SUM(PurchaseCost) AS TotalEquipmentValue,
        
        -- Maintenance alerts
        SUM(CASE WHEN MaintenanceStatus = 'Overdue' THEN 1 ELSE 0 END) AS OverdueMaintenanceCount,
        SUM(CASE WHEN MaintenanceStatus = 'Due Soon' THEN 1 ELSE 0 END) AS DueSoonMaintenanceCount
        
    FROM EquipmentData
    GROUP BY FacilityId, FacilityName
    ORDER BY AvgEfficiencyRating DESC;
    
    -- Equipment requiring attention
    SELECT 
        EquipmentNumber,
        Name,
        FacilityName,
        Status,
        EfficiencyRating,
        UtilizationRate,
        MaintenanceStatus,
        NextMaintenanceDate,
        EquipmentAgeDays,
        
        -- Priority score for maintenance (higher = more urgent)
        CASE 
            WHEN MaintenanceStatus = 'Overdue' THEN 100
            WHEN MaintenanceStatus = 'Due Soon' THEN 75
            WHEN EfficiencyRating < 80 THEN 50
            WHEN UtilizationRate > 90 THEN 40
            ELSE 10
        END AS MaintenancePriority
        
    FROM EquipmentData
    WHERE MaintenanceStatus IN ('Overdue', 'Due Soon') 
       OR EfficiencyRating < 85 
       OR Status = 4
    ORDER BY MaintenancePriority DESC, EfficiencyRating ASC;
    
END
GO

PRINT 'Production analytics stored procedures created successfully'
GO 