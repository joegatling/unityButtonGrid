using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Grids.ButtonHandlers
{    
    [System.Serializable]
    public class TogglePauseButtonHandler : IButtonHandler
    {
        private GlowingButton _button = null;        

        public void Initialize(GlowingButton button)
        {
            Teardown();

            _button = button;
            _button.onKeyStateChanged += OnButtonStateChanged;        
            //_button.ledFunction = LedFunctions.LedDelegate(() => _)    
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
            if(_button.key == true)
            {
                _button.ledFunction = LedFunctions.LedOn.instance;
            }
            else
            {
                if(EditorApplication.isPaused)
                {
                    if(EditorApplication.isPlaying)
                    {
                        _button.ledFunction = LedFunctions.LedOn.instance;
                    }
                    else
                    {
                        _button.ledFunction = LedFunctions.LedFlashing.fast;
                    }
                }
                else
                {
                    _button.ledFunction = LedFunctions.LedOff.instance;
                }
            }
        }

        protected void OnButtonStateChanged(bool state)
        {
            if(state == false)
            {
                EditorApplication.isPaused = !EditorApplication.isPaused;
            }
        }
    }
}