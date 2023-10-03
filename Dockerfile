FROM mcr.microsoft.com/dotnet/sdk:8.0 AS iSketchBuild
COPY . /iSketchBuild
WORKDIR /iSketchBuild
RUN dotnet restore
RUN dotnet publish -c release -o /iSketch.app --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /iSketch.app
COPY --from=iSketchBuild /iSketch.app .
ENV IS_SQL_Pass="iSketch.app"
ENV IS_SQL_ServerHost="127.0.0.1, 11434"
ENV IS_SQL_User="sa"
ENV IS_SQL_DatabaseName="iSketch.app"
ENTRYPOINT ./iSketch.app
