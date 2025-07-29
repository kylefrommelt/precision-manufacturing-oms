-- Sample Data for Precision Manufacturing OMS
-- Demonstrates realistic aerospace manufacturing scenarios

USE [PrecisionManufacturingOMS]
GO

-- Insert Facilities (Simulating PCC locations)
INSERT INTO [dbo].[Facilities] ([Name], [Code], [Address], [City], [State], [Country], [Type], [IsActive])
VALUES 
    ('Minerva Airfoils Facility', 'MIN', '13300 Shaw Ave', 'Minerva', 'Ohio', 'USA', 2, 1),
    ('Crooksville Investment Casting', 'CRK', '3455 Ceramic Road', 'Crooksville', 'Ohio', 'USA', 1, 1),
    ('Merida Forging Operations', 'MER', 'Industrial Park 123', 'Merida', 'Yucatan', 'Mexico', 3, 1),
    ('Quality Control Center', 'QCC', '13300 Shaw Ave Bldg B', 'Minerva', 'Ohio', 'USA', 6, 1);

-- Insert Equipment
INSERT INTO [dbo].[Equipment] ([EquipmentNumber], [Name], [Type], [FacilityId], [Manufacturer], [Model], [SerialNumber], [InstallationDate], [OperatingHours], [EfficiencyRating], [PurchaseCost], [IsActive])
VALUES 
    ('MIN-CF-001', 'Primary Casting Furnace', 1, 1, 'Inductotherm', 'VIP-2000', 'ICF2000-001', '2020-01-15', 8760.5, 94.2, 850000.00, 1),
    ('MIN-WIM-001', 'Wax Injection Machine Alpha', 2, 1, 'Freeman Manufacturing', 'WIM-500', 'WIM500-A1', '2019-08-20', 7200.3, 96.8, 125000.00, 1),
    ('MIN-DR-001', 'Dipping Robot Line 1', 3, 1, 'ABB Robotics', 'IRB-4600', 'IRB4600-L1', '2021-03-10', 6840.2, 91.5, 180000.00, 1),
    ('CRK-AC-001', 'AutoClave Unit 1', 4, 2, 'ASC Process Systems', 'AC-2000', 'AC2000-U1', '2018-11-05', 9240.8, 89.3, 450000.00, 1),
    ('MER-CNC-001', 'CNC Machining Center', 6, 3, 'Haas Automation', 'VF-6/50', 'VF650-MC1', '2022-01-12', 5280.7, 97.1, 320000.00, 1),
    ('QCC-CMM-001', 'Coordinate Measuring Machine', 13, 4, 'Carl Zeiss', 'CONTURA G2', 'CG2-CMM1', '2021-06-18', 4380.5, 95.8, 285000.00, 1);

-- Insert Production Orders for Aerospace Components
INSERT INTO [dbo].[ProductionOrders] ([OrderNumber], [PartNumber], [PartDescription], [Quantity], [QuantityCompleted], [Status], [Priority], [ScheduledStartDate], [ScheduledEndDate], [ActualStartDate], [ActualEndDate], [FacilityId], [CustomerName], [CustomerOrderNumber], [MaterialType], [EstimatedCost], [ActualCost])
VALUES 
    ('MIN-20241101-001', 'GE-T700-BLADE-001', 'Gas Turbine Blade - T700 Engine', 25, 25, 5, 3, '2024-11-01 08:00:00', '2024-11-15 17:00:00', '2024-11-01 08:15:00', '2024-11-14 16:30:00', 1, 'General Electric', 'GE-PO-2024-0847', 1, 125000.00, 118500.00),
    ('MIN-20241105-002', 'PW-F119-VANE-002', 'Stator Vane Assembly - F119 Engine', 50, 48, 5, 3, '2024-11-05 07:00:00', '2024-11-20 18:00:00', '2024-11-05 07:30:00', '2024-11-19 17:45:00', 1, 'Pratt & Whitney', 'PW-SO-2024-1024', 6, 275000.00, 285400.00),
    ('CRK-20241110-001', 'CFM-LEAP-CASE-001', 'Turbine Case - LEAP Engine', 12, 10, 3, 4, '2024-11-10 06:00:00', '2024-12-01 16:00:00', '2024-11-10 06:15:00', NULL, 2, 'Safran Aircraft Engines', 'SAE-WO-2024-2156', 2, 180000.00, 165000.00),
    ('MER-20241112-001', 'RR-TRENT-DISK-001', 'Compressor Disk - Trent 1000', 8, 0, 2, 2, '2024-11-12 05:00:00', '2024-12-10 15:00:00', NULL, NULL, 3, 'Rolls-Royce', 'RR-FO-2024-3890', 3, 95000.00, 0.00),
    ('MIN-20241108-003', 'GE-GE9X-STRUT-001', 'Engine Strut Component - GE9X', 15, 12, 3, 3, '2024-11-08 09:00:00', '2024-11-25 17:00:00', '2024-11-08 09:20:00', NULL, 1, 'General Electric', 'GE-PO-2024-0963', 5, 85000.00, 78000.00),
    ('CRK-20241101-002', 'BA-787-BRACKET-001', 'Wing Bracket Assembly - 787', 35, 35, 5, 2, '2024-11-01 07:30:00', '2024-11-18 16:00:00', '2024-11-01 08:00:00', '2024-11-17 15:30:00', 2, 'Boeing', 'BA-CO-2024-5647', 4, 156000.00, 149800.00);

