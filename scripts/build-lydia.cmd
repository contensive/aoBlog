
rem all paths are relative to the git scripts folder
set appName=lydiakidwell

call build.cmd

rem upload to contensive application
c:
cd %collectionPath%
cc -a %appName% --installFile "%collectionName%.zip"
cd ..\..\scripts

pause