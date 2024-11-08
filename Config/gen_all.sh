#!/bin/bash

# 检查是否安装了 dotnet
if ! command -v dotnet &> /dev/null; then
    echo ".NET未安装，前往 https://dotnet.microsoft.com/zh-cn/download 下载安装后再试。"
    exit 0
fi

# 获取当前版本
VERSION=$(dotnet --version)

# 判断是否为 8.0
echo "当前.NET版本为 $VERSION"
if [[ ${VERSION:0:3} != "8.0" ]]; then
    echo "当前版本不是 8.0，前往 https://dotnet.microsoft.com/zh-cn/download 下载安装后再试。"
    exit 0
fi

cd Tools/ || exit
echo "**********Generating Server==========Generating Server**********"
echo "**********Generating Server==========Generating Server**********"
echo "**********Generating Server==========Generating Server**********"
bash gen_server.sh
echo "**********Generating Client==========Generating Client**********"
echo "**********Generating Client==========Generating Client**********"
echo "**********Generating Client==========Generating Client**********"
bash gen_client.sh
