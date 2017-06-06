@echo off
if exist bootstrap.cmd (
  call bootstrap.cmd
)

:: Temporarily set the paket version to a prerelease until it has been deployed
:: to a stable release.
set PAKET_VERSION=5.0.0-beta010
set buildFile=build.fsx
"packages/Yaaf.AdvancedBuilding/content/build.cmd" %*
