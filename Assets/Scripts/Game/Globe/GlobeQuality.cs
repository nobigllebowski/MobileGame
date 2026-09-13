using UnityEngine;

namespace Nation.Game.Globe
{
    public enum GlobeQualityTier
    {
        /// <summary>Flat-shaded land and ocean only.</summary>
        Low = 0,
        /// <summary>Adds terrain shading and the atmosphere rim.</summary>
        Medium = 1,
        /// <summary>Adds the cloud layer and night-side city lights.</summary>
        High = 2
    }

    /// <summary>Picks a tier from the device. A user override can be layered on top in the settings phase.</summary>
    public static class GlobeQuality
    {
        public static GlobeQualityTier Resolve()
        {
            if (!Application.isMobilePlatform)
            {
                return GlobeQualityTier.High;
            }

            var memory = SystemInfo.graphicsMemorySize;
            var cores = SystemInfo.processorCount;
            if (memory >= 2048 && cores >= 6)
            {
                return GlobeQualityTier.High;
            }

            if (memory >= 1024 || cores >= 4)
            {
                return GlobeQualityTier.Medium;
            }

            return GlobeQualityTier.Low;
        }
    }
}
