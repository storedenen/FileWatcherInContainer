echo "Starting filewatcher..."

if not exist C:\Temp\WatchDir (
  mkdir C:\Temp\WatchDir
)

dotnet build .
docker build -t filewatcher-image -f .\Dockerfile .
docker run -it --mount type=bind,source=/c/Temp/WatchDir,target=/appdata --rm filewatcher-image
