using System.Collections;
using System.Collections.Generic;
using JoeGatling.ButtonGrids;
using JoeGatling.ButtonGrids.LedFunctions;
using UnityEngine;

namespace JoeGatling.ButtonGrids.ButtonHandlers
{
    [HandlerName("Test/Flash Test")]
    public class FlashTestButtonHandler : IButtonHandler
    {
        GlowingButton _button = null;

        private float[] _frequencies = { 1.0f, 2.0f, 4.0f, 10.0f};
        [HideInInspector] [SerializeField] private int _currentFrequency = 0;

        LedFlashing _flashingLedFunction = null;
        
        
        public void Initialize(GlowingButton button)
        {
            _button = button;     
            _button.onKeyStateChanged += OnButtonStateChanged;

            //_currentFrequency = 0;
            _flashingLedFunction = new LedFlashing(_frequencies[_currentFrequency]);
            _button.ledFunction = _flashingLedFunction;
        }

        public void Teardown()
        {
            _button.ledFunction = null;
            _button = null;
        }

        public void Update()
        {
            _flashingLedFunction.frequency = _frequencies[_currentFrequency];
        }

        public void OnButtonStateChanged(bool state)
        {
            if(state == true)
            {
                _button.ledFunction = LedOn.instance;
            }
            else
            {
                _currentFrequency = (_currentFrequency + 1) % _frequencies.Length;
                _flashingLedFunction.frequency = _frequencies[_currentFrequency];            
                _button.ledFunction = _flashingLedFunction;
            }
        }
    }
}
