@echo off
setlocal enabledelayedexpansion

cd /d %~dp0

set PLUGIN_NAME=COM3D2.MotionTimelineEditor_PngPlacement.Plugin
set SOURCE_DIR=%~dp0

set CONFIG=Release
if "%~1"=="debug" set CONFIG=Debug

set MSBUILD_PATH="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set OUTPUT_DIR=%SOURCE_DIR%\bin\%CONFIG%

if "%CONFIG%"=="Release" (
    %MSBUILD_PATH% %PLUGIN_NAME%.csproj /t:Clean /p:Configuration=Debug
    %MSBUILD_PATH% %PLUGIN_NAME%.csproj /t:Clean /p:Configuration=Release
    if %ERRORLEVEL% neq 0 (
        echo クリーンビルドに失敗しました
        exit /b 1
    )
)

%MSBUILD_PATH% %PLUGIN_NAME%.csproj /p:Configuration=%CONFIG%
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

copy %OUTPUT_DIR%\%PLUGIN_NAME%.dll ..\..\..\UnityInjector
if %ERRORLEVEL% neq 0 (
    echo dllのコピーに失敗しました
    exit /b 1
)

copy %OUTPUT_DIR%\%PLUGIN_NAME%.dll ..\..\UnityInjector
if %ERRORLEVEL% neq 0 (
    echo dllのコピーに失敗しました
    exit /b 1
)
