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