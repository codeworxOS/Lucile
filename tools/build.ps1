Import-Module -Name "./Build-Versioning.psm1"


$projects = "..\src\Lucile.Primitives\Lucile.Primitives.csproj",
"..\src\Lucile.Core\Lucile.Core.csproj",
"..\src\Lucile.Dynamic\Lucile.Dynamic.csproj",
"..\src\Lucile.Dynamic.DependencyInjection\Lucile.Dynamic.DependencyInjection.csproj",
"..\src\Lucile.EntityFramework\Lucile.EntityFramework.csproj",
"..\src\Lucile.ServiceModel\Lucile.ServiceModel.csproj",
"..\src\Lucile.ServiceModel.DependencyInjection\Lucile.ServiceModel.DependencyInjection.csproj",
"..\src\Lucile.Windows\Lucile.Windows.csproj",
"..\src\Lucile.AspNetCore\Lucile.AspNetCore.csproj"


$coreVersion =  New-NugetPackages `
                    -Projects $projects `
                    -NugetServerUrl "https://www.nuget.org/api/v2" `
                    -VersionPackage "Lucile.Core" `
                    -OutputPath "..\dist\nuget" `
                    -VersionFilePath "..\version.json" `
                    -MsBuildParams "SignAssembly=true;AssemblyOriginatorKeyFile=..\..\private\lucile_signkey.snk"

$projects = "..\src\Lucile.EntityFrameworkCore\Lucile.EntityFrameworkCore.csproj"

    New-NugetPackages `
    -Projects $projects `
    -NugetServerUrl "https://www.nuget.org/api/v2" `
    -VersionPackage "Lucile.EntityFrameworkCore" `
    -VersionFilePath "..\version_ef6.json" `
    -DoNotCleanOutput `
    -OutputPath "..\dist\nuget" `
    -MsBuildParams "SignAssembly=true;AssemblyOriginatorKeyFile=..\..\private\lucile_signkey.snk;EfVersion=6;LucileCoreVersion=$($coreVersion.NugetVersion)"

    New-NugetPackages `
    -Projects $projects `
    -NugetServerUrl "https://www.nuget.org/api/v2" `
    -VersionPackage "Lucile.EntityFrameworkCore" `
    -VersionFilePath "..\version_ef7.json" `
    -DoNotCleanOutput `
    -OutputPath "..\dist\nuget" `
    -MsBuildParams "SignAssembly=true;AssemblyOriginatorKeyFile=..\..\private\lucile_signkey.snk;EfVersion=7;LucileCoreVersion=$($coreVersion.NugetVersion)"

    New-NugetPackages `
    -Projects $projects `
    -NugetServerUrl "https://www.nuget.org/api/v2" `
    -VersionPackage "Lucile.EntityFrameworkCore" `
    -VersionFilePath "..\version_ef8.json" `
    -DoNotCleanOutput `
    -OutputPath "..\dist\nuget" `
    -MsBuildParams "SignAssembly=true;AssemblyOriginatorKeyFile=..\..\private\lucile_signkey.snk;EfVersion=8;LucileCoreVersion=$($coreVersion.NugetVersion)"




Write-Host "##vso[build.updatebuildnumber]$($coreVersion.NugetVersion)"