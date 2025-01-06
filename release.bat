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

call .\source\COM3D2.MotionTimelineEditor_MeidoPhotoStudio.Plugin\build.bat
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

call .\source\COM3D2.MotionTimelineEditor_DCM.Plugin\build.bat
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

call .\source\COM3D2.MotionTimelineEditor_SceneCapture.Plugin\build.bat
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

call .\source\COM3D2.MotionTimelineEditor_PartsEdit.Plugin\build.bat
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

call .\source\COM3D2.MotionTimelineEditor_PngPlacement.Plugin\build.bat
if %ERRORLEVEL% neq 0 (
    echo ビルドに失敗しました
    exit /b 1
)

copy .\UnityInjector\Config\*.csv ..\UnityInjector\Config
if %ERRORLEVEL% neq 0 (
    echo Failed to copy csv
    exit /b 1
)

for /f "tokens=*" %%i in ('powershell -Command "$content = Get-Content 'source/COM3D2.MotionTimelineEditor.Plugin/PluginInfo.cs'; $version = [regex]::Match($content, 'PluginVersion = \""(.*?)\""').Groups[1].Value; echo $version"') do set VERSION=%%i
echo VERSION: %VERSION%

set PLUGIN_NAME=COM3D2.MotionTimelineEditor.Plugin

if exist output rmdir /s /q output
md output\%PLUGIN_NAME%

xcopy UnityInjector output\%PLUGIN_NAME%\UnityInjector /E /I

set README_TXT=output\%PLUGIN_NAME%\README.txt
echo このテキストはWeb上で見ることを推奨しています。 > %README_TXT%
echo https://github.com/kidonaru/COM3D2.MotionTimelineEditor.Plugin/blob/master/README.md >> %README_TXT%
echo. >> %README_TXT%
echo. >> %README_TXT%
type README.md >> %README_TXT%

powershell Compress-Archive -Path "output\%PLUGIN_NAME%" -DestinationPath "output\%PLUGIN_NAME%-v%VERSION%.zip" -Force

rmdir /s /q output\%PLUGIN_NAME%

echo ビルドに成功しました
exit /b 0
