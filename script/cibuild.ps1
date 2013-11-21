$MSBuild = "C:\Program Files (x86)\MSBuild\12.0\bin\MSBuild.exe"

$SlnFileExists = Test-Path ".\Peasant.sln"
if ($SlnFileExists -eq $False) {
    echo "*** ERROR: Run this in the project root ***"
    exit -1
}

& "$MSBuild" /t:Rebuild /p:Configuration=Release .\Peasant.sln

$host.SetShouldExit($LastExitCode)
exit
