using System.IO;
using Nation.Core.Countries;
using Nation.Core.Data;
using Nation.Core.Map;
using Nation.Core.Map.Geometry;
using Nation.Core.Map.Import;
using Nation.Core.Map.Layers;
using Nation.Core.Models;
using Nation.Core.Signals;
using Nation.Core.World;
using NUnit.Framework;

namespace Nation.Tests
{
    public sealed class MapCatalogTests
    {
        private static MapCatalog _catalog;

        private static MapCatalog Catalog()
        {
            if (_catalog == null)
            {
                _catalog = MapCatalogSerializer.Read(TestData.ReadBytes("Map", "world.map.bytes"));
            }

            return _catalog;
        }

        private static CountryCatalog Simulation() => new CountryCatalog(CountryDataParser.Parse(TestData.Countries));

        [Test]
        public void Shipped_Catalog_Loads_With_Every_Simulated_Country()
        {
            var catalog = Catalog();

            Assert.IsTrue(catalog.Countries.Count >= 200);
            Assert.AreEqual("equal-earth", catalog.Projection);
            foreach (var definition in Simulation().All)
            {
                Assert.IsTrue(catalog.TryGet(definition.Id, out var country), definition.Id);
                Assert.IsTrue(country.Selectable, definition.Id);
                Assert.IsTrue(country.HasCapital, definition.Id);
                Assert.IsTrue(country.Lod(MapZoomBand.Near).VertexCount > 3, definition.Id);
            }
        }

        [Test]
        public void Iso3_Lookup_Is_Case_Insensitive_And_Codes_Are_Unique()
        {
            var catalog = Catalog();
            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (var country in catalog.Countries)
            {
                Assert.AreEqual(3, country.Id.Length, country.Id);
                Assert.IsTrue(seen.Add(country.Id), "duplicate " + country.Id);
            }

            Assert.IsTrue(catalog.TryGet("fra", out var france));
            Assert.AreEqual("FRA", france.Id);
            Assert.IsTrue(france.IsIsoCode);
            Assert.IsFalse(catalog.TryGet("ZZZ", out _));
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => catalog.Get("ZZZ"));
        }

        [Test]
        public void Capitals_Map_To_Known_Cities_Inside_Their_Countries()
        {
            var catalog = Catalog();
            var germany = catalog.Get("DEU");

            Assert.IsTrue(germany.HasCapital);
            Assert.IsTrue(System.Math.Abs(germany.CapitalLatitude - 52.5) < 0.3);
            Assert.IsTrue(System.Math.Abs(germany.CapitalLongitude - 13.4) < 0.3);
            Assert.IsTrue(MapValidator.ContainsPoint(germany, germany.CapitalX, germany.CapitalY, 0f));

            var southAfrica = catalog.Get("ZAF");
            Assert.IsTrue(System.Math.Abs(southAfrica.CapitalLatitude + 25.7) < 0.3, "expected Pretoria");
        }

        [Test]
        public void Every_Band_Has_Geometry_And_Detail_Increases()
        {
            var catalog = Catalog();

            var far = catalog.TotalVertices(MapZoomBand.Far);
            var mid = catalog.TotalVertices(MapZoomBand.Mid);
            var near = catalog.TotalVertices(MapZoomBand.Near);

            Assert.IsTrue(far < mid && mid < near);
            foreach (var country in catalog.Countries)
            {
                for (var band = 0; band < 3; band++)
                {
                    var lod = country.Lods[band];
                    Assert.IsTrue(lod.VertexCount >= 3, country.Id);
                    Assert.IsTrue(lod.Triangles.Length % 3 == 0, country.Id);
                    foreach (var index in lod.Triangles)
                    {
                        Assert.IsTrue(index >= 0 && index < lod.VertexCount, country.Id);
                    }
                }
            }
        }

