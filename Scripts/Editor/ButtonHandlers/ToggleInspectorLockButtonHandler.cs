using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace JoeGatling.ButtonGrids.ButtonHandlers
{    
    [System.Serializable]
    [HandlerName("Inspector/Toggle Lock")]
    public class ToggleInspectorLockButtonHandler : IButtonHandler
    {
        private GlowingButton _button = null;

        ButtonGrids.LedFunctions.ILedFunction _defaultLedFunction = null;

        public string GetShortName()
        {
            return "Insp\nLock";
        }

        public void Initialize(GlowingButton button)
        {
            Teardown();

            _defaultLedFunction = new LedFunctions.LedDelegate(() =>
            {
                return UnityEditor.ActiveEditorTracker.sharedTracker.isLocked && LedFunctions.LedFlashing.fast.GetLedState(button);
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
                UnityEditor.ActiveEditorTracker.sharedTracker.isLocked = !UnityEditor.ActiveEditorTracker.sharedTracker.isLocked;
                UnityEditor.ActiveEditorTracker.sharedTracker.ForceRebuild();
            }
        }
    }
}