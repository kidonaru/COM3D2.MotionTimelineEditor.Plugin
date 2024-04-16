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

echo ビルドに成功しました
exit /b 0
