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

        public int x { get; private set; }
        public int y { get; private set; }
        public Vector2Int position => new Vector2Int(x, y);
        

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

        public GlowingButton(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void Update(bool pressedState, ILedFunction overrideLedFunction = null)
        {
            if(pressedState != _currentKeyState)
            {
                _currentKeyState = pressedState;
                onKeyStateChanged?.Invoke(_currentKeyState);
            }

            UpdateLedFunction(overrideLedFunction != null ? overrideLedFunction : ledFunction);
        }

        private void UpdateLedFunction(ILedFunction ledFunc = null)
        {
            bool targetLedState = false;

            if(ledFunc != null)
            {
                targetLedState =  ledFunc.GetLedState(this);
            }

            if(targetLedState != _currentLedState)
            {
                _currentLedState = targetLedState;
                onLedStateChanged?.Invoke(_currentLedState);
            }
        }
    }
}