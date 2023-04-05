FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ClipboardService/ClipboardService.csproj", "ClipboardService/"]
RUN dotnet restore "ClipboardService/ClipboardService.csproj"
COPY . .
WORKDIR "/src/ClipboardService"
RUN dotnet build "ClipboardService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClipboardService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClipboardService.dll"]
