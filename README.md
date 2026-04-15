# BuildingCostsDump — Farthest Frontier MelonLoader Mod

Dumps all building definitions (identifier, category, grid size, labor cost, construction materials, prerequisites) to text and CSV files so they can be imported into the [Farthest Frontier Planner](https://sagedragoon79.github.io/FarthestFrontierPlanner/).

## How it works

On game startup, the mod waits for `GlobalAssets.buildingSetupData` to initialize, then iterates every `BuildingData` entry and writes two files to `UserData/`:

- `BuildingCostsDump.txt` — human-readable table
- `BuildingCostsDump.csv` — spreadsheet-ready

Both files land in:
```
<Farthest Frontier install>\Farthest Frontier (Mono)\UserData\
```

## Build instructions

1. **Install .NET SDK** (6+ works fine for building net472):
   https://dotnet.microsoft.com/download

2. **Adjust game path in `BuildingCostsDump.csproj`** if your install is not at the default:
   ```xml
   <GameDir>G:\SteamLibrary\steamapps\common\Farthest Frontier\Farthest Frontier (Mono)</GameDir>
   ```

3. **Build**:
   ```
   cd BuildingCostsDumpMod
   dotnet build -c Release
   ```

4. **Copy** `bin\Release\BuildingCostsDump.dll` into:
   ```
   <Farthest Frontier install>\Farthest Frontier (Mono)\Mods\
   ```

5. **Launch the game**. The mod runs automatically and writes both dump files to `UserData/`.

6. **Share** the `.csv` back to the planner project — I'll import it and batch-update all building costs.

## What gets dumped

Per building:
- `identifier` — in-game ID (e.g. `market`, `shelter_tier2`)
- `category` — SHELTER, STORAGE, FOOD_PRODUCTION, etc.
- `gridSize` — width × height
- `workRequiredToConstruct` — labor cost
- `buildingMaterials` — list of `{item, quantity}` (Planks, Stone, Gold, Iron, etc.)
- `goldRequiredToRelocate`
- `workRequiredToDeconstruct`
- `prerequisiteIdentifiers` — unlock requirements
