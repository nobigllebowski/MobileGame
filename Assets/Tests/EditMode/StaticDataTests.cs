using Nation.Core.Buildings;
using NUnit.Framework;

namespace Nation.Tests
{
    public sealed class StaticDataTests
    {
        [Test]
        public void Buildings_Load_With_Positive_Costs_And_Durations()
        {
            var buildings = BuildingDataParser.Parse(TestData.Buildings);

            Assert.IsTrue(buildings.Count >= 10);
            foreach (var building in buildings)
            {
                Assert.IsTrue(building.Cost > 0, building.Id);
                Assert.IsTrue(building.DurationDays > 0, building.Id);
                Assert.IsTrue(building.EffectValue > 0, building.Id);
                Assert.IsFalse(string.IsNullOrEmpty(building.EffectUnitKey), building.Id);
            }

            Assert.IsTrue(buildings.Exists(b => b.Id == "solar_plant" && b.Category == BuildingCategory.Energy && b.DurationDays == 120));
        }

        [Test]
        public void Map_Catalog_Loads_Landmasses_With_Valid_Coordinates()
        {
            var catalog = Nation.Core.Map.MapCatalogSerializer.Read(TestData.ReadBytes("Map", "world.map.bytes"));

            Assert.IsTrue(catalog.Countries.Count >= 200);
            foreach (var country in catalog.Countries)
            {
                var lod = country.Lod(Nation.Core.Map.MapZoomBand.Far);
                Assert.IsTrue(lod.RingCount >= 1, country.Id);
                for (var v = 0; v < lod.VertexCount; v++)
                {
                    var point = lod.Vertex(v);
                    Assert.IsTrue(point.X >= -2.75f && point.X <= 2.75f, country.Id);
                    Assert.IsTrue(point.Y >= -1.35f && point.Y <= 1.35f, country.Id);
                }
            }
        }

        [Test]
        public void Projection_Is_Symmetric_About_The_Origin()
        {
            Nation.Core.Map.Geometry.EqualEarthProjection.Project(-180, 0, out var minX, out _);
            Nation.Core.Map.Geometry.EqualEarthProjection.Project(180, 0, out var maxX, out _);
            Nation.Core.Map.Geometry.EqualEarthProjection.Project(0, -90, out _, out var minY);
            Nation.Core.Map.Geometry.EqualEarthProjection.Project(0, 90, out _, out var maxY);

            Assert.AreEqual(-maxX, minX);
            Assert.AreEqual(-maxY, minY);
        }
    }
}
