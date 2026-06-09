# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["ToDoApi/ToDoApi.csproj", "ToDoApi/"]
RUN dotnet restore "ToDoApi/ToDoApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ToDoApi"
RUN dotnet build "ToDoApi.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "ToDoApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ToDoApi.dll"]
