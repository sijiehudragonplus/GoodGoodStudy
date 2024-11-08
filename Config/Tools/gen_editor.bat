WORKSPACE=..
OUTPUT=%WORKSPACE%\Output\editor
CONFIG=%WORKSPACE%\Input\luban.conf
DLL=%WORKSPACE%\Tools\Luban\Luban.dll

dotnet %DLL% ^
    -t editor ^
    -c cs-simple-json ^
    -d json ^
    --conf %CONFIG% ^
    -x outputCodeDir=%OUTPUT%\code ^
    -x outputDataDir=%OUTPUT%\data ^
    -x codeStyle=csharp-default