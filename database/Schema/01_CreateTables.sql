-- Precision Manufacturing OMS Database Schema
-- Created for manufacturing operations management
-- Supports multiple facilities, production tracking, quality control

USE [PrecisionManufacturingOMS]
GO

-- Create Facilities table
CREATE TABLE [dbo].[Facilities] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Code] nvarchar(10) NOT NULL,
    [Address] nvarchar(200) NULL,
    [City] nvarchar(50) NULL,
    [State] nvarchar(50) NULL,
    [Country] nvarchar(20) NULL,
    [Type] int NOT NULL, -- FacilityType enum
    [IsActive] bit NOT NULL DEFAULT(1),
    [CreatedDate] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
    CONSTRAINT [PK_Facilities] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_Facilities_Code] UNIQUE NONCLUSTERED ([Code] ASC)
)
GO

-- Create ProductionOrders table
CREATE TABLE [dbo].[ProductionOrders] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [OrderNumber] nvarchar(50) NOT NULL,
    [PartNumber] nvarchar(100) NOT NULL,
    [PartDescription] nvarchar(200) NULL,
    [Quantity] int NOT NULL,
    [QuantityCompleted] int NOT NULL DEFAULT(0),
    [Status] int NOT NULL DEFAULT(1), -- ProductionStatus enum
    [Priority] int NOT NULL DEFAULT(2), -- Priority enum
    [ScheduledStartDate] datetime2 NOT NULL,
    [ScheduledEndDate] datetime2 NOT NULL,
    [ActualStartDate] datetime2 NULL,
    [ActualEndDate] datetime2 NULL,
    [FacilityId] int NOT NULL,
    [CustomerName] nvarchar(100) NOT NULL,
    [CustomerOrderNumber] nvarchar(50) NULL,
    [MaterialType] int NOT NULL, -- MaterialType enum
    [EstimatedCost] decimal(18,2) NOT NULL DEFAULT(0),
    [ActualCost] decimal(18,2) NOT NULL DEFAULT(0),
    [CreatedDate] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
    [Notes] nvarchar(500) NULL,
    CONSTRAINT [PK_ProductionOrders] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_ProductionOrders_OrderNumber] UNIQUE NONCLUSTERED ([OrderNumber] ASC),
    CONSTRAINT [FK_ProductionOrders_Facilities] FOREIGN KEY ([FacilityId]) 
        REFERENCES [dbo].[Facilities] ([Id]) ON DELETE NO ACTION
)
GO

-- Create Equipment table
CREATE TABLE [dbo].[Equipment] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [EquipmentNumber] nvarchar(50) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(200) NULL,
    [Type] int NOT NULL, -- EquipmentType enum
    [Manufacturer] nvarchar(100) NULL,
    [Model] nvarchar(100) NULL,
    [SerialNumber] nvarchar(50) NULL,
    [InstallationDate] datetime2 NOT NULL,
    [LastMaintenanceDate] datetime2 NULL,
    [NextMaintenanceDate] datetime2 NULL,
    [Status] int NOT NULL DEFAULT(1), -- EquipmentStatus enum
    [FacilityId] int NOT NULL,
    [PurchaseCost] decimal(18,2) NOT NULL DEFAULT(0),
    [OperatingHours] decimal(10,2) NOT NULL DEFAULT(0),
    [MaintenanceHours] decimal(10,2) NOT NULL DEFAULT(0),
    [EfficiencyRating] decimal(5,2) NOT NULL DEFAULT(100.0),
    [MaintenanceNotes] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT(1),
    [CreatedDate] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
    CONSTRAINT [PK_Equipment] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_Equipment_EquipmentNumber] UNIQUE NONCLUSTERED ([EquipmentNumber] ASC),
    CONSTRAINT [FK_Equipment_Facilities] FOREIGN KEY ([FacilityId]) 
        REFERENCES [dbo].[Facilities] ([Id]) ON DELETE NO ACTION
)
GO

