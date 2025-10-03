# .NET 9 Upgrade Report

## Project target framework modifications

| Project name                                           | Old Target Framework | New Target Framework | Commits   |
|:-------------------------------------------------------|:--------------------:|:--------------------:|:----------|
| WallCalendarMakerCore\WallCalendarMakerCore.csproj     | net7.0               | net9.0               | 468fb9bc  |
| WallCalendarMaker\WallCalendarMaker.csproj             | net7.0               | net9.0               | d37712e8  |

## NuGet Packages

| Package Name                  | Old Version | New Version | Commit Id |
|:------------------------------|:-----------:|:-----------:|:----------|
| Microsoft.Extensions.Hosting  | 7.0.1       | 9.0.9       | 8e7000f7  |
| RestSharp                     | 110.2.0     | 112.1.0     | 8e7000f7  |

## All commits

| Commit ID  | Description                                  |
|:-----------|:---------------------------------------------|
| b6a7bc17   | Commit upgrade plan                          |
| 468fb9bc   | Update WallCalendarMakerCore.csproj to target .NET 9.0 |
| d37712e8   | Update WallCalendarMaker.csproj to target .NET 9.0     |
| 8e7000f7   | Update package versions in WallCalendarMaker.csproj    |

## Project feature upgrades

(No feature-specific upgrades were required or detected.)

## Next steps

- Run a full solution build and execute any external or integration tests you may have.
- Verify runtime behavior in staging.
- Consider adding or updating a global.json to lock to a .NET 9 SDK version if desired.
- Monitor for any deprecations when moving to .NET 10 preview later.

## Token usage and cost

Model token usage and cost tracking not available in this environment.
