echo "Starting filesystemwatchertrigger..."
dotnet build .
docker build -t filesystemwatchertrigger-image -f .\Dockerfile .
docker run -it --mount type=bind,source=/c/Temp/WatchDir,target=/appdata --rm filesystemwatchertrigger-image
