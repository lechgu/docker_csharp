FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine

WORKDIR /app
COPY datems.csproj Program.cs ./
RUN dotnet publish -c Release -o build --self-contained=false
CMD [ "dotnet", "/app/build/date.dll" ]