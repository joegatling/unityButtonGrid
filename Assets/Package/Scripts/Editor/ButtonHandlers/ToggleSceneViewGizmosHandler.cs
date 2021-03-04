using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace JoeGatling.ButtonGrids.ButtonHandlers
{    
    [System.Serializable]
    public class ToggleSceneViewGizmosHandler : IButtonHandler
    {
        private GlowingButton _button = null;

        ButtonGrids.LedFunctions.ILedFunction _defaultLedFunction = null;

        public void Initialize(GlowingButton button)
        {
            Teardown();

            _defaultLedFunction = new LedFunctions.LedDelegate(() =>
            {
                return SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.drawGizmos;
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
                if(SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.drawGizmos = !SceneView.lastActiveSceneView.drawGizmos;
                    SceneView.lastActiveSceneView.Repaint();
                }
            }
        }
    }
}