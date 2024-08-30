winget install -e --id Git.Git -v 2.46.0
winget install Microsoft.DotNet.SDK.7

cd /
git clone https://github.com/therealisc/inventory-rapid-scanner.git

cd inventory-rapid-scanner
git pull

VFPOLEDBSetup.msi
Launch.bat
pause
