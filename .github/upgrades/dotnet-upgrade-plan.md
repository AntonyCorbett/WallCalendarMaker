# .NET 9.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 9.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 9.0 upgrade.
3. Upgrade WallCalendarMakerCore\WallCalendarMakerCore.csproj
4. Upgrade WallCalendarMaker\WallCalendarMaker.csproj
5. Run unit tests to validate upgrade in the projects listed below:
  (no test projects discovered)

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                        | Current Version | New Version | Description                         |
|:------------------------------------|:---------------:|:-----------:|:------------------------------------|
| Microsoft.Extensions.Hosting        | 7.0.1           | 9.0.9       | Recommended for .NET 9.0            |
| RestSharp                           | 110.2.0         | 112.1.0     | Security vulnerability              |

### Project upgrade details
This section contains details about each project upgrade and modifications that need to be done in the project.

#### WallCalendarMakerCore\WallCalendarMakerCore.csproj modifications

Project properties changes:
  - Target framework should be changed from `net7.0` to `net9.0`

NuGet packages changes:
  - (none)

Feature upgrades:
  - (none)

Other changes:
  - (none)

#### WallCalendarMaker\WallCalendarMaker.csproj modifications

Project properties changes:
  - Target framework should be changed from `net7.0` to `net9.0`

NuGet packages changes:
  - Microsoft.Extensions.Hosting should be updated from `7.0.1` to `9.0.9` (*recommended for .NET 9.0*)
  - RestSharp should be updated from `110.2.0` to `112.1.0` (*security vulnerability*)

Feature upgrades:
  - (none)

Other changes:
  - (none)
