-- ============================================================================
-- Reolmarkedet - Database Schema & Seed Data
-- ============================================================================

-- 1. Opret databasen hvis den ikke findes
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Reolmarkedet')
BEGIN
    CREATE DATABASE [Reolmarkedet];
END
GO

USE [Reolmarkedet];
GO

-- 2. Ryd op i eksisterende tabeller (i omvendt rækkefølge af afhængigheder)
IF OBJECT_ID('dbo.SALE_AUDIT_LOG', 'U') IS NOT NULL DROP TABLE dbo.SALE_AUDIT_LOG;
IF OBJECT_ID('dbo.SALE', 'U') IS NOT NULL DROP TABLE dbo.SALE;
IF OBJECT_ID('dbo.PAYMENT', 'U') IS NOT NULL DROP TABLE dbo.PAYMENT;
IF OBJECT_ID('dbo.RENTAL', 'U') IS NOT NULL DROP TABLE dbo.RENTAL;
IF OBJECT_ID('dbo.RENTAL_PRICE_TIER', 'U') IS NOT NULL DROP TABLE dbo.RENTAL_PRICE_TIER;
IF OBJECT_ID('dbo.RACK', 'U') IS NOT NULL DROP TABLE dbo.RACK;
IF OBJECT_ID('dbo.RENTER', 'U') IS NOT NULL DROP TABLE dbo.RENTER;
GO

-- 3. Opret tabeller

-- RENTER (Kunder / Standlejere)
-- Kolonnerne herunder matcher Renter.cs og RenterRepository.cs: FirstName/LastName
-- i stedet for ét samlet Name-felt, og PostalCode som tal (INT), ikke tekst.
-- Telefon og e-mail er valgfrie (NULL), ligesom i modellen (string?).
CREATE TABLE dbo.RENTER (
    RenterId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Address NVARCHAR(200) NOT NULL,
    PostalCode INT NOT NULL,
    City NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- RACK (Reoler / Stande)
CREATE TABLE dbo.RACK (
    RackId INT IDENTITY(1,1) PRIMARY KEY,
    Number INT NOT NULL CONSTRAINT UQ_RACK_Number UNIQUE,
    Status INT NOT NULL DEFAULT 0 -- 0 = Available, 1 = Rented, 2 = UnderTermination
);
GO

-- RENTAL_PRICE_TIER (Prisregler for standleje)
CREATE TABLE dbo.RENTAL_PRICE_TIER (
    TierId INT IDENTITY(1,1) PRIMARY KEY,
    MinRacks INT NOT NULL,
    MaxRacks INT NULL,
    MonthlyPrice DECIMAL(18,2) NOT NULL
);
GO

-- RENTAL (Lejeaftaler)
CREATE TABLE dbo.RENTAL (
    RentalId INT IDENTITY(1,1) PRIMARY KEY,
    RenterId INT NOT NULL,
    RackId INT NOT NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NULL,
    MonthlyRent DECIMAL(18,2) NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_RENTAL_RENTER FOREIGN KEY (RenterId) REFERENCES dbo.RENTER(RenterId),
    CONSTRAINT FK_RENTAL_RACK FOREIGN KEY (RackId) REFERENCES dbo.RACK(RackId)
);
GO

-- PAYMENT (Lejeindbetalinger)
-- Kolonnerne herunder matcher Payment.cs og PaymentRepository.cs: betalingen er
-- knyttet til en RenterId (ikke en RentalId) og har et Date-felt (ikke
-- DueDate/PaidDate/Status, som koden aldrig bruger).
CREATE TABLE dbo.PAYMENT (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    RenterId INT NOT NULL,
    Date DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Amount DECIMAL(18,2) NOT NULL,
    Type INT NOT NULL DEFAULT 0,   -- 0 = FirstMonthPayment, 1 = RegularMonthlyRent
    PaymentMethod INT NOT NULL DEFAULT 0, -- 0 = MobilePay, 1 = Bank, 2 = Cash
    CONSTRAINT FK_PAYMENT_RENTER FOREIGN KEY (RenterId) REFERENCES dbo.RENTER(RenterId)
);
GO

-- SALE (Varesalg og butikssalg)
CREATE TABLE dbo.SALE (
    SaleId INT IDENTITY(1,1) PRIMARY KEY,
    RackId INT NULL,              -- Nullable for at understøtte Reol 0 / Butikssalg
    RenterId INT NULL,            -- Nullable ved butikkens eget salg
    Amount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(200) NOT NULL,
    Date DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    PaymentMethod INT NOT NULL DEFAULT 0, -- 0 = MobilePay, 1 = Bank, 2 = Cash
    CONSTRAINT FK_SALE_RACK FOREIGN KEY (RackId) REFERENCES dbo.RACK(RackId),
    CONSTRAINT FK_SALE_RENTER FOREIGN KEY (RenterId) REFERENCES dbo.RENTER(RenterId)
);
GO

-- SALE_AUDIT_LOG (Historik over administrative rettelser og sletninger i Søg/ret salg)
CREATE TABLE dbo.SALE_AUDIT_LOG (
    AuditId INT IDENTITY(1,1) PRIMARY KEY,
    SaleId INT NOT NULL,
    ActionType NVARCHAR(20) NOT NULL, -- 'UPDATE' eller 'DELETE'
    OldAmount DECIMAL(18,2) NOT NULL,
    NewAmount DECIMAL(18,2) NULL,
    OldDescription NVARCHAR(200) NULL,
    NewDescription NVARCHAR(200) NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- 4. Indekser for hurtige opslag og performance
CREATE NONCLUSTERED INDEX IX_RENTAL_RenterId ON dbo.RENTAL(RenterId);
CREATE NONCLUSTERED INDEX IX_RENTAL_RackId ON dbo.RENTAL(RackId);
CREATE NONCLUSTERED INDEX IX_PAYMENT_RenterId ON dbo.PAYMENT(RenterId);
CREATE NONCLUSTERED INDEX IX_SALE_RackId ON dbo.SALE(RackId);
CREATE NONCLUSTERED INDEX IX_SALE_RenterId ON dbo.SALE(RenterId);
CREATE NONCLUSTERED INDEX IX_SALE_Date ON dbo.SALE(Date);
GO

-- 5. Seed Data

-- Prisregler: 1. reol = 850 kr, 2-3 reoler = 825 kr/stk, 4+ reoler = 800 kr/stk
INSERT INTO dbo.RENTAL_PRICE_TIER (MinRacks, MaxRacks, MonthlyPrice) VALUES
(1, 1, 850.00),
(2, 3, 825.00),
(4, NULL, 800.00);
GO

-- Reol 0 oprettes som intern system-reol til butikssalg (poser, prismærker mv.)
SET IDENTITY_INSERT dbo.RACK ON;
INSERT INTO dbo.RACK (RackId, Number, Status) VALUES (0, 0, 0);
SET IDENTITY_INSERT dbo.RACK OFF;
GO

-- Reoler 1 til 80 (fysiske stande i butikslokalet)
DECLARE @i INT = 1;
WHILE @i <= 80
BEGIN
    INSERT INTO dbo.RACK (Number, Status) VALUES (@i, 0);
    SET @i = @i + 1;
END;
GO