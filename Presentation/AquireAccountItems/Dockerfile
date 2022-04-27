FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Presentation/AquireAccountItems/AquireAccountItems.csproj", "Presentation/AquireAccountItems/"]
COPY ["ResearchXBRL.Infrastructure/ResearchXBRL.Infrastructure.csproj", "ResearchXBRL.Infrastructure/"]
COPY ["ResearchXBRL.Application/ResearchXBRL.Application.csproj", "ResearchXBRL.Application/"]
COPY ["ResearchXBRL.Domain/ResearchXBRL.Domain.csproj", "ResearchXBRL.Domain/"]
COPY ["ResearchXBRL.CrossCuttingInterest/ResearchXBRL.CrossCuttingInterest.csproj", "ResearchXBRL.CrossCuttingInterest/"]
RUN dotnet restore "Presentation/AquireAccountItems/AquireAccountItems.csproj"
COPY . .
WORKDIR "/src/Presentation/AquireAccountItems"
RUN dotnet build "AquireAccountItems.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AquireAccountItems.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AquireAccountItems.dll"]