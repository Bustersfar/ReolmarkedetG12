# Reolmarkedet

Middelby Reolmarked er en WPF-applikation (C#, MVVM) med en SQL Server-database. Programmet bruges til at oprette lejere og lejemål, registrere salg og lave den månedlige afregning med lejerne.

Lavet af gruppe 12: Line Møller Jørgensen, Henrik Carlsen og Jens Benjamin Hansen.

Systemdokumentationen (klassediagram, ER-diagram, sekvensdiagrammer, sporbarhed og test) ligger i den afleverede PDF.

## Det du skal bruge
- Windows
- .NET 10 SDK (gratis): https://dotnet.microsoft.com/download
- SQL Server og Visual Studio, hvis du vil køre selve programmet eller alle tests

## Hent koden
1. Gå til https://github.com/Bustersfar/ReolmarkedetG12
2. Tryk **Code**, og vælg **Download ZIP**.
3. Højreklik på ZIP-filen, og vælg **Udpak alle**.

## Kør testene (kræver ikke database)
Dobbeltklik på **koer-tests.bat** i den udpakkede mappe.

Hvis Windows viser en blå besked, "Windows beskyttede din pc", så tryk **Flere oplysninger** og derefter **Kør alligevel**. Det er normalt for filer, der er hentet fra internettet.

Der åbner et sort vindue. Når det er færdigt, skal der stå `Alle tests bestod`, og testoversigten skal vise `failed: 0`. Den kører de 94 tests, der ikke kræver en database.

## Valgfrit: kør alle 115 tests (kræver SQL Server)
De sidste 21 tests prøver SQL-koden mod en rigtig database.
1. Åbn `database/schema.sql` i SQL Server Management Studio.
2. Åbn en ny Query (Ctrl+N), og kopiér hele teksten fra `schema.sql` ind i den.
3. Erstat `Reolmarkedet` med `ReolmarkedetTest` (3 steder), og kør scriptet.
4. Hedder din server ikke `localhost`, så ret navnet i `ReolmarkedetG12.Tests/TestDatabase.cs`.
5. Dobbeltklik på **koer-alle-tests.bat**.

## Kør selve programmet (kræver SQL Server og Visual Studio)
1. Åbn `database/schema.sql` i SQL Server Management Studio, og kør scriptet. Det opretter databasen `Reolmarkedet` med reoler og priser.
2. Valgfrit: kør `database/testdata.sql` bagefter. Det lægger 10 eksempellejere, lejemål og salg ind, så du kan se månedsopgørelsen med det samme.
3. Gå til mappen `src/ReolmarkedetG12.UI`. Kopiér filen `appsettings.example.json`, og kald kopien `appsettings.json`.
4. Åbn `appsettings.json`, og erstat `YOUR_SERVER_NAME` med navnet på din SQL Server, fx `localhost`.
5. Åbn `ReolmarkedetG12.slnx` i Visual Studio, og tryk **F5**.

Adgangskoden til fanerne Søg/ret salg og Månedsopgørelse står i `appsettings.json` (`SearchSalesPassword`, som standard `1234`).

## Programmets faner
- **Kunder:** opret, ret og slet lejere.
- **Reoler:** se reolerne, opret lejemål og opsig dem.
- **Kasse / Salg:** registrer salg.
- **Søg/ret salg:** find, ret og slet salg (med log).
- **Månedsopgørelse:** afregning pr. lejer (salg − kommission − leje).

## Mapper
- `src/ReolmarkedetG12.Core`: modeller, services (prisberegning og afregning) og repositories.
- `src/ReolmarkedetG12.UI`: WPF-programmet (Views, ViewModels, MVVM-klasser).
- `ReolmarkedetG12.Tests`: MSTest-tests og fakes.
- `database`: `schema.sql` og `testdata.sql`.
