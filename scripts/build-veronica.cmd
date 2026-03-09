
rem all paths are relative to the git scripts folder
set appName=veronica

call build.cmd

rem upload to contensive application
c:
cd %collectionPath%
cc -a %appName% --installFile "%deploymentFolderRoot%%versionNumber%\%collectionName%.zip"
cd ..\..\scripts

pause