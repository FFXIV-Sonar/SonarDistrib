# SonarDistrib
SonarPlugin's Source Code mirror and all dependencies.

## Building
To build SonarPlugin, you can:
- Open `Sonar.sln` solution and compile the `SonarPlugin` project
- Alternatively: `dotnet build -c Release SonarPlugin/SonarPlugin.csproj`

## Components
### SonarPlugin
The plugin part of Sonar. This is what's installed when installing Sonar under Dalamud.

### Sonar
Shared library providing the client, core functionality, communiucations and shared structures of Sonar. This includes a database containing information about Hunts, Fates, Zones, Worlds and Data Centers.
This is not included in this repo.

### SonarServer
The server side of Sonar, performing the heavywork of receiving and broadcasting relays for every Sonar client. 
This is not included in this repo.

