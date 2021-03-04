using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace JoeGatling.ButtonGrids.ButtonHandlers
{    
    [System.Serializable]
    public class ToggleActivationHandler : IButtonHandler
    {
        private GlowingButton _button = null;

        ButtonGrids.LedFunctions.ILedFunction _defaultLedFunction = null;

        public void Initialize(GlowingButton button)
        {
            Teardown();

            _defaultLedFunction = new LedFunctions.LedDelegate(() =>
            {
                return (Selection.gameObjects.Length > 0) && Selection.gameObjects.All((x) => x.activeSelf == true);
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
        {
            if(_button.key == true)
            {
                _button.ledFunction = LedFunctions.LedOn.instance;
            }
            else
            {
                if(EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    _button.ledFunction = LedFunctions.LedOn.instance;
                }
                else
                {
                    _button.ledFunction = LedFunctions.LedOff.instance;
                }
            }
        }

        protected void OnButtonStateChanged(bool state)
        {
            if(state == true)
            {
                if(Selection.gameObjects.Length > 0)
                {
                    bool allActive = Selection.gameObjects.All((x) => x.activeSelf == true);

                    foreach(var g in Selection.gameObjects)
                    {
                        g.SetActive(!allActive);
                    }
                }
            }
        }
    }
}