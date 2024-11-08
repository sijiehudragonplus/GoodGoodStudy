#!/bin/bash

#同步配置到美术库

# 定义要检查的文件夹路径
folder="../../CardRPG_Res"

# 检查文件夹是否存在
if [ -d "$folder" ]; then
    # 如果文件夹存在
    echo "Folder Res."
else
	exit
fi

rsync -a --delete ../Output/client/data/ ${folder}/CardRPG/Assets/Config/Gameplay
echo "Sys Success."