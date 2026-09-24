@echo off
cd /d "%~dp0"
echo Koerer tests uden database...
dotnet test ReolmarkedetG12.Tests\ReolmarkedetG12.Tests.csproj --filter "TestCategory!=Database"
pause