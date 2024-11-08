WORKSPACE=..
OUTPUT=%WORKSPACE%\Output\client
CONFIG=%WORKSPACE%\Input\luban.conf
DLL=%WORKSPACE%\Tools\Luban\Luban.dll

dotnet %DLL% ^
    -t client ^
    -c cs-bin ^
    -d bin ^
    --conf %CONFIG% ^
    -x outputCodeDir=%OUTPUT%\code ^
    -x outputDataDir=%OUTPUT%\data ^
    -x codeStyle=csharp-default

dotnet %DLL% ^
    -t client ^
    -d json ^
    --conf %CONFIG% ^
    -x outputDataDir=%OUTPUT%\data_dev_diff ^