-- Insert Quality Inspections
INSERT INTO [dbo].[QualityInspections] ([InspectionNumber], [ProductionOrderId], [FacilityId], [Type], [Status], [ScheduledDate], [ActualDate], [InspectorName], [InspectorBadge], [Passed], [Results], [DimensionTolerance], [MeasuredDimension], [MeasurementUnit], [DefectCount], [CertificationRequired], [CertificationCompleted], [CompletedDate])
VALUES 
    ('QI-MIN-241115-001', 1, 4, 3, 7, '2024-11-15 10:00:00', '2024-11-15 10:30:00', 'Sarah Johnson', 'QC-001', 1, 'All dimensional requirements met. Surface finish excellent.', 0.005, 0.003, 'inches', 0, 'AS9100 Rev D', 1, '2024-11-15 14:00:00'),
    ('QI-MIN-241119-002', 2, 4, 3, 7, '2024-11-19 14:00:00', '2024-11-19 14:15:00', 'Michael Chen', 'QC-002', 1, 'Dimensional inspection passed. Minor surface defects noted but within spec.', 0.002, 0.0015, 'inches', 2, 'NADCAP Approval', 1, '2024-11-19 16:30:00'),
    ('QI-CRK-241125-001', 3, 4, 2, 3, '2024-11-25 11:00:00', '2024-11-25 11:45:00', 'Robert Williams', 'QC-003', 0, 'In-process inspection revealed porosity issues. Rework required.', 0.001, 0.0025, 'inches', 5, 'AMS Specification', 0, '2024-11-25 15:00:00'),
    ('QI-MIN-241120-003', 5, 4, 2, 2, '2024-11-20 13:00:00', '2024-11-20 13:30:00', 'Lisa Anderson', 'QC-004', 1, 'In-process dimensional check. All measurements within tolerance.', 0.003, 0.002, 'inches', 0, 'Customer Drawing Rev C', 0, NULL),
    ('QI-CRK-241117-002', 6, 4, 3, 7, '2024-11-17 16:00:00', '2024-11-17 16:20:00', 'David Martinez', 'QC-005', 1, 'Final inspection completed successfully. Ready for delivery.', 0.004, 0.0035, 'inches', 1, 'FAA PMA Approval', 1, '2024-11-17 17:00:00');

