using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JoeGatling.ButtonGrids.ButtonHandlers
{    
    [System.Serializable]
    [HandlerName("Scene View/Align To Main Camera")]
    public class AlignSceneViewToMainCameraButtonHandler : IButtonHandler
    {
        private GlowingButton _button = null;        

        public void Initialize(GlowingButton button)
        {
            Teardown();

            _button = button;
            _button.onKeyStateChanged += OnButtonStateChanged;          
            _button.ledFunction = _button.isPressedFunction;  
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
        {}

        protected void OnButtonStateChanged(bool state)
        {
            if(state)
            {
                var sceneView = UnityEditor.SceneView.lastActiveSceneView;
                var camera = Camera.main;

                if(camera != null)
                {
                    sceneView.AlignViewToObject(camera.transform);                
                }
            }
        }
    }
}