FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as build

WORKDIR /app
COPY datems.csproj Program.cs ./
RUN dotnet publish -c Release -o build --self-contained=false

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine as final
RUN mkdir /app
COPY --from=build /app/build/* /app/

CMD [ "dotnet", "/app/datems.dll" ]