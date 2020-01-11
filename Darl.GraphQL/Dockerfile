#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Darl.GraphQL/Darl.GraphQL.csproj", "Darl.GraphQL/"]
COPY ["Darl.GraphQL.Process/Darl.GraphQL.Process.csproj", "Darl.GraphQL.Process/"]
RUN dotnet restore "Darl.GraphQL/Darl.GraphQL.csproj"
COPY . .
WORKDIR "/src/Darl.GraphQL"
RUN dotnet build "Darl.GraphQL.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Darl.GraphQL.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Darl.GraphQL.dll"]