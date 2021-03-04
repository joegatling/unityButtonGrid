using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JoeGatling.ButtonGrids.LedFunctions
{
    public class LedOff : ILedFunction
    {
        public static LedOff instance = new LedOff();

        public LedOff()
        {}

        public bool GetLedState(GlowingButton button)
        {
            return false;
        }
    }
}