        [Test]
        public void Serializer_Round_Trips_Exactly()
        {
            var catalog = Catalog();
            var bytes = MapCatalogSerializer.Write(catalog);
            var copy = MapCatalogSerializer.Read(bytes);

            Assert.AreEqual(catalog.Countries.Count, copy.Countries.Count);
            var a = catalog.Get("JPN");
            var b = copy.Get("JPN");
            Assert.AreEqual(a.Lod(MapZoomBand.Near).Vertices, b.Lod(MapZoomBand.Near).Vertices);
            Assert.AreEqual(a.Lod(MapZoomBand.Mid).Triangles, b.Lod(MapZoomBand.Mid).Triangles);
            Assert.AreEqual(a.CapitalX, b.CapitalX);
            Assert.AreEqual(a.PopulationEstimate, b.PopulationEstimate);
        }

        [Test]
        public void Picker_Resolves_Capitals_To_Their_Countries_And_Ocean_To_Nothing()
        {
            var catalog = Catalog();
            var picker = new MapPicker(catalog);

            foreach (var id in new[] { "DEU", "FRA", "USA", "JPN", "BRA", "IND", "CHN", "RUS", "GBR", "TUR" })
            {
                var country = catalog.Get(id);
                var hit = picker.CountryAt(country.CapitalX, country.CapitalY);
                Assert.IsNotNull(hit, id);
                Assert.AreEqual(id, hit.Id);
            }

            var atlantic = EqualEarthProjection.Project(-30, 30);
            Assert.IsNull(picker.CountryAt(atlantic.X, atlantic.Y));

            var lesotho = catalog.Get("LSO");
            var enclave = picker.CountryAt(lesotho.Bounds.CenterX, lesotho.Bounds.CenterY);
            Assert.AreEqual("LSO", enclave.Id, "enclave must win over its host");
        }

        [Test]
        public void Validation_Reports_Missing_And_Duplicate_Ids()
        {
            var report = new MapValidationReport();
            var catalog = new MapCatalog();
            var lod = new MapLod { Vertices = new float[] { 0, 0, 1, 0, 0, 1 }, RingStarts = new[] { 0 }, RingLengths = new[] { 3 }, Triangles = new[] { 0, 1, 2 } };
            catalog.Countries.Add(new MapCountry { Id = "AAA", Lods = new[] { lod, lod, lod }, Type = MapTerritoryType.Dependency });
            catalog.Countries.Add(new MapCountry { Id = "AAA", Lods = new[] { lod, lod, lod }, Type = MapTerritoryType.Dependency });
            catalog.Countries.Add(new MapCountry { Id = "", Lods = new[] { lod, lod, lod } });

            MapValidator.ValidateCatalog(catalog, report);

            Assert.IsTrue(report.HasErrors);
            Assert.AreEqual(1, report.Count(MapValidationReport.DuplicateId));
            Assert.AreEqual(1, report.Count(MapValidationReport.MissingIso));
        }

        [Test]
        public void Validation_Flags_Simulated_Country_Missing_From_Map()
        {
            var report = new MapValidationReport();
            var catalog = new MapCatalog();
            var simulation = new CountryCatalog(new[] { new CountryDefinition { Id = "XYZ", Population = 1, Gdp = 1 } });

            MapValidator.ValidateAgainstSimulation(catalog, simulation, report);

            Assert.AreEqual(1, report.Count(MapValidationReport.SimulationMissing));
        }

        [Test]
        public void Shipped_Catalog_Passes_Validation_Without_Errors()
        {
            var report = new MapValidationReport();
            MapValidator.ValidateCatalog(Catalog(), report);
            MapValidator.ValidateAgainstSimulation(Catalog(), Simulation(), report);

            Assert.IsFalse(report.HasErrors, report.Summary());
            Assert.AreEqual(0, report.Count(MapValidationReport.SimulationMissing));
        }

