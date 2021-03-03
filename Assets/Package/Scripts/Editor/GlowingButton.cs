using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JoeGatling.ButtonGrids.LedFunctions;

namespace JoeGatling.ButtonGrids
{
    public class GlowingButton
    {

        public ILedFunction ledFunction { get; set; }

        public bool led => _currentLedState;
        public bool key => _currentKeyState;

        public event System.Action<bool> onKeyStateChanged = delegate { };
        public event System.Action<bool> onLedStateChanged = delegate { };

        private bool _currentKeyState = false;
        private bool _currentLedState = false;

        private LedDelegate _isPressedLedFunction = null;// = new LedDelegate(() => { return key; });
        public LedDelegate isPressedFunction
        {
            get
            {
                if(_isPressedLedFunction == null)
                {
                    _isPressedLedFunction = new LedDelegate(() => key);
                }

                return _isPressedLedFunction;
            }

        }

        public void Update(bool pressedState)
        {
            if(pressedState != _currentKeyState)
            {
                _currentKeyState = pressedState;
                onKeyStateChanged?.Invoke(_currentKeyState);
            }

            UpdateLedFunction();
        }

        private void UpdateLedFunction()
        {
            bool targetLedState = false;

            if(ledFunction != null)
            {
                targetLedState =  ledFunction.GetLedState();
            }

            if(targetLedState != _currentLedState)
            {
                _currentLedState = targetLedState;
                onLedStateChanged?.Invoke(_currentLedState);
            }
        }
    }
}