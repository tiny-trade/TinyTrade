rmdir /s /q bundle 
FOR /d /r . %%d IN ("bin") DO @IF EXIST "%%d" rd /s /q "%%d"
FOR /d /r . %%d IN ("obj") DO @IF EXIST "%%d" rd /s /q "%%d"
rmdir /s /q bin 
rmdir /s /q win-x64-bundle
rmdir /s /q linux-x64-bundle
del *.zip
echo Cleaning completed
