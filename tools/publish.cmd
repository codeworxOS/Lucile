del .\nuget\*.symbols.nupkg

nuget.exe push .\nuget\*.nupkg 37d53ea3-37b0-4974-a4d5-54e1433c5d02 -Source https://nugetserver.azurewebsites.net/api/v2/package

nuget push .\nuget\*.nupkg fa29550c-a4b2-47a0-aa38-23408376046a -Source https://www.myget.org/F/codeworx/api/v2/package 

PAUSE