echo "Starting filesystemwatchertrigger..."

if not exist C:\Temp\WatchDir (
  mkdir C:\Temp\WatchDir
)

dotnet build .
docker build -t filesystemwatchertrigger-image -f .\Dockerfile .
docker run -it --mount type=bind,source=/c/Temp/WatchDir,target=/appdata --rm filesystemwatchertrigger-image
 