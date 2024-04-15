@echo off
chcp 65001

call .\source\COM3D2.MotionTimelineEditor.Plugin\build.bat
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

call .\source\COM3D2.MotionTimelineEditor_MultipleMaids.Plugin\build.bat
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

set VERSION=1.2.2.0
set PLUGIN_NAME=COM3D2.MotionTimelineEditor.Plugin

if exist output rmdir /s /q output
md output\%PLUGIN_NAME%

xcopy img output\%PLUGIN_NAME% /E /I
xcopy UnityInjector output\%PLUGIN_NAME%\UnityInjector /E /I

set README_TXT=output\%PLUGIN_NAME%\README.txt
echo このテキストはWeb上で見ることを推奨しています。 > %README_TXT%
echo https://github.com/kidonaru/COM3D2.MotionTimelineEditor.Plugin/blob/v%VERSION%/README.md >> %README_TXT%
echo. >> %README_TXT%
echo. >> %README_TXT%
type README.md >> %README_TXT%

powershell Compress-Archive -Path "output\%PLUGIN_NAME%" -DestinationPath "output\%PLUGIN_NAME%-v%VERSION%.zip" -Force

rmdir /s /q output\%PLUGIN_NAME%
