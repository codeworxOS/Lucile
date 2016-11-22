@echo off
del .\nuget\*.nupkg

for /r %%f in (*.version) do (

echo "creating version %%~nf"

dotnet restore ../src/Lucile.Primitives
dotnet restore ../src/Lucile.Core
dotnet restore ../src/Lucile.EntityFrameworkCore

dotnet pack ../src/Lucile.Primitives -c Release -o .\nuget\ --version-suffix %%~nf
dotnet pack ../src/Lucile.Core -c Release -o .\nuget\ --version-suffix %%~nf
dotnet pack ../src/Lucile.EntityFrameworkCore -c Release -o .\nuget\ --version-suffix %%~nf

del .\nuget\*.symbols.nupkg

nuget.exe push .\nuget\*.nupkg 37d53ea3-37b0-4974-a4d5-54e1433c5d02 -Source https://nugetserver.azurewebsites.net/api/v2/package


)

PAUSE