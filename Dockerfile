FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . .


RUN dotnet restore "DScannerWebApp/RCommerce.WebApp.csproj"
RUN dotnet publish "DScannerWebApp/RCommerce.WebApp.csproj" -c Release -o /out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000

ENTRYPOINT ["dotnet", "RCommerce.WebApp.dll"]
