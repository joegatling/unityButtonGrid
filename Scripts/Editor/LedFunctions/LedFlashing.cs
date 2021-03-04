using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JoeGatling.ButtonGrids.LedFunctions
{
    public class LedFlashing : ILedFunction
    {
        public static LedFlashing verySlow = new LedFlashing(0.5f);
        public static LedFlashing slow = new LedFlashing(1.0f);
        public static LedFlashing fast = new LedFlashing(4.0f);
        public static LedFlashing veryFast = new LedFlashing(8.0f);

        public float frequency { get; set; }
        
        public float xTimeOffset { get; set; }
        public float yTimeOffset { get; set; }

        public LedFlashing(float frequency, float xTimeOffset = 0, float yTimeOffset = 0)
        {
            this.frequency = frequency;

            this.xTimeOffset = xTimeOffset;
            this.yTimeOffset = yTimeOffset;
        }

        public bool GetLedState(GlowingButton button)
        {
            return ((int)((UnityEditor.EditorApplication.timeSinceStartup + (button.x * xTimeOffset + button.y * yTimeOffset)) * frequency) % 2) == 1;
        }
    }
}