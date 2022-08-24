using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using JoeGatling.ButtonGrids.ButtonHandlers;
using JoeGatling.ButtonGrids;
using SpatialGraph;

namespace JoeGatling.FrogownButtonGrid
{    
    [System.Serializable]
    [HandlerName("Night Tools/Toggle Spatial Tools")]
    public class ToggleSpatialToolsButtonHandler : IButtonHandler
    {
        private GlowingButton _button = null;

        ButtonGrids.LedFunctions.ILedFunction _defaultLedFunction = null;

        public void Initialize(GlowingButton button)
        {
            Teardown();

            _defaultLedFunction = new ButtonGrids.LedFunctions.LedDelegate(() =>
            {
                return (SpatialEditor.spatialToolsAreOpen);
            });

            _button = button;
            _button.onKeyStateChanged += OnButtonStateChanged;
            _button.ledFunction = _defaultLedFunction;
        }

        public void Teardown()
        {
            if(_button != null)
            {
                _button.onKeyStateChanged -= OnButtonStateChanged;
                _button.ledFunction = null;
                _button = null;
            }
        }

        public void Update()
        { }           


        protected void OnButtonStateChanged(bool state)
        {
            if(state == true)
            {
                SpatialEditor.spatialToolsAreOpen = !SpatialEditor.spatialToolsAreOpen;
            }
        }
    }
}