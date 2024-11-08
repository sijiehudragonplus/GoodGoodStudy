#!/bin/bash

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

WORKSPACE=..
OUTPUT=${WORKSPACE}/Output/server
CONFIG=${WORKSPACE}/Input/luban.conf
DLL=${WORKSPACE}/Tools/Luban/Luban.dll

dotnet ${DLL} \
    -t server \
    -c go-json \
    -d json \
    --conf ${CONFIG} \
    -x outputCodeDir=${OUTPUT}/code \
    -x outputDataDir=${OUTPUT}/data \
    -x codeStyle=go-default \
    -x lubanGoModule=$WORKSPACE/Tools/luban

