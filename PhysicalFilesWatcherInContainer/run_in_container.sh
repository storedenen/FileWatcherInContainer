#!/bin/bash
echo "Starting physical filewatcher..."

[ ! -d "/tmp/WatchDir" ] && mkdir /tmp/WatchDir

dotnet build .
docker build -t physicalfilewatcher-image -f Dockerfile .
docker run -it --mount type=bind,source=/tmp/WatchDir,target=/appdata --rm physicalfilewatcher-image
