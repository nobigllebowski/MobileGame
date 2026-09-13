#!/usr/bin/env bash
# Builds Nation.Core and the importer with Mono's mcs and regenerates Assets/Data/Map/world.map.bytes.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
OUT="$ROOT/Tools/MapImport/build"
mkdir -p "$OUT"
mcs -langversion:latest -target:library -out:"$OUT/Nation.Core.dll" -recurse:"$ROOT/Assets/Scripts/Core/*.cs"
mcs -langversion:latest -target:exe -out:"$OUT/MapImportCli.exe" -r:"$OUT/Nation.Core.dll" -r:System.Core "$ROOT/Tools/MapImport/MapImportCli.cs"
mono "$OUT/MapImportCli.exe" "$ROOT"
