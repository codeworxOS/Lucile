@echo off

for %%F in (..\dist\nuget\*.nupkg) do (
    if exist ..\dist\nuget\%%~nF.symbols.nupkg (
	nuget push %%F %PACKAGEAPIKEY% -Source https://www.myget.org/F/codeworx/api/v2/package -SymbolSource https://www.myget.org/F/codeworx/symbols/api/v2/package -SymbolApiKey %SYMBOLSAPIKEY%
    )
    
    if %errorlevel% neq 0 exit /b %errorlevel%
)

del ..\dist\nuget\*.nupkg