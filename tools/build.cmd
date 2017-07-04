@echo off

del .\nuget\*.nupkg

for /r %%f in (*.version) do (

echo "creating version %%~nf"

dotnet clean ../Lucile.sln

dotnet restore ../Lucile.sln /p:VersionSuffix=%%~nf

dotnet msbuild ../Lucile.sln /p:Configuration=Release;

dotnet pack ../src/Lucile.Primitives/Lucile.Primitives.csproj --no-build -c Release -o ..\..\tools\nuget\ --include-symbols --include-source --version-suffix %%~nf
dotnet pack ../src/Lucile.Core/Lucile.Core.csproj --no-build -c Release -o ..\..\tools\nuget\ --include-symbols --include-source --version-suffix %%~nf
dotnet pack ../src/Lucile.EntityFrameworkCore/Lucile.EntityFrameworkCore.csproj --no-build -c Release -o ..\..\tools\nuget\ --include-symbols --include-source --version-suffix %%~nf
dotnet pack ../src/Lucile.ServiceModel/Lucile.ServiceModel.csproj --no-build -c Release -o ..\..\tools\nuget\ --include-symbols --include-source --version-suffix %%~nf

)

PAUSE