using Nation.Game.Bootstrap;
using UnityEngine;

namespace Nation.Game.Map
{
    /// <summary>Creates the scene-side map objects in the World scene: the renderer root and the camera controller.</summary>
    public static class MapWorldBuilder
    {
        public sealed class Result
        {
            public MapRenderer Renderer;
            public MapCameraController Camera;
        }

        public static Result Build(GameContext context)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = MapPalette.OceanDeep;
            camera.orthographic = true;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 50f;

            var controller = camera.gameObject.GetComponent<MapCameraController>();
            if (controller == null)
            {
                controller = camera.gameObject.AddComponent<MapCameraController>();
            }

            controller.Initialize(camera, context.Map.Zoom);

            var root = new GameObject("World Map");
            var renderer = root.AddComponent<MapRenderer>();
            renderer.Initialize(context.Map);
            controller.BandChanged += band => renderer.SetBand(band);

            return new Result { Renderer = renderer, Camera = controller };
        }
    }
}
