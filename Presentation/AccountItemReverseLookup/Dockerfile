#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Presentation/AccountItemReverseLookup/AccountItemReverseLookup.csproj", "Presentation/AccountItemReverseLookup/"]
COPY ["ResearchXBRL.Infrastructure/ResearchXBRL.Infrastructure.csproj", "ResearchXBRL.Infrastructure/"]
COPY ["ResearchXBRL.Application/ResearchXBRL.Application.csproj", "ResearchXBRL.Application/"]
COPY ["ResearchXBRL.Domain/ResearchXBRL.Domain.csproj", "ResearchXBRL.Domain/"]
COPY ["ResearchXBRL.CrossCuttingInterest/ResearchXBRL.CrossCuttingInterest.csproj", "ResearchXBRL.CrossCuttingInterest/"]
RUN dotnet restore "Presentation/AccountItemReverseLookup/AccountItemReverseLookup.csproj"
COPY . .
WORKDIR "/src/Presentation/AccountItemReverseLookup"
RUN dotnet build "AccountItemReverseLookup.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AccountItemReverseLookup.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AccountItemReverseLookup.dll"]