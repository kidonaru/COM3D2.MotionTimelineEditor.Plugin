@echo off
chcp 65001

call .\source\COM3D2.MotionTimelineEditor.Plugin\build.bat debug
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

call .\source\COM3D2.MotionTimelineEditor_MultipleMaids.Plugin\build.bat debug
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

call .\source\COM3D2.MotionTimelineEditor_MeidoPhotoStudio.Plugin\build.bat debug
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

call .\source\COM3D2.MotionTimelineEditor_DCM.Plugin\build.bat debug
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

call .\source\COM3D2.MotionTimelineEditor_SceneCapture.Plugin\build.bat debug
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

call .\source\COM3D2.MotionTimelineEditor_PartsEdit.Plugin\build.bat debug
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

call .\source\COM3D2.MotionTimelineEditor_PngPlacement.Plugin\build.bat debug
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

call .\source\COM3D2.MotionTimelineEditor_NPRShader.Plugin\build.bat debug
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

copy .\UnityInjector\Config\*.csv ..\UnityInjector\Config
if %ERRORLEVEL% neq 0 (
    echo Failed to copy csv
    exit /b 1
)

echo ビルドに成功しました
exit /b 0
