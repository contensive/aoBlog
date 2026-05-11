@echo off
setlocal enabledelayedexpansion

set siteUrl=https://sprint11.sitefpo.com/installCollection
set collectionZip=%~dp0..\collections\Blog\Blog.zip
set tokenFile=%TEMP%\sprint11-bearer-token.txt

@echo Build project and deploy to sprint11.sitefpo.com

rem run the build
call "%~dp0build.cmd" /nopause
if not "!errorlevel!"=="0" (
    echo Build failed, skipping deploy.
    pause
    exit /b 1
)

rem check for token file created today
set needToken=1
if exist "%tokenFile%" (
    for /f "delims=" %%a in ('powershell -NoProfile -Command "(Get-Item '%tokenFile%').LastWriteTime.ToString('yyyy-MM-dd')"') do set tokenDate=%%a
    for /f "delims=" %%a in ('powershell -NoProfile -Command "Get-Date -Format yyyy-MM-dd"') do set today=%%a
    if "!tokenDate!"=="!today!" set needToken=0
)

rem prompt for token if needed
if "!needToken!"=="1" (
    set /p BEARER_TOKEN=Enter bearer token for sprint11:
    if "!BEARER_TOKEN!"=="" (
        echo No token provided. Aborting.
        pause
        exit /b 1
    )
    echo !BEARER_TOKEN!> "%tokenFile%"
) else (
    set /p BEARER_TOKEN=<"%tokenFile%"
)

echo.
echo Deploying Blog.zip to %siteUrl%...
echo.

curl -X POST "%siteUrl%" -H "Authorization: Bearer !BEARER_TOKEN!" -F "collectionFile=@%collectionZip%"
set curlExit=!errorlevel!

echo.
if not "!curlExit!"=="0" (
    echo.
    echo ========================================
    echo DEPLOY FAILED - curl error !curlExit!
    echo ========================================
)

pause
exit /b !curlExit!
