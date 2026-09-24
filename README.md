# Reolmarkedet

## Det du skal bruge
- Windows
- .NET 10 SDK (gratis): https://dotnet.microsoft.com/download

## Hent koden
1. Gå til https://github.com/Bustersfar/ReolmarkedetG12
2. Tryk **Code**, og vælg **Download ZIP**.
3. Højreklik på ZIP-filen, og vælg **Udpak alle**.

## Kør testene
Dobbeltklik på **koer-tests.bat** i den udpakkede mappe.

Hvis Windows viser en blå besked, "Windows beskyttede din pc", så tryk **Flere oplysninger** og derefter **Kør alligevel**. Det er normalt for filer, der er hentet fra internettet.

Der åbner et sort vindue. Når det er færdigt, skal der stå `failed: 0`.

Den kører de tests, der ikke kræver en database.

## Valgfrit: kør alle tests (kræver SQL Server)
1. Åbn `database/schema.sql` i SQL Server Management Studio.
2. Åbn en ny Query (Ctrl+N), og kopiér hele teksten fra `schema.sql` ind i den.
3. Erstat `Reolmarkedet` med `ReolmarkedetTest` (3 steder), og kør scriptet.
4. Hedder din server ikke `localhost`, så ret navnet i `ReolmarkedetG12.Tests/TestDatabase.cs`.
5. Dobbeltklik på **koer-alle-tests.bat**.

## Kør selve programmet (kræver SQL Server og Visual Studio)
1. Åbn `database/schema.sql` i SQL Server Management Studio, og kør scriptet. Det opretter databasen `Reolmarkedet`.
2. Gå til mappen `src/ReolmarkedetG12.UI`. Kopiér filen `appsettings.example.json`, og kald kopien `appsettings.json`.
3. Åbn `appsettings.json`, og erstat `YOUR_SERVER_NAME` med navnet på din SQL Server, fx `localhost`.
4. Åbn `ReolmarkedetG12.slnx` i Visual Studio, og tryk **F5**.