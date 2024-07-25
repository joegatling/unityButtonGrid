using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace JoeGatling.ButtonGrids.ButtonHandlers
{
    [System.Serializable]
    [HandlerName("Window/Maximize Window")]
    public class MaximuzeWindowButtonHandler : IButtonHandler
    {
        private GlowingButton _button = null;      

        LedFunctions.LedDelegate _defaultLedFunction = null;

        private bool isWindowMaximized => UnityEditor.EditorWindow.focusedWindow != null && UnityEditor.EditorWindow.focusedWindow.maximized == true;

        public void Initialize(GlowingButton button)        
        {
            _button = button;
            _button.onKeyStateChanged += OnKeyStateChanged;

            _defaultLedFunction = new LedFunctions.LedDelegate(() => 
            {
                return isWindowMaximized;
            });

            _button.ledFunction = _defaultLedFunction;
        }

        public void Teardown()
        {
            if(_button != null)
            {
                _button.onKeyStateChanged -= OnKeyStateChanged;
                _button.ledFunction = null;
                _button = null;
            }
        }

        public void Update()
        { }

        void OnKeyStateChanged(bool state)
        {
            if(state == true)
            {
                if (UnityEditor.EditorWindow.focusedWindow != null)
                {
                    UnityEditor.EditorWindow.focusedWindow.maximized = !UnityEditor.EditorWindow.focusedWindow.maximized;
                }
            }
        }      
    }
}