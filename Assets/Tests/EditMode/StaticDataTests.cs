using Nation.Core.Buildings;
using Nation.Core.Map;
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
        public void Map_Loads_Landmasses_With_Valid_Coordinates()
        {
            var map = MapData.Parse(TestData.Map);

            Assert.IsTrue(map.Landmasses.Count >= 6);
            foreach (var landmass in map.Landmasses)
            {
                Assert.IsTrue(landmass.Points.Count >= 3, landmass.Id);
                foreach (var point in landmass.Points)
                {
                    Assert.IsTrue(point.Latitude >= -90 && point.Latitude <= 90, landmass.Id);
                    Assert.IsTrue(point.Longitude >= -180 && point.Longitude <= 180, landmass.Id);
                }
            }
        }

        [Test]
        public void Projection_Maps_Corners_To_Unit_Square()
        {
            Assert.AreEqual(0.0, MapProjection.X(-180));
            Assert.AreEqual(1.0, MapProjection.X(180));
            Assert.AreEqual(0.5, MapProjection.X(0));
            Assert.AreEqual(0.0, MapProjection.Y(90));
            Assert.AreEqual(1.0, MapProjection.Y(-90));
        }
    }
}
