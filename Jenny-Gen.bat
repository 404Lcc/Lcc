pushd %~dp0
dotnet Jenny/Jenny.Generator.Cli.dll gen -v
pause
popd
