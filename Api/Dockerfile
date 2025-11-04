# Dockerfile cho .NET API Server
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5050

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ThongKe/ThongKe.csproj", "ThongKe/"]
RUN dotnet restore "ThongKe/ThongKe.csproj"
COPY . .
WORKDIR "/src/ThongKe"
RUN dotnet build "ThongKe.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ThongKe.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the build stage as the default target for migrations
# This provides access to the SDK and source code needed for EF Core tools
FROM build AS development
WORKDIR /src/ThongKe
# Install EF Core global tool
RUN dotnet tool install --global dotnet-ef --version 9.0.0
ENV PATH="$PATH:/root/.dotnet/tools"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ThongKe.dll"]