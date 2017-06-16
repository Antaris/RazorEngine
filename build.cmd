@echo off
if exist bootstrap.cmd (
  call bootstrap.cmd
)

set buildFile=build.fsx
"packages/Yaaf.AdvancedBuilding/content/build.cmd" %*
