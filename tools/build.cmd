@echo off
set DOTNET_BUILD_STRONG_NAME_KEYFILE=..\..\private\signkey.snk

del .\nuget\*.nupkg

for /r %%f in (*.version) do (

echo "creating version %%~nf"

dotnet restore ../src/Lucile.Primitives
dotnet restore ../src/Lucile.Core
dotnet restore ../src/Lucile.EntityFrameworkCore

dotnet pack ../src/Lucile.Primitives -c Release -o .\nuget\ --version-suffix %%~nf
dotnet pack ../src/Lucile.Core -c Release -o .\nuget\ --version-suffix %%~nf
dotnet pack ../src/Lucile.EntityFrameworkCore -c Release -o .\nuget\ --version-suffix %%~nf

)

PAUSE