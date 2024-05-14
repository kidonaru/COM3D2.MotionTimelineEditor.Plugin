@echo off
setlocal enabledelayedexpansion

cd /d %~dp0

set PLUGIN_NAME=COM3D2.MotionTimelineEditor.Plugin
set CSC_PATH="C:\Windows\Microsoft.NET\Framework\v3.5\csc"
set LIB_PATHS="/lib:..\..\..\..\COM3D2x64_Data\Managed" "/lib:..\..\.." "/lib:..\..\..\lib" "/lib:..\..\..\UnityInjector"
set REFERENCES="/r:UnityEngine.dll" "/r:UnityInjector.dll" "/r:Assembly-CSharp.dll" "/r:Assembly-CSharp-firstpass.dll"
set SOURCE_DIR=%~dp0
set MAIN_FILE=%SOURCE_DIR%%PLUGIN_NAME%.cs

set DEBUG_OPTION=
IF "%~1"=="debug" (
    set DEBUG_OPTION="/debug+"
    set DEBUG_OPTION=!DEBUG_OPTION! /define:DEBUG
)

set SOURCES="%MAIN_FILE%"

for /R %SOURCE_DIR% %%f in (*.cs) do (
    echo Add: %%f
    set "SOURCE_FILE=%%f"
    if not "%%f"=="%MAIN_FILE%" set SOURCES=!SOURCES! "!SOURCE_FILE:%SOURCE_DIR%=.\!"
)

echo %CSC_PATH% /t:library %LIB_PATHS% %REFERENCES% %DEBUG_OPTION% %SOURCES%
%CSC_PATH% /t:library %LIB_PATHS% %REFERENCES% %DEBUG_OPTION% %SOURCES%
if %ERRORLEVEL% neq 0 (
    echo Failed build
    exit /b 1
)

copy *.dll ..\..\..\UnityInjector
if %ERRORLEVEL% neq 0 (
    echo Failed to copy dlls
    exit /b 1
)

move *.dll ..\..\UnityInjector
del *.pdb
