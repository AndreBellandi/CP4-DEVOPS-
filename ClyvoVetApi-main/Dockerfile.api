FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies to optimize Docker layer caching
COPY ["ClyvoVetApi.csproj", "./"]
RUN dotnet restore "ClyvoVetApi.csproj"

# Copy the remaining files and build
COPY . .
RUN dotnet build "ClyvoVetApi.csproj" -c Release -o /app/build
RUN dotnet publish "ClyvoVetApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the secure aspnet image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose non-privileged HTTP port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Enforce secure non-root user execution (DevOps Compliance)
USER app

ENTRYPOINT ["dotnet", "ClyvoVetApi.dll"]
