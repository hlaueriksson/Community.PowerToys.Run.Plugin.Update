dotnet build .\src\Community.PowerToys.Run.Plugin.Update\ -c Release /p:Platform=x64 /p:TF_BUILD=true
dotnet build .\src\Community.PowerToys.Run.Plugin.Update\ -c Release /p:Platform=ARM64 /p:TF_BUILD=true
dotnet pack -c Release
