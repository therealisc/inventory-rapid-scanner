winget install -e --id Git.Git
winget winget install Microsoft.DotNet.SDK.7

cd /
git clone https://github.com/therealisc/inventory-rapid-scanner.git

git pull

cd inventory-rapid-scanner/ConsoleScanner
dotnet run
