-- ============================================================================
-- Reolmarkedet - Testdata
-- ============================================================================
-- Indsætter 10 lejere, en række lejeaftaler og salg for de to foregående måneder.
-- Køres EFTER schema.sql på en database uden lejere. Datoerne beregnes ud fra dags
-- dato, så salgene altid ligger i de to seneste afsluttede måneder.
--
-- Lejerne dækker de tilfælde, månedsopgørelsen skal kunne vise:
--   Anna      1 reol, pænt salg                      -> positiv udbetaling
--   Bo        2 reoler (850 + 825)
--   Camilla   4 reoler (850 + 825 + 825 + 800), stort salg
--   David     1 reol, meget lavt salg                -> negativ udbetaling (rød)
--   Emma      1 reol, ingen e-mail registreret
--   Frederik  opsagt, lejemålet sluttede den 1. i denne måned (reolen er ledig igen)
--   Gitte     under opsigelse, lejemålet slutter den 1. i næste måned
--   Henrik    startede midt i forrige måned          -> kun salg i forrige måned
--   Ida       2 reoler, intet salg                   -> negativ udbetaling (rød)
--   Jonas     ny kunde uden reoler                   -> vises ikke i opgørelsen
-- Derudover et par butikssalg på reol 0 (uden lejer), som ikke må tælle med.
-- ============================================================================

USE [Reolmarkedet];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM dbo.RENTER)
BEGIN
    RAISERROR('Der findes allerede lejere i databasen. Kør schema.sql først, hvis testdata skal indlæses forfra.', 16, 1);
    RETURN;
END

DECLARE @thisMonth DATETIME2 = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
DECLARE @nextMonth DATETIME2 = DATEADD(MONTH, 1, @thisMonth);
DECLARE @oneMonthAgo DATETIME2 = DATEADD(MONTH, -1, @thisMonth);
DECLARE @threeMonthsAgo DATETIME2 = DATEADD(MONTH, -3, @thisMonth);

BEGIN TRANSACTION;

-- 1. Lejere
INSERT INTO dbo.RENTER (FirstName, LastName, Address, PostalCode, City, Phone, Email) VALUES
(N'Anna',     N'Jensen',      N'Algade 12',          4000, N'Roskilde',   N'20112233', N'anna.jensen@example.dk'),
(N'Bo',       N'Larsen',      N'Søndergade 4',       8000, N'Aarhus C',   N'21223344', N'bo.larsen@example.dk'),
(N'Camilla',  N'Nielsen',     N'Vestergade 27, 2.',  5000, N'Odense C',   N'22334455', N'camilla.nielsen@example.dk'),
(N'David',    N'Pedersen',    N'Møllevej 8',         7100, N'Vejle',      N'23445566', N'david.pedersen@example.dk'),
(N'Emma',     N'Christensen', N'Birkevænget 3',      6000, N'Kolding',    N'24556677', NULL),
(N'Frederik', N'Andersen',    N'Havnegade 15',       9000, N'Aalborg',    N'25667788', N'frederik.andersen@example.dk'),
(N'Gitte',    N'Sørensen',    N'Kirkestræde 6',      4600, N'Køge',       NULL,        N'gitte.soerensen@example.dk'),
(N'Henrik',   N'Møller',      N'Åboulevarden 41',    8700, N'Horsens',    N'27889900', N'henrik.moeller@example.dk'),
(N'Ida',      N'Rasmussen',   N'Lærkevej 19',        2800, N'Kgs. Lyngby', N'28990011', N'ida.rasmussen@example.dk'),
(N'Jonas',    N'Thomsen',     N'Nørrebrogade 102',   2200, N'København N', N'29001122', N'jonas.thomsen@example.dk');

-- 2. Lejeaftaler
-- RackStatus: 0 = ledig, 1 = udlejet, 2 = under opsigelse
DECLARE @rental TABLE (
    FirstName NVARCHAR(100),
    RackNumber INT,
    StartDate DATETIME2,
    EndDate DATETIME2 NULL,
    MonthlyRent DECIMAL(18,2),
    RackStatus INT
);

