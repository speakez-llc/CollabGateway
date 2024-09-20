#!/bin/bash

# Install .NET SDK
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 8.0 -InstallDir ./dotnet8
export PATH=./dotnet8:$PATH

# Restore tools and build the project
dotnet tool restore
dotnet build

# Run Fable and Vite build
dotnet fable --run npx vite build