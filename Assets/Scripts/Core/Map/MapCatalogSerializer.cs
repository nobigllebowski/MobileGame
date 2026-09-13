using System;
using System.IO;
using System.Text;
using Nation.Core.Map.Geometry;

namespace Nation.Core.Map
{
    /// <summary>
    /// Compact little-endian binary format for the processed map (a few hundred kilobytes per zoom band).
    /// Chosen over JSON because 100k floats parse in milliseconds instead of hundreds of milliseconds on a phone.
    /// </summary>
    public static class MapCatalogSerializer
    {
        private const string Magic = "NWOM";

        public static byte[] Write(MapCatalog catalog)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                writer.Write(Encoding.ASCII.GetBytes(Magic));
                writer.Write(MapCatalog.FormatVersion);
                writer.Write(catalog.Projection ?? string.Empty);
                writer.Write(catalog.Source ?? string.Empty);
                WriteBounds(writer, catalog.Bounds);
                writer.Write(catalog.Countries.Count);

                foreach (var country in catalog.Countries)
                {
                    writer.Write(country.Id ?? string.Empty);
                    writer.Write(country.SovereignId ?? string.Empty);
                    writer.Write((int)country.Type);
                    writer.Write(country.Continent ?? string.Empty);
                    writer.Write(country.IsIsoCode);
                    writer.Write(country.Selectable);
                    writer.Write(country.LabelX);
                    writer.Write(country.LabelY);
                    writer.Write(country.LabelRank);
                    writer.Write(country.MinLabelZoom);
                    writer.Write(country.HasCapital);
                    writer.Write(country.CapitalX);
                    writer.Write(country.CapitalY);
                    writer.Write(country.CapitalLatitude);
                    writer.Write(country.CapitalLongitude);
                    writer.Write(country.PopulationEstimate);
                    writer.Write(country.GdpEstimate);
                    WriteBounds(writer, country.Bounds);
                    writer.Write(country.Area);
                    writer.Write(country.Lods.Length);
                    foreach (var lod in country.Lods)
                    {
                        WriteInts(writer, lod.RingStarts);
                        WriteInts(writer, lod.RingLengths);
                        writer.Write(lod.Vertices.Length);
                        for (var i = 0; i < lod.Vertices.Length; i++)
                        {
                            writer.Write(lod.Vertices[i]);
                        }

                        WriteInts(writer, lod.Triangles);
                    }
                }

                writer.Flush();
                return stream.ToArray();
            }
        }

        public static MapCatalog Read(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            using (var stream = new MemoryStream(bytes))
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
                if (magic != Magic)
                {
                    throw new InvalidDataException("Not a Nation map catalog.");
                }

                var version = reader.ReadInt32();
                if (version != MapCatalog.FormatVersion)
                {
                    throw new InvalidDataException("Map catalog format " + version + " is not supported (expected " + MapCatalog.FormatVersion + ").");
                }

                var catalog = new MapCatalog
                {
                    Projection = reader.ReadString(),
                    Source = reader.ReadString(),
                    Bounds = ReadBounds(reader)
                };

                var count = reader.ReadInt32();
                for (var c = 0; c < count; c++)
                {
                    var country = new MapCountry
                    {
                        Id = reader.ReadString(),
                        SovereignId = reader.ReadString(),
                        Type = (MapTerritoryType)reader.ReadInt32(),
                        Continent = reader.ReadString(),
                        IsIsoCode = reader.ReadBoolean(),
                        Selectable = reader.ReadBoolean(),
                        LabelX = reader.ReadSingle(),
                        LabelY = reader.ReadSingle(),
                        LabelRank = reader.ReadInt32(),
                        MinLabelZoom = reader.ReadSingle(),
                        HasCapital = reader.ReadBoolean(),
                        CapitalX = reader.ReadSingle(),
                        CapitalY = reader.ReadSingle(),
                        CapitalLatitude = reader.ReadDouble(),
                        CapitalLongitude = reader.ReadDouble(),
                        PopulationEstimate = reader.ReadDouble(),
                        GdpEstimate = reader.ReadDouble(),
                        Bounds = ReadBounds(reader),
                        Area = reader.ReadDouble()
                    };

                    var lodCount = reader.ReadInt32();
                    country.Lods = new MapLod[lodCount];
                    for (var l = 0; l < lodCount; l++)
                    {
                        var lod = new MapLod
                        {
                            RingStarts = ReadInts(reader),
                            RingLengths = ReadInts(reader)
                        };
                        var vertexFloats = reader.ReadInt32();
                        lod.Vertices = new float[vertexFloats];
                        for (var i = 0; i < vertexFloats; i++)
                        {
                            lod.Vertices[i] = reader.ReadSingle();
                        }

                        lod.Triangles = ReadInts(reader);
                        country.Lods[l] = lod;
                    }

                    catalog.Countries.Add(country);
                }

                return catalog;
            }
        }

        private static void WriteBounds(BinaryWriter writer, MapBounds bounds)
        {
            writer.Write(bounds.MinX);
            writer.Write(bounds.MinY);
            writer.Write(bounds.MaxX);
            writer.Write(bounds.MaxY);
        }

        private static MapBounds ReadBounds(BinaryReader reader)
        {
            return new MapBounds
            {
                MinX = reader.ReadSingle(),
                MinY = reader.ReadSingle(),
                MaxX = reader.ReadSingle(),
                MaxY = reader.ReadSingle()
            };
        }

        private static void WriteInts(BinaryWriter writer, int[] values)
        {
            writer.Write(values.Length);
            for (var i = 0; i < values.Length; i++)
            {
                writer.Write(values[i]);
            }
        }

        private static int[] ReadInts(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            var values = new int[count];
            for (var i = 0; i < count; i++)
            {
                values[i] = reader.ReadInt32();
            }

            return values;
        }
    }
}
