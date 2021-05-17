FROM mcr.microsoft.com/dotnet/sdk:5.0 AS sFlowBuild
COPY . /sFlowBuild
WORKDIR /sFlowBuild
RUN dotnet restore
RUN dotnet publish -c release -o /iSketch.app --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /iSketch.app
COPY --from=sFlowBuild /iSketch.app .
ENV IS_SQL_Pass="iSketch.app"
ENV IS_SQL_ServerHost="10.0.5.8"
ENV IS_SQL_User="iSketch.app"
ENV IS_SQL_DatabaseName="iSketch.app"
ENTRYPOINT ./iSketch.app