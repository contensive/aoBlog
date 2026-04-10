@echo off
setlocal

call "%~dp0build.cmd" /nopause
if not "%errorlevel%"=="0" (
    echo Deploy aborted — build failed.
    pause
    exit /b %errorlevel%
)

rem find the latest deployment folder and install to ecommercetest
for /f "delims=" %%d in ('dir /b /ad /od "C:\deployments\aoBlog"') do set latestVersion=%%d
cc -a ecommercetest --installFile "C:\deployments\aoBlog\%latestVersion%\Blog.zip"

pause
