FROM mcr.microsoft.com/dotnet/sdk:9.0 AS isketch-build
COPY . /isketch-build
WORKDIR /isketch-build
RUN dotnet restore
RUN dotnet publish -c release -o /iSketch.app --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS isketch-app
LABEL org.opencontainers.image.authors="support@belowaverage.org"
WORKDIR /iSketch.app
COPY --from=isketch-build /iSketch.app .
RUN apt update
RUN apt install curl --yes
EXPOSE 8080/tcp
HEALTHCHECK CMD curl --fail http://localhost:8080/_health || exit 1
ENV IS_SQL_Pass="iSketch.app"
ENV IS_SQL_ServerHost="localhost, 1433"
ENV IS_SQL_User="sa"
ENV IS_SQL_DatabaseName="iSketch.app"
ENTRYPOINT ["./iSketch.app"]