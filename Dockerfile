# Student:     Mandlakazi Sibiya (ST10473547)
# Module:      PROG6212 Programming 2B - PoE Part 3
# Institution: The Independent Institute of Education (IIE)
# Date:        19 November 2026
# AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
# Reference:   Microsoft Learn, n.d. Tutorial: Containerize a .NET app. [online] Available at:
#              https://learn.microsoft.com/dotnet/core/docker/build-container [Accessed 19 November 2026].
#              (That tutorial uses a console app; the multi-stage pattern below is the same for ASP.NET Core.)

# A multi-stage build: the first stage has the big .NET SDK and builds the app,
# the second stage has only the small ASP.NET runtime and the finished app. The final image stays small.

# ---------- Stage 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project file first and restore the packages.
# Docker remembers this step, so a rebuild after a code change does not download everything again.
COPY ["RaceDay.Web/RaceDay.Web.csproj", "./"]
RUN dotnet restore "RaceDay.Web.csproj"

# Now copy the rest of the code and publish a Release version
COPY . ./
RUN dotnet publish "RaceDay.Web.csproj" -c Release -o /app/publish --no-restore

# ---------- Stage 2: run ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# The app listens on port 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./

# Run as the built-in non-root user, not as an administrator (a basic security habit)
USER $APP_UID

ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]


