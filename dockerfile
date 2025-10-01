FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["backend.csproj", "./"]
RUN dotnet restore "./backend.csproj"
COPY . .
RUN dotnet publish "backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_ENVIRONMENT=Production \
    Jwt__Key=uqW8EXYt+3WOsDntgbG5Jt68rNTMmKZwpawNRcMIkSY=

COPY --from=build /app/publish .

USER app

EXPOSE 8080
ENTRYPOINT ["dotnet", "backend.dll"]
