# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/nights.test.input/*" -Force -Recurse
dotnet publish "./nights.test.input.csproj" -c Release -o "$env:RELOADEDIIMODS/nights.test.input" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location
