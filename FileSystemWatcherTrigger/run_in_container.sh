#!/bin/bash
echo "Starting filewatcher..."

[ ! -d "/tmp/WatchDir" ] && mkdir /tmp/WatchDir

dotnet build .
docker build -t filesystemwatchertrigger-image -f Dockerfile .
docker run -it --mount type=bind,source=/tmp/WatchDir,target=/appdata --rm filesystemwatchertrigger-image