        [Test]
        public void Pipeline_Is_Deterministic_On_A_Small_Sample()
        {
            const string sample = "{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"properties\":{\"ISO_A3_EH\":\"AAA\",\"ADM0_A3\":\"AAA\",\"SOV_A3\":\"AAA\",\"TYPE\":\"Sovereign country\",\"NAME_EN\":\"Alpha\",\"LABEL_X\":1,\"LABEL_Y\":1,\"POP_EST\":100,\"GDP_MD\":5,\"LABELRANK\":2,\"MIN_LABEL\":2},\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[2,0],[2,2],[0,2],[0,0]]]}},{\"type\":\"Feature\",\"properties\":{\"ISO_A3_EH\":\"-99\",\"ADM0_A3\":\"BBB\",\"SOV_A3\":\"AAA\",\"TYPE\":\"Dependency\",\"NAME_EN\":\"Beta\",\"LABEL_X\":5,\"LABEL_Y\":5},\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[4,4],[6,4],[6,6],[4,6],[4,4]]]]}}]}";
            const string places = "{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"properties\":{\"FEATURECLA\":\"Admin-0 capital\",\"ADM0_A3\":\"AAA\",\"NAME\":\"Alphaville\",\"NAME_EN\":\"Alphaville\",\"ADM0CAP\":1,\"POP_MAX\":10},\"geometry\":{\"type\":\"Point\",\"coordinates\":[1,1]}}]}";

            var first = MapBuildPipeline.Build(sample, places, new MapBuildOptions());
            var second = MapBuildPipeline.Build(sample, places, new MapBuildOptions());

            Assert.AreEqual(2, first.Catalog.Countries.Count);
            Assert.AreEqual(MapCatalogSerializer.Write(first.Catalog), MapCatalogSerializer.Write(second.Catalog));

            var alpha = first.Catalog.Get("AAA");
            Assert.IsTrue(alpha.IsIsoCode);
            Assert.IsTrue(alpha.HasCapital);
            Assert.AreEqual(5e6, alpha.GdpEstimate);
            Assert.AreEqual("ALPHA", first.NameTables["en"]["country.AAA"]);
            Assert.AreEqual("Alphaville", first.NameTables["en"]["capital.AAA"]);

            var beta = first.Catalog.Get("BBB");
            Assert.IsFalse(beta.IsIsoCode);
            Assert.AreEqual("AAA", beta.SovereignId);
            Assert.AreEqual(MapTerritoryType.Dependency, beta.Type);
            Assert.IsFalse(first.Report.HasErrors);
        }

        [Test]
        public void Generated_Name_Tables_Cover_Every_Map_Country()
        {
            var catalog = Catalog();
            var english = LocalizationTableParser.Parse(TestData.Read("Localization", "map-names.en.json"));
            var russian = LocalizationTableParser.Parse(TestData.Read("Localization", "map-names.ru.json"));

            foreach (var country in catalog.Countries)
            {
                Assert.IsTrue(english.Strings.ContainsKey(country.NameKey), country.NameKey);
                Assert.IsTrue(russian.Strings.ContainsKey(country.NameKey), country.NameKey);
                if (country.HasCapital)
                {
                    Assert.IsTrue(english.Strings.ContainsKey(country.CapitalKey), country.CapitalKey);
                }
            }
        }

        [Test]
        public void Zoom_Model_Bands_Clamps_And_Focus()
        {
            var model = new MapZoomModel(Catalog().Bounds);

            Assert.AreEqual(MapZoomBand.Far, model.BandFor(5f));
            Assert.AreEqual(MapZoomBand.Mid, model.BandFor(1.5f));
            Assert.AreEqual(MapZoomBand.Near, model.BandFor(0.4f));
            Assert.AreEqual(model.MinVisibleWidth, model.ClampVisibleWidth(0.001f));
            Assert.AreEqual(model.MaxVisibleWidth, model.ClampVisibleWidth(100f));
            Assert.IsTrue(MapZoomModel.WebZoomFor(MapZoomModel.WorldWidth) > 0.5f && MapZoomModel.WebZoomFor(MapZoomModel.WorldWidth) < 0.7f);

            var germany = Catalog().Get("DEU");
            var width = model.FocusWidthFor(germany.Bounds, 390f / 600f);
            Assert.IsTrue(width >= model.MinVisibleWidth && width < 1f);
            Assert.AreEqual(MapZoomBand.Near, model.BandFor(width));
        }

        [Test]
        public void Camera_Center_Is_Clamped_To_World_Bounds()
        {
            var model = new MapZoomModel(Catalog().Bounds);
            var x = 100f;
            var y = -100f;

            model.ClampCenter(ref x, ref y, 1f, 1.5f);

            Assert.IsTrue(x < model.Bounds.MaxX && x > model.Bounds.MinX);
            Assert.IsTrue(y < model.Bounds.MaxY && y > model.Bounds.MinY);

            var cx = 0f;
            var cy = 0f;
            model.ClampCenter(ref cx, ref cy, 1f, 1f);
            Assert.AreEqual(0f, cx);
            Assert.AreEqual(0f, cy);
        }

