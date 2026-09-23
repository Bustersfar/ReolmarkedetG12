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

-- Seed: opret de 80 reoler, men kun hvis tabellen er tom (undgår dubletter ved gentagne kørsler)
IF NOT EXISTS (SELECT 1 FROM RACK)
BEGIN
    DECLARE @i INT = 1;
    WHILE @i <= 80
    BEGIN
        INSERT INTO RACK (Number) VALUES (@i);
        SET @i = @i + 1;
    END
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

-- Seed: 10 testlejere + 5 aktive og 5 opsagte lejemål, men kun ved en frisk opsætning (RENTER er tom)
IF NOT EXISTS (SELECT 1 FROM RENTER)
BEGIN
    INSERT INTO RENTER (FirstName, LastName, Address, PostalCode, City, Email, Phone)
    VALUES
    ('Anna', 'Andersen', 'Testvej 2', 1234, 'Testby', 'anna.andersen@test.dk', '20000001'),
    ('Bo', 'Bertelsen', 'Testvej 3', 1234, 'Testby', 'bo.bertelsen@test.dk', '20000002'),
    ('Camilla', 'Christensen', 'Testvej 4', 1234, 'Testby', 'camilla.c@test.dk', '20000003'),
    ('David', 'Dahl', 'Testvej 5', 1234, 'Testby', 'david.dahl@example.dk', '20000004'),
    ('Emma', 'Eriksen', 'Testvej 6', 1234, 'Testby', 'emma.eriksen@test.dk', '20000005'),
    ('Frederik', 'Falk', 'Testvej 7', 1234, 'Testby', 'frederik.falk@test.dk', '20000006'),
    ('Gitte', 'Green', 'Testvej 8', 1234, 'Testby', 'gitte.green@test.dk', '20000007'),
    ('Henrik', 'Holm', 'Testvej 9', 1234, 'Testby', 'henrik.holm@test.dk', '20000008'),
    ('Ida', 'Iversen', 'Testvej 10', 1234, 'Testby', 'ida.iversen@test.dk', '20000009'),
    ('Jonas', 'Juhl', 'Testvej 11', 1234, 'Testby', 'jonas.juhl@test.dk', '20000010');

    -- Reol 1-5: aktivt udlejet (rød), til lejer 1-5
    UPDATE RACK SET Status = 1 WHERE RackId IN (1, 2, 3, 4, 5);

    INSERT INTO RENTAL (RackId, RenterId, StartDate, EndDate, MonthlyRent)
    VALUES
    (1, 1, GETDATE(), NULL, 850.00),
    (2, 2, GETDATE(), NULL, 850.00),
    (3, 3, GETDATE(), NULL, 850.00),
    (4, 4, GETDATE(), NULL, 850.00),
    (5, 5, GETDATE(), NULL, 850.00);

    -- Reol 6-10: under opsigelse (gul), til lejer 6-10
    UPDATE RACK SET Status = 2 WHERE RackId IN (6, 7, 8, 9, 10);

    INSERT INTO RENTAL (RackId, RenterId, StartDate, EndDate, MonthlyRent)
    VALUES
    (6, 6, DATEADD(MONTH, -1, GETDATE()), '2026-11-01', 850.00),
    (7, 7, DATEADD(MONTH, -1, GETDATE()), '2026-11-01', 850.00),
    (8, 8, DATEADD(MONTH, -1, GETDATE()), '2026-11-01', 850.00),
    (9, 9, DATEADD(MONTH, -1, GETDATE()), '2026-11-01', 850.00),
    (10, 10, DATEADD(MONTH, -1, GETDATE()), '2026-11-01', 850.00);
END
GO