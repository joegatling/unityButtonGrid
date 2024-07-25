using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace JoeGatling.ButtonGrids.ButtonHandlers
{    
    [System.Serializable]
    [HandlerName("Timeline/Toggle Lock")]
    public class ToggleTimelineLockButtonHandler : IButtonHandler
    {
        
        private GlowingButton _button = null;

        ButtonGrids.LedFunctions.ILedFunction _defaultLedFunction = null;

        public void Initialize(GlowingButton button)
        {
            Teardown();

            _defaultLedFunction = new LedFunctions.LedDelegate(() =>
            {
                // TimelineEditorWindow timelineEditor = UnityEditor.EditorWindow.GetWindow(typeof(TimelineEditorWindow)) as TimelineEditorWindow;
                TimelineEditorWindow timelineEditor = Resources.FindObjectsOfTypeAll<TimelineEditorWindow>().FirstOrDefault();
                
                if (timelineEditor == null)
                {
                    return false;
                }
               
                return timelineEditor.locked && LedFunctions.LedFlashing.veryFast.GetLedState(button);
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
                //TimelineEditorWindow timelineEditor = UnityEditor.EditorWindow.GetWindow(typeof(TimelineEditorWindow)) as TimelineEditorWindow;
                TimelineEditorWindow timelineEditor = Resources.FindObjectsOfTypeAll<TimelineEditorWindow>().FirstOrDefault();
                if (timelineEditor == null)
                {
                    return;
                }             
                
                timelineEditor.locked = !timelineEditor.locked;
                timelineEditor.Repaint();
            }
        }
    }
}