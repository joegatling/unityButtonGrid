using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JoeGatling.ButtonGrids.ButtonHandlers
{    
    [System.Serializable]
    [HandlerName("Menu Item")]
    public class MenuItemButtonHandler : IButtonHandler
    {
        [SerializeField] private string _menuItem = "";
        
        private GlowingButton _button = null;

        private double _errorResetTime = 0.0f;

        public void Initialize(GlowingButton button)
        {
            Teardown();

            button.ledFunction = LedFunctions.LedOn.instance;
            
            _button = button;
            _button.onKeyStateChanged += OnButtonStateChanged;            
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
        {
            if(_errorResetTime > 0.0 && EditorApplication.timeSinceStartup > _errorResetTime)
            {
                _button.ledFunction = LedFunctions.LedOn.instance;
                _errorResetTime = 0.0;
            }
        }

        protected void OnButtonStateChanged(bool state)
        {
            if(state == true && _menuItem != "")
            {
                var result = EditorApplication.ExecuteMenuItem(_menuItem);

                if (result == false)
                {
                    _button.ledFunction = LedFunctions.LedFlashing.veryFast;
                    _errorResetTime = EditorApplication.timeSinceStartup + 1.0;
                }
            }
        }
    }
}