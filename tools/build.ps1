Import-Module -Name "./Build-Versioning.psm1"


$projects = "..\src\Lucile.Primitives\Lucile.Primitives.csproj",
"..\src\Lucile.Core\Lucile.Core.csproj",
"..\src\Lucile.EntityFramework\Lucile.EntityFramework.csproj",
"..\src\Lucile.EntityFrameworkCore\Lucile.EntityFrameworkCore.csproj",
"..\src\Lucile.ServiceModel\Lucile.ServiceModel.csproj",
"..\src\Lucile.Windows\Lucile.Windows.csproj"


New-NugetPackages `
    -Projects $projects `
    -NugetServerUrl "https://www.myget.org/F/codeworx/api/v2" `
    -VersionPackage "Lucile.Core" `
    -OutputPath "..\dist\nuget" `
    -MsBuildParams "SignAssembly=true;AssemblyOriginatorKeyFile=..\..\private\signkey.snk;SourceLinkCreate=true"