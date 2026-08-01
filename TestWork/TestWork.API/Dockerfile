FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["TestWork.API/TestWork.API.csproj", "TestWork.API/"]
COPY ["TestWork.Application/TestWork.Application.csproj", "TestWork.Application/"]
COPY ["TestWork.Core/TestWork.Core.csproj", "TestWork.Core/"]
COPY ["TestWork.Infrastructure/TestWork.Infrastructure.csproj", "TestWork.Infrastructure/"]

RUN dotnet restore "TestWork.API/TestWork.API.csproj"

COPY . .
WORKDIR "/src/TestWork.API"
RUN dotnet publish "TestWork.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TestWork.API.dll"]

