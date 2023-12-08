FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Chatify.Web/Chatify.Web.csproj", "Chatify.Web/"]
COPY ["Chatify.Application/Chatify.Application.csproj", "Chatify.Application/"]
COPY ["Chatify.Domain/Chatify.Domain.csproj", "Chatify.Domain/"]
COPY ["Chatify.Shared.Abstractions/Chatify.Shared.Abstractions.csproj", "Chatify.Shared.Abstractions/"]
COPY ["Chatify.Shared.Infrastructure/Chatify.Shared.Infrastructure.csproj", "Chatify.Shared.Infrastructure/"]
COPY ["Chatify.Infrastructure/Chatify.Infrastructure.csproj", "Chatify.Infrastructure/"]
RUN dotnet restore "Chatify.Web/Chatify.Web.csproj"
COPY . .
WORKDIR "/src/Chatify.Web"
RUN dotnet build "Chatify.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Chatify.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV ASPNETCORE_URLS=http://+:8080
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chatify.Web.dll"]
