#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Darl.GraphQL/Darl.GraphQL.csproj", "Darl.GraphQL/"]
COPY ["Darl.GraphQL.Process/Darl.GraphQL.Process.csproj", "Darl.GraphQL.Process/"]
COPY ["Darl.ai/Darl.ai/Darl.ai.csproj", "Darl.ai/Darl.ai/"]
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