        [Test]
        public void Labels_Follow_Zoom_Band_And_Rank()
        {
            var catalog = Catalog();
            var model = new MapZoomModel(catalog.Bounds);
            var russia = catalog.Get("RUS");
            var luxembourg = catalog.Get("LUX");

            Assert.IsTrue(model.ShowLabel(russia, model.MaxVisibleWidth));
            Assert.IsFalse(model.ShowLabel(luxembourg, model.MaxVisibleWidth));
            Assert.IsTrue(model.ShowLabel(luxembourg, model.MinVisibleWidth));
            Assert.IsFalse(model.ShowCapitals(model.MaxVisibleWidth));
            Assert.IsTrue(model.ShowCapitals(0.5f));
        }

        [Test]
        public void Political_Layer_Highlights_Player_And_Simulated_Countries()
        {
            var catalog = Catalog();
            var world = WorldFactory.CreateFromDefinitions(1, "DEU", Simulation().Playable);
            var layers = new MapLayerSet(() => world);
            var political = layers.Get(MapLayerKind.Political);

            Assert.IsTrue(political.TryGetValue(catalog.Get("DEU"), out var player));
            Assert.AreEqual(1f, player);
            political.TryGetValue(catalog.Get("FRA"), out var simulated);
            Assert.AreEqual(0.5f, simulated);
            political.TryGetValue(catalog.Get("KEN"), out var other);
            Assert.AreEqual(0f, other);
            MapCountry anyDependency = null;
            foreach (var candidate in catalog.Countries)
            {
                if (candidate.Type == MapTerritoryType.Dependency) { anyDependency = candidate; break; }
            }

            Assert.IsNotNull(anyDependency);
            political.TryGetValue(anyDependency, out var dependency);
            Assert.AreEqual(0.25f, dependency);
        }

        [Test]
        public void Economy_Layer_Uses_Live_State_And_Estimates()
        {
            var catalog = Catalog();
            var world = WorldFactory.CreateFromDefinitions(1, "DEU", Simulation().Playable);
            var layers = new MapLayerSet(() => world);
            var economy = layers.Get(MapLayerKind.Economy);

            Assert.AreEqual(MapLayerAvailability.Available, economy.Availability);
            Assert.IsTrue(economy.TryGetValue(catalog.Get("USA"), out var usa));
            Assert.IsTrue(economy.TryGetValue(catalog.Get("KEN"), out var kenya));
            Assert.IsTrue(usa > kenya);
            Assert.IsTrue(usa > 0.9f);

            world.GetCountry("DEU").Gdp = 1e14;
            economy.TryGetValue(catalog.Get("DEU"), out var boosted);
            Assert.AreEqual(1f, boosted);

            Assert.AreEqual(MapLayerAvailability.Unavailable, layers.Get(MapLayerKind.Diplomacy).Availability);
            Assert.IsFalse(layers.Get(MapLayerKind.Diplomacy).TryGetValue(catalog.Get("DEU"), out _));
            Assert.AreEqual(MapLayerAvailability.Partial, layers.Get(MapLayerKind.Stability).Availability);
            Assert.IsFalse(layers.Get(MapLayerKind.Stability).TryGetValue(catalog.Get("KEN"), out _));
            Assert.AreEqual(7, layers.Layers.Count);
        }

        [Test]
        public void Selection_Signal_Carries_Iso3_Or_Null()
        {
            var bus = new SignalBus();
            var received = new System.Collections.Generic.List<string>();
            bus.Subscribe<CountrySelectedSignal>(s => received.Add(s.CountryId));

            bus.Publish(new CountrySelectedSignal("FRA"));
            bus.Publish(new CountrySelectedSignal(null));

            Assert.AreEqual(new[] { "FRA", null }, received.ToArray());
            Assert.IsFalse(new CountrySelectedSignal(null).IsSelection);
        }
    }
}
