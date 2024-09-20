#!/bin/bash

curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh; 
chmod +x dotnet-install.sh; 
./dotnet-install.sh -c 8.0 -InstallDir ~/dotnet8; 
export PATH=$PATH:~/dotnet8; 
dotnet tool restore; 
dotnet build -c Release; 