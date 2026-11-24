FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 3001

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://*:3001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG GH_PAT
ARG PROJECT_API_ID
ARG ALIAS
ARG CONTENSIS_CLIENT_ID
ARG CONTENSIS_CLIENT_SECRET
ARG ACCESS_TOKEN

ENV GH_PAT=${GH_PAT} \
    PROJECT_API_ID=${PROJECT_API_ID} \
    ALIAS=${ALIAS} \
    CONTENSIS_CLIENT_ID=${CONTENSIS_CLIENT_ID} \
    CONTENSIS_CLIENT_SECRET=${CONTENSIS_CLIENT_SECRET} \
    ACCESS_TOKEN=${ACCESS_TOKEN}

WORKDIR /src

COPY RazorPageBlackpoolWebsite/*.csproj ./RazorPageBlackpoolWebsite/

RUN dotnet nuget add source \
    --name github \
    --username asifblackpool \
    --password $GH_PAT \
    --store-password-in-clear-text \
    https://nuget.pkg.github.com/asifblackpool/index.json

RUN dotnet nuget list source

WORKDIR /src/RazorPageBlackpoolWebsite

RUN dotnet restore

COPY RazorPageBlackpoolWebsite/. .

# ── Removed --runtime linux-amd64 (base image already targets linux/amd64)
# ── Added -p:PublishReadyToRun=false to avoid precompiling native images (saves size)
RUN dotnet publish \
    --self-contained false \
    -o /app/publish \
    -p:RazorCompileOnBuild=true \
    -p:RazorCompileOnPublish=true \
    -p:PublishReadyToRun=false

FROM base AS final
WORKDIR /app

ARG PROJECT_API_ID
ARG ALIAS
ARG CONTENSIS_CLIENT_ID
ARG CONTENSIS_CLIENT_SECRET
ARG ACCESS_TOKEN

ENV PROJECT_API_ID=${PROJECT_API_ID} \
    ALIAS=${ALIAS} \
    CONTENSIS_CLIENT_ID=${CONTENSIS_CLIENT_ID} \
    CONTENSIS_CLIENT_SECRET=${CONTENSIS_CLIENT_SECRET} \
    ACCESS_TOKEN=${ACCESS_TOKEN} \
    ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
COPY manifest.json /manifest.json

# NOTE: removed the .env writing block — secrets should come from
# Contensis block environment variables, not baked into the image.

ENTRYPOINT ["dotnet", "RazorPageBlackpoolWebsite.dll"]