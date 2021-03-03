using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JoeGatling.ButtonGrids.LedFunctions
{
    public class LedOn : ILedFunction
    {
        public static LedOn instance = new LedOn();

        public LedOn()
        {}

        public bool GetLedState()
        {
            return true;
        }
    }
}