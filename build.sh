#!/bin/bash
if [ -f "bootstrap.sh" ];
then
  ./bootstrap.sh
fi

# Temporarily set the paket version to a prerelease until it has been deployed
# to a stable release.
export PAKET_VERSION="5.0.0-beta010"

build="packages/Yaaf.AdvancedBuilding/content/build.sh"
chmod +x "$build"
"$build" $@
