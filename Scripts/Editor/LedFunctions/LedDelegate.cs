using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JoeGatling.ButtonGrids.LedFunctions
{
    public class LedDelegate : ILedFunction
    {
        public System.Func<bool> _ledStateDelegate = null;

        public LedDelegate(System.Func<bool> ledStateDelegate)
        {
            _ledStateDelegate = ledStateDelegate;
        }

        public bool GetLedState()
        {
            if(_ledStateDelegate != null)
            {
                return _ledStateDelegate();
            }
            else
            {
                return false;
            }
        }
    }
}