@echo off
if exist bootstrap.cmd (
  call bootstrap.cmd
)

set PAKET_VERSION=prerelease
set buildFile=build.fsx
"packages/Yaaf.AdvancedBuilding/content/build.cmd" %*
