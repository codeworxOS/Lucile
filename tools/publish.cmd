del .\nuget\*.symbols.nupkg

nuget.exe push .\nuget\*.nupkg 37d53ea3-37b0-4974-a4d5-54e1433c5d02 -Source https://nugetserver.azurewebsites.net/api/v2/package

PAUSE