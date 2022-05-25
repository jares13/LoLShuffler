#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["LoLShuffler/LoLShuffler.csproj", "LoLShuffler/"]
COPY ["LoLSuffler.DAL/LoLShuffler.DAL.csproj", "LoLSuffler.DAL/"]
RUN dotnet restore "LoLShuffler/LoLShuffler.csproj"
COPY . .
WORKDIR "/src/LoLShuffler"
RUN dotnet build "LoLShuffler.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "LoLShuffler.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LoLShuffler.dll"]