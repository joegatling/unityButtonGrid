using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JoeGatling.ButtonGrids.ButtonHandlers
{
    [System.Serializable]
    public class SaveSceneButtonHandler : IButtonHandler
    {
        [SerializeField] Vector3 _position = Vector3.zero;
        [SerializeField] Vector3 _rotation = Vector3.zero;
        [SerializeField] bool _orthographic = false;
        [SerializeField] bool _hasSavedData = false;

        private GlowingButton _button = null;
        private double _buttonPressTime = 0;
        private bool _pressComplete = false;
        private bool _isInDeleteMode = false;

        LedFunctions.LedDelegate _defaultLedFunction = null;
        

        public void Initialize(GlowingButton button)        
        {
            _button = button;
            _button.onKeyStateChanged += OnKeyStateChanged;

            _defaultLedFunction = new LedFunctions.LedDelegate(() => 
            {
                return _button.key || _hasSavedData;
            });

            _button.ledFunction = _defaultLedFunction;

            DeleteStoredDataButtonHandler.onDeleteStateChanged += OnDeleteStateChanged;
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
        {
            if(_button.key == true && !_isInDeleteMode)
            {
                if(!_pressComplete)
                {
                    if(UnityEditor.EditorApplication.timeSinceStartup - _buttonPressTime > 1.0f)
                    {
                        _pressComplete = true;

                        var sceneView = UnityEditor.SceneView.lastActiveSceneView;
                        var camera = sceneView.camera;

                        _position = camera.transform.position;
                        _rotation = camera.transform.eulerAngles;
                        _orthographic = sceneView.orthographic;

                        _hasSavedData = true;

                        _button.ledFunction = LedFunctions.LedFlashing.veryFast;
                        //UnityEditor.EditorApplication.delayCall()
                    }
                }
            }

        }

        void OnDeleteStateChanged(bool deleteState)
        {
            if(deleteState == true)
            {
                if(_hasSavedData)
                {
                    _button.ledFunction = DeleteStoredDataButtonHandler.deleteModeLedFunction;
                }
            }
            else
            {
                _button.ledFunction = _defaultLedFunction;
            }

            _isInDeleteMode = deleteState;
        }

        void OnKeyStateChanged(bool state)
        {
            if(state == true)
            {
                if(_isInDeleteMode)
                {
                    _hasSavedData = false;
                }
                else
                {
                    _buttonPressTime = UnityEditor.EditorApplication.timeSinceStartup;
                }
            }
            else
            {
                if(!_pressComplete)
                {
                    if(_hasSavedData)
                    {
                        var sceneView = UnityEditor.SceneView.lastActiveSceneView;
                        var camera = sceneView.camera;

                        camera.transform.position = _position;
                        camera.transform.eulerAngles = _rotation;

                        sceneView.AlignViewToObject(camera.transform);
                        sceneView.orthographic = _orthographic;
                    }
                }

                _pressComplete = false;

                _button.ledFunction = _defaultLedFunction;
            }

        }
    }
}