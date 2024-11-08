#!/bin/bash

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

WORKSPACE=..
OUTPUT=${WORKSPACE}/Output/client
CONFIG=${WORKSPACE}/Input/luban.conf
DLL=${WORKSPACE}/Tools/Luban/Luban.dll
#TEMP=${WORKSPACE}/Input/CustomTemplate/LazyLoad
#--customTemplateDir ${TEMP} \

# 生成项目实际使用的二进制格式代码及数据
dotnet ${DLL} \
    -t client \
    -c cs-bin \
    -d bin \
    --conf ${CONFIG} \
    -x outputCodeDir=${OUTPUT}/code \
    -x outputDataDir=${OUTPUT}/data \
    -x codeStyle=csharp-default

# 生成用于策划提交时查看对比的json数据
dotnet ${DLL} \
    -t client \
    -d json \
    --conf ${CONFIG} \
    -x outputDataDir=${OUTPUT}/data_dev_diff \