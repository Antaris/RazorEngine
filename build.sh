#!/bin/bash
if [ -f "bootstrap.sh" ];
then
  ./bootstrap.sh
fi

export PAKET_VERSION=prerelease
build="packages/Yaaf.AdvancedBuilding/content/build.sh"
chmod +x "$build"
"$build" $@
