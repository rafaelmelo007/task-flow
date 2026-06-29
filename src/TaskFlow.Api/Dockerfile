FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/TaskFlow.Domain/TaskFlow.Domain.csproj", "TaskFlow.Domain/"]
COPY ["src/TaskFlow.Application/TaskFlow.Application.csproj", "TaskFlow.Application/"]
COPY ["src/TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj", "TaskFlow.Infrastructure/"]
COPY ["src/TaskFlow.Api/TaskFlow.Api.csproj", "TaskFlow.Api/"]
RUN dotnet restore "TaskFlow.Api/TaskFlow.Api.csproj"

COPY src/ .
RUN dotnet publish "TaskFlow.Api/TaskFlow.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

RUN addgroup --system appgroup && adduser --system --ingroup appgroup --no-create-home appuser \
    && mkdir -p /data \
    && chown -R appuser:appgroup /app /data

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER appuser
ENTRYPOINT ["dotnet", "TaskFlow.Api.dll"]
