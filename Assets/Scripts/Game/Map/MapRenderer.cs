using System;
using System.Collections.Generic;
using System.Diagnostics;
using Nation.Core.Map;
using UnityEngine;

namespace Nation.Game.Map
{
    /// <summary>
    /// Owns every GameObject of the map: ocean, graticule, three land meshes, three border meshes and the
    /// selection highlight. Only the active zoom band is enabled. There are no per-country objects and no
    /// Update loop; everything changes through explicit calls from the camera, layer and selection systems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MapRenderer : MonoBehaviour
    {
        private const float OceanZ = 1f;
        private const float GraticuleZ = 0.5f;
        private const float BorderZ = -0.15f;
        private const float SelectionFillZ = -0.2f;
        private const float SelectionGlowZ = -0.22f;
        private const float SelectionLineZ = -0.25f;

        private static readonly float[] BorderHalfWidth = { 0.0025f, 0.0012f, 0.0005f };

        private MapService _service;
        private readonly MapMeshBuilder.LandResult[] _land = new MapMeshBuilder.LandResult[3];
        private readonly GameObject[] _landObjects = new GameObject[3];
        private readonly GameObject[] _borderObjects = new GameObject[3];
        private readonly Mesh[] _borderMeshes = new Mesh[3];
        private List<MapCountry> _byArea;
        private Material _landMaterial;
        private Material _selectedLandMaterial;
        private Material _borderMaterial;
        private Material _glowMaterial;
        private GameObject _selectionFill;
        private GameObject _selectionGlow;
        private GameObject _selectionLine;
        private MapZoomBand _band = MapZoomBand.Far;
        private string _selectedId;
        private Func<MapCountry, Color> _colorFor;

        public MapZoomBand Band => _band;
        public bool IsBuilt { get; private set; }

        public void Initialize(MapService service)
        {
            _service = service;
            var stopwatch = Stopwatch.StartNew();

            _byArea = new List<MapCountry>(service.Catalog.Countries);
            _byArea.Sort((a, b) => b.Area.CompareTo(a.Area));

            _landMaterial = CreateMaterial("Nation/MapLand", "Sprites/Default");
            _borderMaterial = CreateMaterial("Nation/MapLine", "Sprites/Default");
            _glowMaterial = CreateMaterial("Nation/MapLine", "Sprites/Default");
            _landMaterial.SetFloat("_Dim", 1f);

            // The selected country keeps its own material so dimming the rest of the world never dims it.
            _selectedLandMaterial = CreateMaterial("Nation/MapLand", "Sprites/Default");
            _selectedLandMaterial.SetFloat("_Dim", 1f);

            var scale = MapCameraController.Scale;
            var bounds = service.Catalog.Bounds;

            var ocean = CreateObject("Ocean", MapMeshBuilder.BuildQuad(bounds.Width * scale * 3f, bounds.Height * scale * 3f, OceanZ, "Ocean"), CreateMaterial("Nation/MapOcean", "Sprites/Default"));
            ocean.transform.localPosition = new Vector3(bounds.CenterX * scale, bounds.CenterY * scale, 0f);
            var oceanMaterial = ocean.GetComponent<MeshRenderer>().sharedMaterial;
            oceanMaterial.SetColor("_Color", MapPalette.Ocean);
            oceanMaterial.SetColor("_DeepColor", MapPalette.OceanDeep);
            oceanMaterial.SetVector("_Extent", new Vector4(bounds.Width * scale, bounds.Height * scale, bounds.CenterX * scale, bounds.CenterY * scale));

            CreateObject("Graticule", MapMeshBuilder.BuildGraticule(scale, 0.0012f, MapPalette.Graticule, GraticuleZ), _borderMaterial);

            for (var band = 0; band < 3; band++)
            {
                var kind = (MapZoomBand)band;
                _land[band] = MapMeshBuilder.BuildLand(_byArea, kind, scale);
                _landObjects[band] = CreateObject("Land_" + kind, _land[band].Mesh, _landMaterial);
                var borderColor = band == 0 ? MapPalette.BorderFar : MapPalette.Border;
                _borderMeshes[band] = MapMeshBuilder.BuildBorders(_byArea, kind, scale, BorderHalfWidth[band], borderColor, BorderZ);
                _borderObjects[band] = CreateObject("Borders_" + kind, _borderMeshes[band], _borderMaterial);
            }

            SetBand(MapZoomBand.Far, true);
            IsBuilt = true;
            service.Stats.MeshBuildMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            service.Stats.DrawObjects = 4;
        }

        public void SetBand(MapZoomBand band, bool force = false)
        {
            if (!force && band == _band)
            {
                return;
            }

            _band = band;
            for (var i = 0; i < 3; i++)
            {
                var active = i == (int)band;
                _landObjects[i].SetActive(active);
                _borderObjects[i].SetActive(active);
            }

            _service.Stats.Band = band;
            _service.Stats.VisibleVertices = _land[(int)band].Mesh.vertexCount + _borderMeshes[(int)band].vertexCount;
            _service.Stats.VisibleTriangles = _service.Catalog.TotalTriangles(band);
            RebuildSelection();
        }

        /// <summary>Recolors every band's land mesh through the vertex color arrays; no geometry is touched.</summary>
        public void SetColors(Func<MapCountry, Color> colorFor)
        {
            _colorFor = colorFor;
            for (var band = 0; band < 3; band++)
            {
                var land = _land[band];
                for (var r = 0; r < land.Ranges.Count; r++)
                {
                    var range = land.Ranges[r];
                    var color = (Color32)colorFor(_byArea[r]);
                    for (var v = 0; v < range.Count; v++)
                    {
                        land.Colors[range.Start + v] = color;
                    }
                }

                land.Mesh.SetColors(land.Colors);
            }
        }

        public void SetSelected(string iso3)
        {
            _selectedId = iso3;
            _landMaterial.SetFloat("_Dim", iso3 == null ? 1f : 0.72f);
            RebuildSelection();
        }

        private void RebuildSelection()
        {
            DestroySelection();
            if (_selectedId == null || !_service.Catalog.TryGet(_selectedId, out var country))
            {
                _service.Stats.DrawObjects = 4;
                return;
            }

            var scale = MapCameraController.Scale;
            var band = _band == MapZoomBand.Far ? MapZoomBand.Mid : _band;
            var baseColor = _colorFor != null ? _colorFor(country) : MapPalette.LandSimulated;
            var fillColor = Color.Lerp(baseColor, MapPalette.Selection, 0.35f);
            fillColor.a = 1f;

            var lineWidth = BorderHalfWidth[(int)_band] * 2.2f;
            _selectionFill = CreateObject("Selection_Fill", MapMeshBuilder.BuildFill(country, band, scale, fillColor, SelectionFillZ), _selectedLandMaterial);
            _selectionGlow = CreateObject("Selection_Glow", MapMeshBuilder.BuildOutline(country, band, scale, lineWidth * 3.2f, MapPalette.SelectionGlow, SelectionGlowZ), _glowMaterial);
            _selectionLine = CreateObject("Selection_Line", MapMeshBuilder.BuildOutline(country, band, scale, lineWidth, MapPalette.Selection, SelectionLineZ), _borderMaterial);
            _service.Stats.DrawObjects = 7;
        }

        private void DestroySelection()
        {
            DestroyWithMesh(ref _selectionFill);
            DestroyWithMesh(ref _selectionGlow);
            DestroyWithMesh(ref _selectionLine);
        }

        /// <summary>Selection meshes are generated per selection, so they must be released with their object.</summary>
        private static void DestroyWithMesh(ref GameObject target)
        {
            if (target == null)
            {
                return;
            }

            var filter = target.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null)
            {
                Destroy(filter.sharedMesh);
            }

            Destroy(target);
            target = null;
        }

        private void OnDestroy()
        {
            DestroySelection();
            for (var band = 0; band < 3; band++)
            {
                if (_land[band] != null && _land[band].Mesh != null) Destroy(_land[band].Mesh);
                if (_borderMeshes[band] != null) Destroy(_borderMeshes[band]);
            }

            if (_landMaterial != null) Destroy(_landMaterial);
            if (_selectedLandMaterial != null) Destroy(_selectedLandMaterial);
            if (_borderMaterial != null) Destroy(_borderMaterial);
            if (_glowMaterial != null) Destroy(_glowMaterial);
        }

        private GameObject CreateObject(string objectName, Mesh mesh, Material material)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            return go;
        }

        private static Material CreateMaterial(string shaderName, string fallbackShader)
        {
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                UnityEngine.Debug.LogWarning("[Map] Shader '" + shaderName + "' not found; falling back to " + fallbackShader + ".");
                shader = Shader.Find(fallbackShader);
            }

            return new Material(shader);
        }
    }
}
