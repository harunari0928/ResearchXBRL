FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Presentation/ReverseLookupAccountItems/ReverseLookupAccountItems.csproj", "Presentation/ReverseLookupAccountItems/"]
COPY ["ResearchXBRL.Infrastructure/ResearchXBRL.Infrastructure.csproj", "ResearchXBRL.Infrastructure/"]
COPY ["ResearchXBRL.Application/ResearchXBRL.Application.csproj", "ResearchXBRL.Application/"]
COPY ["ResearchXBRL.Domain/ResearchXBRL.Domain.csproj", "ResearchXBRL.Domain/"]
COPY ["ResearchXBRL.CrossCuttingInterest/ResearchXBRL.CrossCuttingInterest.csproj", "ResearchXBRL.CrossCuttingInterest/"]
RUN dotnet restore "Presentation/ReverseLookupAccountItems/ReverseLookupAccountItems.csproj"
COPY . .
WORKDIR "/src/Presentation/ReverseLookupAccountItems"
RUN dotnet build "ReverseLookupAccountItems.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReverseLookupAccountItems.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReverseLookupAccountItems.dll"]