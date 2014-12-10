#!/bin/bash

# We require nuget to be installed via package manager instead of bootstrapping it ourselfs
command -v mono >/dev/null 2>&1 || { echo >&2 "Please install mono dependency."; exit 1; }
command -v fsharpi >/dev/null 2>&1 || { echo >&2 "Please install fsharpi dependency."; exit 1; }
myMono="mono --debug --runtime=v4.0"
if [ ! -f .nuget/Build/NuGet.CommandLine/tools/NuGet.exe ]; then
	echo "Bootstrap Nuget"
	fsharpi downloadNuget.fsx
fi
nuget="$myMono .nuget/Build/NuGet.CommandLine/tools/NuGet.exe"
fake="$myMono .nuget/Build/FAKE/tools/FAKE.exe"

echo "Resolve dependencies"
$nuget "install" "packages.config" "-OutputDirectory" ".nuget/Build" "-ExcludeVersion" 

echo "Starting build"


if [ -z "$1" ]; then
    target="all"
else
    target="$1"
fi
# let buildTargets = environVarOrDefault "BUILDTARGETS" ""
# export BUILDTARGETS="$@"
$fake "build.fsx" "target=$target" 
