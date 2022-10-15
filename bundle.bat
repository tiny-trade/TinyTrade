@echo off

set arch=%1
for /f "delims=" %%a in ('dir /b /ad %cd% ^|findstr /ive /c:\.Tests /c:\.git /c:\bin /c:\bundle /c:\.vs /c:\.github') do (
	echo  ^

Publishing: %%a

	dotnet publish %%a -c Release -r %arch% -f net6.0 --no-self-contained -o bundle/release/%arch% 
)

echo Creating archive...
tar.exe -a -c -f ./%arch%-bundle.zip -C bundle/release/%arch% *

