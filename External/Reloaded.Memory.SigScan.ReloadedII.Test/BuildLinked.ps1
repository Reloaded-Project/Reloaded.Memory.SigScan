# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/Reloaded.Memory.SigScan.ReloadedII.Test/*" -Force -Recurse
dotnet publish "./Reloaded.Memory.SigScan.ReloadedII.Test.csproj" -c Release -o "$env:RELOADEDIIMODS/Reloaded.Memory.SigScan.ReloadedII.Test" /p:OutputPath="./bin/Release" /p:RobustILLink="true"

# Restore Working Directory
Pop-Location