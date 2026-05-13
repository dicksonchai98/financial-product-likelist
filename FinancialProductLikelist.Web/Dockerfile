FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FinancialProductLikelist.Web/FinancialProductLikelist.csproj FinancialProductLikelist.Web/
RUN dotnet restore FinancialProductLikelist.Web/FinancialProductLikelist.csproj

COPY . .
RUN dotnet publish FinancialProductLikelist.Web/FinancialProductLikelist.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FinancialProductLikelist.dll"]
