using System.Collections.Generic;
using Nation.Core.Map;
using Nation.Game.UI.Core;
using UnityEngine;

namespace Nation.Game.Globe
{
    /// <summary>Rotates the globe (and its clouds slightly faster) once per frame; the only per-frame work in the menu scene.</summary>
    public sealed class GlobeRotator : MonoBehaviour
    {
        public float DegreesPerSecond = 2.4f;
        public Transform Clouds;
        public float CloudFactor = 1.35f;

        private void Update()
        {
            var step = DegreesPerSecond * UnityEngine.Time.unscaledDeltaTime;
            transform.Rotate(0f, -step, 0f, Space.Self);
            if (Clouds != null)
            {
                Clouds.Rotate(0f, -step * (CloudFactor - 1f), 0f, Space.Self);
            }
        }
    }

    /// <summary>
    /// Builds the main-menu Earth: a UV sphere textured by the rasterized map catalog, with atmosphere and clouds
    /// according to the quality tier. Real geography, no imagery downloads, three draw calls at most.
    /// </summary>
    public static class EarthGlobe
    {
        private const float Radius = 1.0f;

        public static GameObject Build(MapCatalog catalog, GlobeQualityTier tier, Camera camera)
        {
            var root = new GameObject("Earth");
            root.transform.position = new Vector3(0f, 0.55f, 0f);
            root.transform.rotation = Quaternion.Euler(0f, 0f, -23.4f);

            var mask = EarthTextureBuilder.Build(catalog);
            var sphere = BuildSphere(Radius, 64, 40);

            var surfaceMaterial = CreateMaterial("Nation/Globe", "Unlit/Texture");
            surfaceMaterial.SetTexture("_MainTex", mask);
            surfaceMaterial.SetColor("_OceanColor", Palette.Hex("#0A1630"));
            surfaceMaterial.SetColor("_LandColor", Palette.Hex("#3B4E75"));
            surfaceMaterial.SetColor("_NightColor", Palette.Hex("#030610"));
            surfaceMaterial.SetColor("_LightsColor", Palette.Hex("#F2C27A"));
            surfaceMaterial.SetColor("_AtmosphereColor", Palette.Hex("#3B82F6"));
            surfaceMaterial.SetVector("_LightDir", new Vector4(-0.55f, 0.35f, -0.75f, 0f));
            surfaceMaterial.SetFloat("_Tier", (int)tier);
            AddMesh(root, "Surface", sphere, surfaceMaterial);

            var body = root.AddComponent<GlobeRotator>();

            if (tier >= GlobeQualityTier.Medium)
            {
                var atmosphere = AddMesh(root, "Atmosphere", BuildSphere(Radius * 1.045f, 48, 30), CreateMaterial("Nation/GlobeAtmosphere", "Sprites/Default"));
                atmosphere.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_Color", new Color(0.3f, 0.55f, 1f, 1f));
            }

            if (tier >= GlobeQualityTier.High)
            {
                var clouds = AddMesh(root, "Clouds", BuildSphere(Radius * 1.018f, 48, 30), CreateMaterial("Nation/GlobeClouds", "Sprites/Default"));
                clouds.GetComponent<MeshRenderer>().sharedMaterial.SetVector("_LightDir", new Vector4(-0.55f, 0.35f, -0.75f, 0f));
                body.Clouds = clouds.transform;
            }

            if (camera != null)
            {
                camera.orthographic = false;
                camera.fieldOfView = 34f;
                camera.transform.position = new Vector3(0f, 0.55f, -4.2f);
                camera.transform.rotation = Quaternion.identity;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Palette.Background;
            }

            return root;
        }

        private static GameObject AddMesh(GameObject parent, string childName, Mesh mesh, Material material)
        {
            var child = new GameObject(childName);
            child.transform.SetParent(parent.transform, false);
            child.AddComponent<MeshFilter>().sharedMesh = mesh;
            child.AddComponent<MeshRenderer>().sharedMaterial = material;
            return child;
        }

        private static Material CreateMaterial(string shaderName, string fallback)
        {
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogWarning("[Globe] Shader '" + shaderName + "' not found; falling back to " + fallback + ".");
                shader = Shader.Find(fallback);
            }

            return new Material(shader);
        }

        /// <summary>UV sphere with seam-free equirectangular mapping (u = longitude, v = latitude).</summary>
        public static Mesh BuildSphere(float radius, int segments, int rings)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            for (var y = 0; y <= rings; y++)
            {
                var v = (float)y / rings;
                var phi = (v - 0.5f) * Mathf.PI;
                var cosPhi = Mathf.Cos(phi);
                var sinPhi = Mathf.Sin(phi);
                for (var x = 0; x <= segments; x++)
                {
                    var u = (float)x / segments;
                    var theta = (u - 0.5f) * Mathf.PI * 2f;
                    var normal = new Vector3(-Mathf.Cos(theta) * cosPhi, sinPhi, Mathf.Sin(theta) * cosPhi);
                    vertices.Add(normal * radius);
                    normals.Add(normal);
                    uvs.Add(new Vector2(u, v));
                }
            }

            var stride = segments + 1;
            for (var y = 0; y < rings; y++)
            {
                for (var x = 0; x < segments; x++)
                {
                    var a = y * stride + x;
                    var b = a + stride;
                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(a + 1);
                    triangles.Add(a + 1);
                    triangles.Add(b);
                    triangles.Add(b + 1);
                }
            }

            var mesh = new Mesh { name = "Globe_" + segments + "x" + rings };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.normals = normals.ToArray();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
