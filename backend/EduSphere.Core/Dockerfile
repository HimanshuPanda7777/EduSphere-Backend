FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["EduSphere.Core/Core.API/Core.API.csproj", "EduSphere.Core/Core.API/"]
COPY ["EduSphere.Core/Core.Application/Core.Application.csproj", "EduSphere.Core/Core.Application/"]
COPY ["EduSphere.Core/Core.Domain/Core.Domain.csproj", "EduSphere.Core/Core.Domain/"]
COPY ["EduSphere.Core/Core.Infrastructure/Core.Infrastructure.csproj", "EduSphere.Core/Core.Infrastructure/"]
COPY ["shared/SharedKernel/SharedKernel.csproj", "shared/SharedKernel/"]
RUN dotnet restore "EduSphere.Core/Core.API/Core.API.csproj"
COPY . .
WORKDIR "/src/EduSphere.Core/Core.API"
RUN dotnet build "Core.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Core.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Core.API.dll"]
