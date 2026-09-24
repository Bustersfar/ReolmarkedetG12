# Reolmarkedet: sådan kører du testene

## Det du skal bruge
- Windows
- .NET 10 SDK (gratis): https://dotnet.microsoft.com/download

## Hent koden
1. Gå til https://github.com/Bustersfar/ReolmarkedetG12/tree/MSTestBranch
2. Tryk **Code**, og vælg **Download ZIP**.
3. Højreklik på ZIP-filen, og vælg **Udpak alle**.

## Kør testene
Dobbeltklik på **koer-tests.bat** i den udpakkede mappe.

Hvis Windows viser en blå besked, "Windows beskyttede din pc", så tryk **Flere oplysninger** og derefter **Kør alligevel**. Det er normalt for filer, der er hentet fra internettet.

Der åbner et sort vindue. Når det er færdigt, skal der stå `succeeded: 43` og `failed: 0`.

Det kører 43 tests, og de kræver ingen database.

## Valgfrit: kør alle 60 tests (kræver SQL Server)
1. Åbn `database/schema.sql` i SQL Server Management Studio.
2. Lav en Query hvori i kopierer alt teskten fra schema.sql
3. Erstat `Reolmarkedet` med `ReolmarkedetTest` (3 steder), og kør scriptet.
4. Hedder din server ikke `localhost`, så ret navnet i `ReolmarkedetG12.Tests/TestDatabase.cs`.
5. Dobbeltklik på **koer-alle-tests.bat**.