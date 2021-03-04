using System.Collections;
using System.Collections.Generic;
using JoeGatling.ButtonGrids.ButtonHandlers;
using UnityEngine;

namespace JoeGatling.ButtonGrids.LedFunctions
{
    public class LedDeleteMode : ILedFunction
    {
        private float frequency = 2.0f;
        private float dutyCycleNormal = 0.05f;
        private float dutyCycleOn = 0.5f;

        private double startTime = 0;

        public LedDeleteMode()
        {
            startTime = UnityEditor.EditorApplication.timeSinceStartup;
        }

        public bool GetLedState(GlowingButton button)
        {
            if(button.position == DeleteStoredDataButtonHandler.currentDeletePosition)
            {
                return true;
            }

            IButtonHandler buttonHandler = GridController.gridConfig.GetButtonHandler(button.x, button.y);

            float time = (float)(UnityEditor.EditorApplication.timeSinceStartup - startTime) * frequency;
            Vector2 offset = DeleteStoredDataButtonHandler.currentDeletePosition - button.position;
            time -= offset.magnitude * 0.05f;

            if (buttonHandler != null &&
                buttonHandler is IDeleteStoredData &&
                (buttonHandler as IDeleteStoredData).HasStoredData)
            {
                IDeleteStoredData deleter = buttonHandler as IDeleteStoredData;
                return time > 0 && (time < dutyCycleNormal || Mathf.Repeat((float)UnityEditor.EditorApplication.timeSinceStartup * frequency, 1) < dutyCycleOn);
            }

            return time > 0 && time < dutyCycleNormal;

        }
    }
}