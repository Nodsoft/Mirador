FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Nodsoft.Mirador/Nodsoft.Mirador.csproj", "Nodsoft.Mirador/"]
RUN dotnet restore "Nodsoft.Mirador/Nodsoft.Mirador.csproj"
COPY . .
WORKDIR "/src/Nodsoft.Mirador"
RUN dotnet build "Nodsoft.Mirador.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Nodsoft.Mirador.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nodsoft.Mirador.dll"]
