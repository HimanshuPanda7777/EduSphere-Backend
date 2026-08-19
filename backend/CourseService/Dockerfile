FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CourseService/Course.API/Course.API.csproj", "CourseService/Course.API/"]
COPY ["CourseService/Course.Application/Course.Application.csproj", "CourseService/Course.Application/"]
COPY ["CourseService/Course.Domain/Course.Domain.csproj", "CourseService/Course.Domain/"]
COPY ["CourseService/Course.Infrastructure/Course.Infrastructure.csproj", "CourseService/Course.Infrastructure/"]
COPY ["shared/SharedKernel/SharedKernel.csproj", "shared/SharedKernel/"]
RUN dotnet restore "CourseService/Course.API/Course.API.csproj"
COPY . .
WORKDIR "/src/CourseService/Course.API"
RUN dotnet build "Course.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Course.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Course.API.dll"]
