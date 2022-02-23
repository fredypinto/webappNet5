FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY . ./
RUN dotnet restore "/app/Evertec.Automation.Net.CoreWalletClient/Evertec.Automation.Net.CoreWalletClient.csproj"

RUN dotnet publish "/app/Evertec.Automation.Net.CoreWalletClient/Evertec.Automation.Net.CoreWalletClient.csproj" -c Release -o publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/publish .
ENTRYPOINT ["dotnet", "Evertec.Automation.Net.CoreWalletClient.dll"]