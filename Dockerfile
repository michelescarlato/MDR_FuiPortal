# syntax=docker/dockerfile:1

############################
# 1) Build / Publish stage #
############################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution + csproj first for better layer caching
COPY *.sln ./
COPY Server/*.csproj ./Server/
COPY Client/*.csproj ./Client/
COPY Shared/*.csproj ./Shared/

# Restore (Server references the others, so restoring Server is enough)
RUN dotnet restore ./Server/MDR_FuiPortal.Server.csproj

# Copy the rest of the source
COPY . .

# Publish the Server (includes Client static files in hosted WASM setups)
RUN dotnet publish ./Server/MDR_FuiPortal.Server.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

########################
# 2) Runtime stage     #
########################
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Optional: run as non-root (recommended)
# The aspnet image doesn't always ship with a non-root user, so create one.
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

# Kestrel will listen on 8080 by default here
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "MDR_FuiPortal.Server.dll"]