-- Create QualityInspections table
CREATE TABLE [dbo].[QualityInspections] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [InspectionNumber] nvarchar(50) NOT NULL,
    [ProductionOrderId] int NOT NULL,
    [FacilityId] int NOT NULL,
    [Type] int NOT NULL, -- InspectionType enum
    [Status] int NOT NULL DEFAULT(1), -- InspectionStatus enum
    [ScheduledDate] datetime2 NOT NULL,
    [ActualDate] datetime2 NULL,
    [InspectorName] nvarchar(100) NOT NULL,
    [InspectorBadge] nvarchar(50) NULL,
    [Passed] bit NOT NULL DEFAULT(0),
    [Results] nvarchar(1000) NULL,
    [NonConformanceNotes] nvarchar(500) NULL,
    [CorrectiveActions] nvarchar(500) NULL,
    [DimensionTolerance] decimal(10,4) NULL,
    [MeasuredDimension] decimal(10,4) NULL,
    [MeasurementUnit] nvarchar(100) NULL,
    [DefectCount] int NULL,
    [CertificationRequired] nvarchar(200) NULL,
    [CertificationCompleted] bit NOT NULL DEFAULT(0),
    [CreatedDate] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
    [CompletedDate] datetime2 NULL,
    CONSTRAINT [PK_QualityInspections] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UK_QualityInspections_InspectionNumber] UNIQUE NONCLUSTERED ([InspectionNumber] ASC),
    CONSTRAINT [FK_QualityInspections_ProductionOrders] FOREIGN KEY ([ProductionOrderId]) 
        REFERENCES [dbo].[ProductionOrders] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_QualityInspections_Facilities] FOREIGN KEY ([FacilityId]) 
        REFERENCES [dbo].[Facilities] ([Id]) ON DELETE NO ACTION
)
GO

-- Create ProductionMetrics table
CREATE TABLE [dbo].[ProductionMetrics] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ProductionOrderId] int NOT NULL,
    [Type] int NOT NULL, -- MetricType enum
    [MetricName] nvarchar(100) NOT NULL,
    [Value] decimal(18,4) NOT NULL,
    [Unit] nvarchar(50) NULL,
    [TargetValue] decimal(18,4) NULL,
    [MinValue] decimal(18,4) NULL,
    [MaxValue] decimal(18,4) NULL,
    [IsWithinTolerance] bit NOT NULL DEFAULT(1),
    [MeasuredDate] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
    [MeasuredBy] nvarchar(100) NULL,
    [Notes] nvarchar(500) NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
    CONSTRAINT [PK_ProductionMetrics] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ProductionMetrics_ProductionOrders] FOREIGN KEY ([ProductionOrderId]) 
        REFERENCES [dbo].[ProductionOrders] ([Id]) ON DELETE CASCADE
)
GO

-- Create QualityDocuments table
CREATE TABLE [dbo].[QualityDocuments] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [QualityInspectionId] int NOT NULL,
    [DocumentName] nvarchar(200) NOT NULL,
    [Type] int NOT NULL, -- DocumentType enum
    [FilePath] nvarchar(500) NULL,
    [FileExtension] nvarchar(100) NULL,
    [FileSize] bigint NOT NULL DEFAULT(0),
    [Description] nvarchar(1000) NULL,
    [CreatedBy] nvarchar(100) NOT NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT(GETUTCDATE()),
    [ApprovedBy] nvarchar(100) NULL,
    [ApprovedDate] datetime2 NULL,
    [IsApproved] bit NOT NULL DEFAULT(0),
    [Version] nvarchar(50) NOT NULL DEFAULT('1.0'),
    [IsActive] bit NOT NULL DEFAULT(1),
    CONSTRAINT [PK_QualityDocuments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_QualityDocuments_QualityInspections] FOREIGN KEY ([QualityInspectionId]) 
        REFERENCES [dbo].[QualityInspections] ([Id]) ON DELETE CASCADE
)
GO

-- Create indexes for performance optimization
CREATE NONCLUSTERED INDEX [IX_ProductionOrders_FacilityId] ON [dbo].[ProductionOrders] ([FacilityId])
GO

CREATE NONCLUSTERED INDEX [IX_ProductionOrders_Status] ON [dbo].[ProductionOrders] ([Status])
GO

CREATE NONCLUSTERED INDEX [IX_ProductionOrders_ScheduledDates] ON [dbo].[ProductionOrders] ([ScheduledStartDate], [ScheduledEndDate])
GO

CREATE NONCLUSTERED INDEX [IX_QualityInspections_ProductionOrderId] ON [dbo].[QualityInspections] ([ProductionOrderId])
GO

CREATE NONCLUSTERED INDEX [IX_QualityInspections_FacilityId] ON [dbo].[QualityInspections] ([FacilityId])
GO

CREATE NONCLUSTERED INDEX [IX_Equipment_FacilityId] ON [dbo].[Equipment] ([FacilityId])
GO

CREATE NONCLUSTERED INDEX [IX_ProductionMetrics_ProductionOrderId] ON [dbo].[ProductionMetrics] ([ProductionOrderId])
GO

PRINT 'Database schema created successfully'
GO 