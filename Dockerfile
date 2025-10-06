FROM mcr.microsoft.com/dotnet/sdk:9.0 AS isketch-build
COPY . /isketch-build
WORKDIR /isketch-build
RUN dotnet restore
RUN dotnet publish -c release -p:CompressionEnabled=false -o /iSketch.app --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS isketch-app
RUN apt update && apt install curl --yes && rm -rf /var/lib/apt/lists/*
WORKDIR /iSketch.app
COPY --from=isketch-build /iSketch.app .
LABEL org.opencontainers.image.authors="support@belowaverage.org"
EXPOSE 80/tcp
ENV IS_SQL_Pass="iSketch.app"
ENV IS_SQL_ServerHost="localhost, 1433"
ENV IS_SQL_User="sa"
ENV IS_SQL_DatabaseName="iSketch.app"
HEALTHCHECK CMD curl --fail http://localhost/_health || exit 1
ENTRYPOINT ["./iSketch.app"]