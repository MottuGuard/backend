# ===== Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["backend.csproj", "./"]
RUN dotnet restore "./backend.csproj"

COPY . .
RUN dotnet publish "backend.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV Jwt__Key="6bDCacaAFiuqVRY7mA9aGzlAjBsXn9d6tqTtsqyiO/M="
ENV ConnectionStrings__DefaultConnection="Data Source=oracle.fiap.com.br/orcl;User Id=rm555277;Password=160106;"

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "backend.dll"]