INSERT INTO @rental (FirstName, RackNumber, StartDate, EndDate, MonthlyRent, RackStatus) VALUES
(N'Anna',      1, DATEADD(DAY,  4, @threeMonthsAgo), NULL,       850.00, 1),
(N'Bo',        2, DATEADD(DAY,  9, @threeMonthsAgo), NULL,       850.00, 1),
(N'Bo',        3, DATEADD(DAY,  9, @threeMonthsAgo), NULL,       825.00, 1),
(N'Camilla',  10, DATEADD(DAY,  1, @threeMonthsAgo), NULL,       850.00, 1),
(N'Camilla',  11, DATEADD(DAY,  1, @threeMonthsAgo), NULL,       825.00, 1),
(N'Camilla',  12, DATEADD(DAY,  1, @threeMonthsAgo), NULL,       825.00, 1),
(N'Camilla',  13, DATEADD(DAY,  1, @threeMonthsAgo), NULL,       800.00, 1),
(N'David',    20, DATEADD(DAY, 14, @threeMonthsAgo), NULL,       850.00, 1),
(N'Emma',     21, DATEADD(DAY, 19, @threeMonthsAgo), NULL,       850.00, 1),
(N'Frederik', 30, DATEADD(DAY,  2, @threeMonthsAgo), @thisMonth, 850.00, 0),
(N'Gitte',    31, DATEADD(DAY,  6, @threeMonthsAgo), @nextMonth, 850.00, 2),
(N'Henrik',   40, DATEADD(DAY, 14, @oneMonthAgo),    NULL,       850.00, 1),
(N'Ida',      41, DATEADD(DAY, 11, @threeMonthsAgo), NULL,       850.00, 1),
(N'Ida',      42, DATEADD(DAY, 11, @threeMonthsAgo), NULL,       825.00, 1);

INSERT INTO dbo.RENTAL (RenterId, RackId, StartDate, EndDate, MonthlyRent)
SELECT renter.RenterId, rack.RackId, r.StartDate, r.EndDate, r.MonthlyRent
FROM @rental r
JOIN dbo.RENTER renter ON renter.FirstName = r.FirstName
JOIN dbo.RACK rack ON rack.Number = r.RackNumber;

UPDATE rack
SET rack.Status = r.RackStatus
FROM dbo.RACK rack
JOIN @rental r ON r.RackNumber = rack.Number;

-- 3. Salg
-- MonthsBack: 2 = for to måneder siden, 1 = forrige måned. DayOfMonth holdes på 1-28,
-- så datoen findes i alle måneder. PaymentMethod: 0 = MobilePay, 1 = Bank, 2 = Kontant.
DECLARE @sale TABLE (
    RackNumber INT,
    MonthsBack INT,
    DayOfMonth INT,
    HourOfDay INT,
    Amount DECIMAL(18,2),
    Description NVARCHAR(200),
    PaymentMethod INT
);

