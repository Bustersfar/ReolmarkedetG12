@echo off
chcp 65001 >nul
cd /d "%~dp0"

where dotnet >nul 2>nul
if errorlevel 1 (
    echo .NET SDK blev ikke fundet. Hent .NET 10 SDK fra https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo Koerer tests uden database (94 tests)...
echo.
dotnet test ReolmarkedetG12.Tests\ReolmarkedetG12.Tests.csproj --filter "TestCategory!=Database"
echo.
if errorlevel 1 (
    echo Mindst en test fejlede. Laes beskederne ovenfor.
) else (
    echo Alle tests bestod.
)
pause
