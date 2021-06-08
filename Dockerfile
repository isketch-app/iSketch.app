FROM mcr.microsoft.com/dotnet/sdk:5.0 AS iSketchBuild
COPY . /iSketchBuild
WORKDIR /iSketchBuild
RUN dotnet restore
RUN dotnet publish -c release -o /iSketch.app --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /iSketch.app
COPY --from=iSketchBuild /iSketch.app .
ENV IS_SQL_Pass="iSketch.app"
ENV IS_SQL_ServerHost="127.0.0.1"
ENV IS_SQL_User="iSketch.app"
ENV IS_SQL_DatabaseName="iSketch.app"
ENTRYPOINT ./iSketch.app
