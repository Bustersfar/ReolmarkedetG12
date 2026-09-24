@echo off
cd /d "%~dp0"
echo Koerer alle tests, kraever testdatabasen...
dotnet test ReolmarkedetG12.Tests\ReolmarkedetG12.Tests.csproj
pause