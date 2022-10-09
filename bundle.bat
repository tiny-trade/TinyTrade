set arch=%1
dotnet publish -c Release -r %arch% -f net6.0 --no-self-contained -o bundle/release/%arch%
echo Creating archive...
tar.exe -a -c -f ./%arch%-bundle.zip -C bundle/release/%arch% *