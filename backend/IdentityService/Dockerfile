FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["IdentityService/Identity.API/Identity.API.csproj", "IdentityService/Identity.API/"]
COPY ["IdentityService/Identity.Application/Identity.Application.csproj", "IdentityService/Identity.Application/"]
COPY ["IdentityService/Identity.Domain/Identity.Domain.csproj", "IdentityService/Identity.Domain/"]
COPY ["IdentityService/Identity.Infrastructure/Identity.Infrastructure.csproj", "IdentityService/Identity.Infrastructure/"]
COPY ["shared/SharedKernel/SharedKernel.csproj", "shared/SharedKernel/"]
RUN dotnet restore "IdentityService/Identity.API/Identity.API.csproj"
COPY . .
WORKDIR "/src/IdentityService/Identity.API"
RUN dotnet build "Identity.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Identity.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.API.dll"]
