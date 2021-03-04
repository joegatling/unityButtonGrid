using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JoeGatling.ButtonGrids.LedFunctions
{
    public class LedPulse : ILedFunction
    {
        public static LedPulse slow = new LedPulse(1.0f, 0.25f);
        public static LedPulse fast = new LedPulse(4.0f, 0.25f);
        public static LedPulse veryFast = new LedPulse(8.0f, 0.25f);

        public float frequency { get; set; }
        public float dutyCycle { get; set; }
        
        
        public LedPulse(float frequency, float dutyCycle)
        {
            this.dutyCycle = Mathf.Clamp01(dutyCycle);
            this.frequency = frequency;
        }

        public bool GetLedState(GlowingButton button)
        {
            return Mathf.Repeat((float)UnityEditor.EditorApplication.timeSinceStartup * frequency, 1.0f) > (1.0f - dutyCycle);
        }
    }
}