FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Presentation/AquireFinancialReports/AquireFinancialReports.csproj", "Presentation/AquireFinancialReports/"]
COPY ["ResearchXBRL.Infrastructure/ResearchXBRL.Infrastructure.csproj", "ResearchXBRL.Infrastructure/"]
COPY ["ResearchXBRL.Application/ResearchXBRL.Application.csproj", "ResearchXBRL.Application/"]
COPY ["ResearchXBRL.Domain/ResearchXBRL.Domain.csproj", "ResearchXBRL.Domain/"]
COPY ["ResearchXBRL.CrossCuttingInterest/ResearchXBRL.CrossCuttingInterest.csproj", "ResearchXBRL.CrossCuttingInterest/"]
RUN dotnet restore "Presentation/AquireFinancialReports/AquireFinancialReports.csproj"
COPY . .
WORKDIR "/src/Presentation/AquireFinancialReports"
RUN dotnet build "AquireFinancialReports.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AquireFinancialReports.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AquireFinancialReports.dll"]