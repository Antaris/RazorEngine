#!/bin/bash
if [ -f "bootstrap.sh" ];
then
  ./bootstrap.sh
fi

build="packages/Yaaf.AdvancedBuilding/content/build.sh"
chmod +x "$build"
"$build" $@
