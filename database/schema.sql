-- Run this script once against your local SQL Server instance to create the
-- database and tables this project needs. Matches the model classes in
-- ReolmarkedetG12.Core.Models.

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'Reolmarkedet')
BEGIN
    CREATE DATABASE Reolmarkedet;
END
GO

USE Reolmarkedet;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RACK')
BEGIN
    CREATE TABLE RACK
    (
        RackId INT IDENTITY(1,1) PRIMARY KEY,
        Number INT NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RACK') AND name = 'Type')
BEGIN
    ALTER TABLE RACK ADD Type INT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RACK') AND name = 'Status')
BEGIN
    ALTER TABLE RACK ADD Status INT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RENTAL_PRICE_TIER')
BEGIN
    CREATE TABLE RENTAL_PRICE_TIER
    (
        TierId       INT IDENTITY(1,1) PRIMARY KEY,
        MinRacks     INT NOT NULL,
        MaxRacks     INT NULL,
        PricePerRack DECIMAL(10,2) NOT NULL
    );

    INSERT INTO RENTAL_PRICE_TIER (MinRacks, MaxRacks, PricePerRack) VALUES
    (1, 1, 850.00),
    (2, 3, 825.00),
    (4, NULL, 800.00);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RENTER')
BEGIN
    CREATE TABLE RENTER
    (
        RenterId   INT IDENTITY(1,1) PRIMARY KEY,
        FirstName  NVARCHAR(100) NOT NULL,
        LastName   NVARCHAR(100) NOT NULL,
        Address    NVARCHAR(200) NOT NULL,
        PostalCode INT NOT NULL,
        City       NVARCHAR(100) NOT NULL,
        Email      NVARCHAR(200) NULL,
        Phone      NVARCHAR(50) NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RENTAL')
BEGIN
    CREATE TABLE RENTAL
    (
        RentalId  INT IDENTITY(1,1) PRIMARY KEY,
        RackId    INT NOT NULL,
        RenterId  INT NOT NULL,
        StartDate DATETIME NOT NULL,
        EndDate   DATETIME NULL,
        CONSTRAINT FK_Rental_Rack FOREIGN KEY (RackId) REFERENCES RACK(RackId),
        CONSTRAINT FK_Rental_Renter FOREIGN KEY (RenterId) REFERENCES RENTER(RenterId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RENTAL') AND name = 'MonthlyRent')
BEGIN
    ALTER TABLE RENTAL ADD MonthlyRent DECIMAL(10,2) NOT NULL DEFAULT 0;
END
GO