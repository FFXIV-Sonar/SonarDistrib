# SonarDistrib
SonarPlugin's Source Code mirror and all dependencies.
- [Sonar News](https://news.ffxivsonar.com)
- [Sonar Support Discord](https://discord.gg/K7y24Rr)
- [Sonar Patreon](https://www.patreon.com/ffxivsonar)

## Building
To build SonarPlugin, you can:
- Open `Sonar.sln` solution and compile the `SonarPlugin` project
- Alternatively: `dotnet build -c Release SonarPlugin/SonarPlugin.csproj`

## Support
Support is provided exclusively at the [Sonar Support Discord](https://discord.gg/K7y24Rr)
Issues and Pull Requests are welcome, however I tend not to check often (even with email notifications! :sob:)
If you need immediate attention join the Discord

## Components
The sonar source code is contained as multiple components

### SonarPlugin
The plugin part of Sonar. This is what's installed when installing Sonar under Dalamud.

### Sonar
Shared library providing the client, core functionality, communiucations, shared functionality and structures of Sonar. This includes a database containing information about Hunts, Fates, Zones, Worlds and Data Centers.

### SonarResources
Generate the resource files for Sonar to use, which contains hunts, fates and maps information.

### SonarServer
The server side of Sonar, performing the heavywork of receiving and broadcasting relays for every Sonar client. 
This is not included in this repo.

### SonarUtils
Utility code shared by all Sonar components.

### SonarUtils.Tests
Contains tests for SonarUtils.