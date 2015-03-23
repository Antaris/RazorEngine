@echo off
REM cls
set nuget_packages=packages
set paket_packages=packages

set nuget_path=NuGet.CommandLine/tools/NuGet.exe
set fake_path=FAKE/tools/FAKE.exe

REM resore paket build dependencies
if exist ".paket/paket.bootstrapper.exe" (
  echo Bootstrap paket
  .paket\paket.bootstrapper.exe
  if errorlevel 1 (
    exit /b %errorlevel%
  )

  echo restore paket packages
  .paket\paket.exe restore
  if errorlevel 1 (
    exit /b %errorlevel%
  )

  set fake=%paket_packages%/%fake_path%
  set nuget=%paket_packages%/%nuget_path%
) 
REM Download NuGet (if not already available because of paket)
if not exist %nuget% (
  if exist downloadNuget.fsx (
    if not exist %nuget_packages%/%nuget_path% (
      echo Bootstrap Nuget
      "C:\Program Files (x86)\Microsoft SDKs\F#\3.1\Framework\v4.0\fsi.exe" downloadNuget.fsx
    )
    set nuget=%nuget_packages%/%nuget_path%
  )
)
REM Restore Nuget build dependencies
if exist packages.config (
  if exist %nuget% (
    echo Resolve build dependencies
    "%nuget%" "install" "packages.config" "-OutputDirectory" %nuget_packages% "-ExcludeVersion"
  ) else (
    echo NuGet build dependencies file found but no NuGet.exe could be found, either add downloadNuget.fsx or add Nuget.Commandline as paket dependency!.
  )
)

REM FAKE could be available as nuget dependency
if not exist %fake% (
  set fake=%nuget_packages%/%fake_path%
  if not exist %fake% (
    echo Could not find FAKE in nuget or paket dependencies!
    exit /b 1
  )
)

echo start FAKE for the rest of the build procedure...
"%fake%" %* --fsiargs -d:WIN64 -d:FAKE build.fsx