INSERT INTO @sale (RackNumber, MonthsBack, DayOfMonth, HourOfDay, Amount, Description, PaymentMethod) VALUES
-- Anna (reol 1)
( 1, 2,  3, 11,  249.00, N'Vinterjakke str. 110',      0),
( 1, 2,  9, 14,  120.00, N'Børnebøger, 4 stk.',        2),
( 1, 2, 17, 12,  399.00, N'Løbecykel',                 0),
( 1, 2, 24, 16,  180.00, N'Gummistøvler',              1),
( 1, 1,  2, 10,  450.00, N'Flyverdragt',               0),
( 1, 1, 11, 13,   95.00, N'Puslespil',                 2),
( 1, 1, 19, 15,  320.00, N'Autostol',                  0),
( 1, 1, 27, 11,  275.00, N'LEGO-sæt',                  0),
-- Bo (reol 2 og 3)
( 2, 2,  5, 12,  600.00, N'Stel, 12 personer',         0),
( 2, 2, 14, 15,  350.00, N'Lysestager, messing',       1),
( 3, 2, 21, 10,  890.00, N'PH-lampe',                  0),
( 2, 1,  6, 14,  425.00, N'Kaffekander',               0),
( 3, 1, 13, 11,  750.00, N'Royal Copenhagen fad',      1),
( 3, 1, 22, 16,  510.00, N'Glasvaser, 3 stk.',         2),
( 2, 1, 26, 13,  295.00, N'Bordlampe',                 0),
-- Camilla (reol 10-13)
(10, 2,  2, 10,  799.00, N'Uldfrakke',                 0),
(11, 2,  7, 13,  450.00, N'Læderstøvler',              0),
(12, 2, 12, 15,  650.00, N'Kjole, silke',              1),
(13, 2, 18, 11,  320.00, N'Tørklæder',                 2),
(10, 2, 25, 14,  980.00, N'Designertaske',             0),
(11, 2, 27, 16,  240.00, N'Bælter',                    0),
(10, 1,  4, 12, 1200.00, N'Vinterfrakke',              0),
(12, 1,  8, 10,  560.00, N'Blazer',                    1),
(13, 1, 15, 14,  380.00, N'Sneakers',                  0),
(11, 1, 20, 15,  720.00, N'Skindjakke',                0),
(12, 1, 23, 11,  290.00, N'Strik',                     2),
(13, 1, 28, 13,  845.00, N'Kufferter, sæt',            0),
-- David (reol 20) - meget lavt salg
(20, 2, 16, 12,   60.00, N'CD''er',                    2),
(20, 1, 10, 15,   85.00, N'DVD-film',                  2),
-- Emma (reol 21)
(21, 2,  8, 11,  340.00, N'Garn og strikkepinde',      0),
(21, 2, 20, 14,  460.00, N'Symaskine',                 1),
(21, 1,  5, 13,  525.00, N'Stofruller',                0),
(21, 1, 18, 10,  410.00, N'Broderisæt',                0),
(21, 1, 25, 16,  199.95, N'Knapper og bånd',           2),
-- Frederik (reol 30) - lejemålet sluttede den 1. i denne måned
(30, 2,  6, 15,  700.00, N'Boremaskine',               0),
(30, 2, 19, 12,  450.00, N'Værktøjskasse',             1),
(30, 1,  9, 11,  380.00, N'Stiksav',                   0),
(30, 1, 21, 14,  620.00, N'Kompressor',                0),
-- Gitte (reol 31) - under opsigelse
(31, 2, 11, 13,  560.00, N'Havemøbler, 2 stole',       0),
(31, 2, 23, 10,  330.00, N'Krukker',                   2),
(31, 1,  7, 15,  480.00, N'Parasol',                   0),
(31, 1, 17, 12,  615.00, N'Plantekasser',              1),
-- Henrik (reol 40) - startede den 15. i forrige måned
(40, 1, 16, 14,  550.00, N'Vinylplader, 10 stk.',      0),
(40, 1, 24, 11,  925.00, N'Pladespiller',              0),
-- Butikkens egne salg (reol 0, ingen lejer)
( 0, 2, 13, 12,   15.00, N'Bæreposer',                 2),
( 0, 1, 12, 13,   25.00, N'Prismærker',                0);

INSERT INTO dbo.SALE (RackId, RenterId, Amount, Description, Date, PaymentMethod)
SELECT
    rack.RackId,
    rental.RenterId,
    s.Amount,
    s.Description,
    DATEADD(HOUR, s.HourOfDay, DATEADD(DAY, s.DayOfMonth - 1, DATEADD(MONTH, -s.MonthsBack, @thisMonth))),
    s.PaymentMethod
FROM @sale s
JOIN dbo.RACK rack ON rack.Number = s.RackNumber
LEFT JOIN dbo.RENTAL rental ON rental.RackId = rack.RackId;

COMMIT TRANSACTION;

SELECT
    (SELECT COUNT(*) FROM dbo.RENTER) AS Lejere,
    (SELECT COUNT(*) FROM dbo.RENTAL) AS Lejeaftaler,
    (SELECT COUNT(*) FROM dbo.SALE) AS Salg;
GO
