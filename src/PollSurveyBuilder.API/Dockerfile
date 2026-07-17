# syntax=docker/dockerfile:1

# ---- base: slim runtime image the final stage copies into ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
USER $APP_UID

# ---- build: full SDK image, restores + builds ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files first so `dotnet restore` is cached as its own Docker layer -
# this is what keeps rebuilds fast when only .cs files change.
COPY ["src/PollSurveyBuilder.API/PollSurveyBuilder.API.csproj", "src/PollSurveyBuilder.API/"]
COPY ["src/PollSurveyBuilder.Infrastructure/PollSurveyBuilder.Infrastructure.csproj", "src/PollSurveyBuilder.Infrastructure/"]
COPY ["src/PollSurveyBuilder.Application/PollSurveyBuilder.Application.csproj", "src/PollSurveyBuilder.Application/"]
COPY ["src/PollSurveyBuilder.Domain/PollSurveyBuilder.Domain.csproj", "src/PollSurveyBuilder.Domain/"]
RUN dotnet restore "src/PollSurveyBuilder.API/PollSurveyBuilder.API.csproj"

COPY src/ src/
WORKDIR /src/src/PollSurveyBuilder.API
RUN dotnet build "PollSurveyBuilder.API.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# ---- publish: produces the trimmed output that actually ships ----
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PollSurveyBuilder.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---- final: only the aspnet runtime + published output, no SDK ----
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PollSurveyBuilder.API.dll"]
