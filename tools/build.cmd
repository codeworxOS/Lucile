@echo off

del .\nuget\*.nupkg

for /r %%f in (*.version) do (

echo "creating version %%~nf"

dotnet clean ../Lucile.sln

dotnet restore ../Lucile.sln

dotnet msbuild ../Lucile.sln /p:Configuration=Release;SignAssembly=true;AssemblyOriginatorKeyFile=..\..\private\signkey.snk;VersionSuffix=%%~nf

dotnet pack ../src/Lucile.Primitives --no-build -c Release -o ..\..\tools\nuget\ --include-source --version-suffix %%~nf
dotnet pack ../src/Lucile.Core --no-build -c Release -o ..\..\tools\nuget\ --include-source --version-suffix %%~nf
dotnet pack ../src/Lucile.EntityFrameworkCore --no-build -c Release -o ..\..\tools\nuget\ --include-source --version-suffix %%~nf
dotnet pack ../src/Lucile.ServiceModel --no-build -c Release -o ..\..\tools\nuget\ --include-source --version-suffix %%~nf

)

PAUSE