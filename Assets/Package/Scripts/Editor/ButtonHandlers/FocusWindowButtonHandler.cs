using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace JoeGatling.ButtonGrids.ButtonHandlers
{
    [System.Serializable]
    public class FocusWindowButtonHandler : IButtonHandler, IDeleteStoredData
    {
        [SerializeField] string _focusedWindowTypeName = null;
        [SerializeField] bool _hasSavedData = false;

        private GlowingButton _button = null;
        private double _buttonPressTime = 0;
        private bool _pressComplete = false;
        private bool _isInDeleteMode = false;

        LedFunctions.LedDelegate _defaultLedFunction = null;

        public bool HasStoredData => _hasSavedData;

        private System.Type _focusedWindowType = null;

        private bool isWindowFocused => _hasSavedData && UnityEditor.EditorWindow.focusedWindow != null && UnityEditor.EditorWindow.focusedWindow.GetType() == _focusedWindowType;

        public void Initialize(GlowingButton button)        
        {
            _button = button;
            _button.onKeyStateChanged += OnKeyStateChanged;

            _defaultLedFunction = new LedFunctions.LedDelegate(() => 
            {
                return _button.key ||
                       (isWindowFocused && LedFunctions.LedFlashing.slow.GetLedState(button)) ||
                       (_hasSavedData && !isWindowFocused);
            });

            _button.ledFunction = _defaultLedFunction;

            DeleteStoredDataButtonHandler.RegisterDeleter(this);

            _focusedWindowType = System.Type.GetType(_focusedWindowTypeName);
        }

        public void Teardown()
        {
            if(_button != null)
            {
                _button.onKeyStateChanged -= OnKeyStateChanged;
                _button.ledFunction = null;
                _button = null;
            }

            DeleteStoredDataButtonHandler.UnregisterDeleter(this);

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
                        SaveFocusedWindow();
                        _button.ledFunction = LedFunctions.LedFlashing.veryFast;
                    }
                }
            }

        }

        public void OnDeleteStateChanged(bool deleteState)
        {
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
                    LoadFocusedWindow();
                   
                }

                _pressComplete = false;

                _button.ledFunction = _defaultLedFunction;
            }

        }


        private void SaveFocusedWindow()
        {
            if(UnityEditor.EditorWindow.focusedWindow != null)
            {
                _focusedWindowType = UnityEditor.EditorWindow.focusedWindow.GetType();
                _focusedWindowTypeName = _focusedWindowType.AssemblyQualifiedName;

                _hasSavedData = true;
            }
        }

        private void LoadFocusedWindow()
        {
            if(_hasSavedData)
            {
                if(_focusedWindowType == null)
                {
                    _focusedWindowType = System.Type.GetType(_focusedWindowTypeName);
                }

                if(_focusedWindowType != null)
                {
                    UnityEditor.EditorWindow.FocusWindowIfItsOpen(_focusedWindowType);
                }
            }
        }        
    }


}