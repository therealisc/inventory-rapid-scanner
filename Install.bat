winget install -e --id Git.Git
winget install Microsoft.DotNet.SDK.7

cd /
git clone https://github.com/john-admin2024/inventory-rapid-scanner.git
cd inventory-rapid-scanner
git pull

copy vfpoledb.dll c:\Windows\System32
cd c:\Windows\System32
%windir%\System32\regsvr32.exe /u Vfpoledb.dll
%windir%\System32\regsvr32.exe /i Vfpoledb.dll

%~dp0Launch.bat
pause
