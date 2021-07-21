rem echo off

rem 
rem Must be run from the projects git\project\scripts folder - everything is relative
rem run >build [versionNumber]
rem versionNumber is YY.MM.DD.build-number, like 20.5.8.1
rem


c:
cd \Git\aoBlog\scripts

rem all paths are relative to the git scripts folder

set appName=app210629
set collectionName=Blog
set solutionName=aoBlogs2.sln
set collectionPath=..\collections\blog\
set binPath=..\source\aoblogs2\bin\debug\
set msbuildLocation=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\
set deploymentFolderRoot=C:\deployments\aoBlog\Dev\

rem prompt user if appName is correct
@echo Build project and install on site: %appName%
pause

set year=%date:~12,4%
set month=%date:~4,2%
if %month% GEQ 10 goto monthOk
set month=%date:~5,1%
:monthOk
set day=%date:~7,2%
if %day% GEQ 10 goto dayOk
set day=%date:~8,1%
:dayOk
set versionMajor=%year%
set versionMinor=%month%
set versionBuild=%day%
set versionRevision=1
rem
rem if deployment folder exists, delete it and make directory
rem
:tryagain
set versionNumber=%versionMajor%.%versionMinor%.%versionBuild%.%versionRevision%
if not exist "%deploymentFolderRoot%%versionNumber%" goto :makefolder
set /a versionRevision=%versionRevision%+1
goto tryagain
:makefolder
md "%deploymentFolderRoot%%versionNumber%"

rem ==============================================================
rem
echo build 
rem
cd ..\source
"%msbuildLocation%msbuild.exe" %solutionName%
if errorlevel 1 (
   echo failure building
   pause
   exit /b %errorlevel%
)
cd ..\scripts

rem pause

rem ==============================================================
rem
echo Build addon collection
rem

rem build collection folder
copy "%binPath%*.dll" %collectionPath%

c:
cd %collectionPath%

copy ..\..\ui\*.png .
copy ..\..\ui\*.css .
copy ..\..\ui\*.js .
copy ..\..\ui\*.txt .
copy ..\..\ui\*.jpg .

rem create new collection zip file
del "%collectionName%.zip" /Q
"c:\program files\7-zip\7z.exe" a "%collectionName%.zip"
xcopy "%collectionName%.zip" "%deploymentFolderRoot%%versionNumber%" /Y
cd ..\..\scripts

rem clean collection folder
del "%collectionPath%*.dll"
del "%collectionPath%*.png"
del "%collectionPath%*.css"
del "%collectionPath%*.js"
del "%collectionPath%*.txt"
del "%collectionPath%*.jpg"


