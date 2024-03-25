@echo off

call build.bat

set VERSION=1.0.0.0
set PLUGIN_NAME=COM3D2.MotionTimelineEditor.Plugin

if exist output rmdir /s /q output
md output\%PLUGIN_NAME%

xcopy img output\%PLUGIN_NAME% /E /I
xcopy UnityInjector output\%PLUGIN_NAME%\UnityInjector /E /I

chcp 65001
set README_TXT=output\%PLUGIN_NAME%\README.txt
echo このテキストはWeb上で見ることを推奨しています。 > %README_TXT%
echo https://github.com/kidonaru/COM3D2.MotionTimelineEditor.Plugin/blob/master/README.md >> %README_TXT%
echo. >> %README_TXT%
echo. >> %README_TXT%
type README.md >> %README_TXT%
chcp 932

powershell Compress-Archive -Path "output\%PLUGIN_NAME%" -DestinationPath "output\%PLUGIN_NAME%-v%VERSION%.zip" -Force

rmdir /s /q output\%PLUGIN_NAME%
