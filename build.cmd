@echo off
cls

if not exist .nuget/Build/NuGet.CommandLine/tools/NuGet.exe (
	echo Bootstrap Nuget
	"C:\Program Files (x86)\Microsoft SDKs\F#\3.1\Framework\v4.0\fsi.exe" downloadNuget.fsx
)

echo Resolve build dependencies
".nuget/Build/NuGet.CommandLine/tools/NuGet.exe" "install" "packages.config" "-OutputDirectory" ".nuget/Build" "-ExcludeVersion" 

SET TARGET="All"

IF NOT [%1]==[] (set TARGET="%1")

echo Starting build
".nuget/Build/FAKE/tools/Fake.exe" "build.fsx" "target=%TARGET%"
exit /b %errorlevel%