@echo off
rmdir /s /q bundle 
FOR /d /r . %%d IN ("bin") DO @IF EXIST "%%d" rd /s /q "%%d"
FOR /d /r . %%d IN ("obj") DO @IF EXIST "%%d" rd /s /q "%%d"
rd /s /q bin 
rd /s /q win-x64-bundle
rd /s /q linux-x64-bundle
rd /s /q linux-arm64-bundle
del *.zip
echo Cleaning completed
