using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grids.LedFunctions
{
    public class LedFlashing : ILedFunction
    {
        public static LedFlashing verySlow = new LedFlashing(0.5f);
        public static LedFlashing slow = new LedFlashing(1.0f);
        public static LedFlashing fast = new LedFlashing(4.0f);
        public static LedFlashing veryFast = new LedFlashing(8.0f);

        public float frequency { get; set; }
        
        
        public LedFlashing(float frequency)
        {
            this.frequency = frequency;
        }

        public bool GetLedState()
        {
            return ((int)(UnityEditor.EditorApplication.timeSinceStartup * frequency) % 2) == 1;
        }
    }
}