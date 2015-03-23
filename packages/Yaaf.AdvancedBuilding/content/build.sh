#!/bin/bash
if test "$OS" = "Windows_NT"
then
  # use .Net
  MONO=""
  DEFINE="WIN64"
  FSI="C:\Program Files (x86)\Microsoft SDKs\F#\3.1\Framework\v4.0\fsi.exe"
else
  # use mono
  command -v mono >/dev/null 2>&1 || { echo >&2 "Please install mono dependency."; exit 1; }
  myMono="mono --debug --runtime=v4.0"
  FSI="fsharpi"

  MONO="$myMono"
  DEFINE="MONO"
fi

nuget_packages="packages"
paket_packages="packages"

nuget_path="NuGet.CommandLine/tools/NuGet.exe"
fake_path="FAKE/tools/FAKE.exe"

# Restore paket build dependencies
if [ -f ".paket/paket.bootstrapper.exe" ];
then
  echo "Bootstrap paket"
  $MONO .paket/paket.bootstrapper.exe
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
    exit $exit_code
  fi

  echo "restore paket packages"
  $MONO .paket/paket.exe restore
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
    exit $exit_code
  fi
  
  fake=$paket_packages/$fake_path
  nuget=$paket_packages/$nuget_path
fi
# Download NuGet (if not already available because of paket)
if [ ! -f "$nuget" ];
then
  if [ -f downloadNuget.fsx ];
  then
    if [ ! -f "$nuget_packages/$nuget_path" ]; then
      echo "Bootstrap Nuget"
      command -v "$FSI" >/dev/null 2>&1 || { echo >&2 "Please install fsharpi or download a NuGet.exe to $nuget_packages/$nuget_path"; exit 1; }
      "$FSI" downloadNuget.fsx
    fi
    nuget="$nuget_packages/$nuget_path"
  fi
fi

# Restore Nuget build dependencies
if [ -f "packages.config" ];
then
  if [ -f "$nuget" ];
  then
    echo "restore NuGet build dependencies."
    $MONO $nuget "install" "packages.config" "-OutputDirectory" "$nuget_packages" "-ExcludeVersion"
  else
    echo "NuGet build dependencies file found but no NuGet.exe could be found, either add downloadNuget.fsx or add Nuget.Commandline as paket dependency!."
  fi
fi

# FAKE could be available as nuget dependency
if [ ! -f "$fake" ];
then
  fake="$nuget_packages/$fake_path"
  if [ ! -f "$fake" ];
  then
    echo "Could not find FAKE in nuget or paket dependencies!"
    exit 1
  fi
fi

echo "start FAKE for the rest of the build procedure..."
$MONO $fake $@ --fsiargs -d:$DEFINE -d:FAKE build.fsx