-- Insert Production Metrics
INSERT INTO [dbo].[ProductionMetrics] ([ProductionOrderId], [Type], [MetricName], [Value], [Unit], [TargetValue], [IsWithinTolerance], [MeasuredDate], [MeasuredBy])
VALUES 
    (1, 1, 'Cycle Time per Unit', 13.2, 'hours', 14.0, 1, '2024-11-14 16:30:00', 'Production Supervisor'),
    (1, 3, 'Quality Pass Rate', 100.0, 'percent', 98.0, 1, '2024-11-14 16:30:00', 'Quality Manager'),
    (1, 6, 'Equipment Efficiency', 94.2, 'percent', 90.0, 1, '2024-11-14 16:30:00', 'Maintenance Tech'),
    (2, 1, 'Cycle Time per Unit', 11.8, 'hours', 12.0, 1, '2024-11-19 17:45:00', 'Production Supervisor'),
    (2, 4, 'Scrap Rate', 4.0, 'percent', 5.0, 1, '2024-11-19 17:45:00', 'Quality Inspector'),
    (3, 1, 'Cycle Time per Unit', 15.6, 'hours', 15.0, 0, '2024-11-25 12:00:00', 'Production Supervisor'),
    (3, 14, 'Yield Rate', 83.3, 'percent', 95.0, 0, '2024-11-25 12:00:00', 'Process Engineer'),
    (6, 1, 'Cycle Time per Unit', 9.2, 'hours', 10.0, 1, '2024-11-17 15:30:00', 'Production Supervisor'),
    (6, 3, 'Quality Pass Rate', 97.1, 'percent', 98.0, 0, '2024-11-17 15:30:00', 'Quality Manager');

-- Insert Quality Documents
INSERT INTO [dbo].[QualityDocuments] ([QualityInspectionId], [DocumentName], [Type], [Description], [CreatedBy], [ApprovedBy], [IsApproved], [Version])
VALUES 
    (1, 'GE-T700-BLADE-001_Final_Inspection_Report.pdf', 1, 'Final dimensional and visual inspection report for GE T700 turbine blades', 'Sarah Johnson', 'Quality Manager', 1, '1.0'),
    (1, 'AS9100_Certification_GE-T700-BLADE-001.pdf', 2, 'AS9100 Rev D certification document for T700 blade production', 'Quality Manager', 'Plant Manager', 1, '1.0'),
    (2, 'PW-F119-VANE-002_Inspection_Report.pdf', 1, 'Final inspection documentation for F119 stator vane assembly', 'Michael Chen', 'Quality Manager', 1, '1.0'),
    (3, 'CFM-LEAP-CASE-001_NCR_20241125.pdf', 6, 'Non-conformance report for porosity issues in LEAP turbine case', 'Robert Williams', 'Quality Manager', 1, '1.0'),
    (5, 'BA-787-BRACKET-001_Final_Report.pdf', 1, 'Final inspection and certification report for Boeing 787 wing bracket', 'David Martinez', 'Quality Manager', 1, '1.0');

-- Update Equipment maintenance schedules
UPDATE [dbo].[Equipment] 
SET [LastMaintenanceDate] = '2024-10-15 10:00:00', 
    [NextMaintenanceDate] = '2024-12-15 10:00:00',
    [MaintenanceHours] = 45.5
WHERE [EquipmentNumber] = 'MIN-CF-001';

UPDATE [dbo].[Equipment] 
SET [LastMaintenanceDate] = '2024-11-01 08:00:00', 
    [NextMaintenanceDate] = '2025-01-01 08:00:00',
    [MaintenanceHours] = 28.3
WHERE [EquipmentNumber] = 'MIN-WIM-001';

UPDATE [dbo].[Equipment] 
SET [LastMaintenanceDate] = '2024-09-20 14:00:00', 
    [NextMaintenanceDate] = '2024-11-20 14:00:00',
    [MaintenanceHours] = 36.7
WHERE [EquipmentNumber] = 'CRK-AC-001';

PRINT 'Sample data inserted successfully'
PRINT 'Data includes:'
PRINT '- 4 Manufacturing Facilities (Minerva, Crooksville, Merida, Quality Control)'
PRINT '- 6 Equipment items across different facility types'
PRINT '- 6 Production Orders for aerospace components (GE, Pratt & Whitney, Safran, Rolls-Royce, Boeing)'
PRINT '- 5 Quality Inspections with various statuses'
PRINT '- 9 Production Metrics tracking efficiency and quality'
PRINT '- 5 Quality Documents for certifications and reports'
GO 