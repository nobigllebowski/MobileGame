# Map source data

Source: **Natural Earth**, version 5.2.0-pre, 1:50m scale.
Files:

- `ne_50m_admin_0_countries.geojson` — admin-0 countries (242 features) with ISO codes, names in 20+ languages, population and GDP estimates, label anchors.
- `ne_50m_populated_places.geojson` — populated places including every admin-0 capital.

Downloaded from the official GitHub mirror: https://github.com/nvkelso/natural-earth-vector (`geojson/` folder).

## License

Natural Earth is in the **public domain**. From naturalearthdata.com: "All versions of Natural Earth raster + vector map data found on this website are in the public domain. You may use the maps in any manner, including modifying the content and design, electronically displaying, and offline printing. No permission is needed to use Natural Earth. Crediting the authors is unnecessary." A credit line is still shown in the game's settings out of courtesy.

## Pipeline

These files are not imported by Unity (they live outside `Assets/`). They feed the import pipeline:

```
Tools/MapSource/*.geojson
        ↓  Nation.Core.Map.Import.GeoJsonReader
IMPORT  ↓  NaturalEarthMapping (ISO3 resolution, territory type, capitals)
VALIDATE↓  MapValidator (missing / duplicate ids, invalid polygons, capitals outside)
PROCESS ↓  EqualEarthProjection → PolygonSimplifier (3 zoom bands) → EarClipTriangulator
GENERATE↓  MapCatalogSerializer → Assets/Data/Map/world.map.bytes
           name tables → Assets/Data/Localization/map-names.<locale>.json
           report → Assets/Data/Map/import-report.txt
```

Run it from Unity with **Nation → Map → Import Natural Earth** (Editor menu) or from the command line with
`Tools/MapImport/run.sh` (Mono or .NET). Both call the same engine-free `MapBuildPipeline`, so the output is identical.

## Country identifiers

- ISO 3166-1 alpha-3 from `ISO_A3_EH` (falls back to `ISO_A3`).
- Territories without an ISO code keep Natural Earth's `ADM0_A3` code and are flagged `IsIsoCode = false`
  (Kosovo KOS, Somaliland SOL, Northern Cyprus CYN, Siachen KAS, Indian Ocean Territories IOA, Ashmore ATC).
- Dependencies keep their own code and point to their sovereign through `SovereignId` (`SOV_A3`).
- Capitals: the `Admin-0 capital` populated place with `ADM0_A3` equal to the country id. Multi-capital
  countries use the seat of government listed in `NaturalEarthMapping.CapitalOverrides`.

## Polygon conversion

Only outer rings are kept; holes are not carved. Countries that lie inside another (Lesotho, San Marino,
Vatican City) are drawn above their host because the renderer sorts by area, which gives the same picture at a
fraction of the triangulation cost. Antarctica is included as a non-selectable territory.
