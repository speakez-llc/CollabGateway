#!/bin/bash

curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 8.0 -InstallDir ./dotnet8
export PATH=./dotnet8:$PATH
dotnet tool restore
dotnet build
dotnet fable --run npx vite build