using System.Collections;
using System.Collections.Generic;
using Grids;
using UnityEngine;

namespace Grids.ButtonHandlers
{
    public class DeleteStoredDataButtonHandler : IButtonHandler
    {
        public static LedFunctions.ILedFunction deleteModeLedFunction = new LedFunctions.LedPulse(2.0f, 0.6f);

        public static System.Action<bool> onDeleteStateChanged = delegate{};
        private static int __deleteButtonsHeld = 0;
        public static bool isDeleteButtonHeld => __deleteButtonsHeld > 0;

        private GlowingButton _button = null;
        public void Initialize(GlowingButton button)
        {
            _button = button;
            _button.onKeyStateChanged += OnButtonStateChanged;
            _button.ledFunction = Grids.LedFunctions.LedOff.instance;
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
        }

        protected void OnButtonStateChanged(bool state)
        {
            if(state == false)
            {
                DecrementDeleteButtonCount();
                _button.ledFunction = Grids.LedFunctions.LedOff.instance;
            }
            else
            {
                IncrementDeleteButtonCount();
                _button.ledFunction = deleteModeLedFunction;
            }
        }        

        private static void IncrementDeleteButtonCount()
        {
            if(__deleteButtonsHeld == 0)
            {
                onDeleteStateChanged?.Invoke(true);
            }

            __deleteButtonsHeld++;
        }

        private static void DecrementDeleteButtonCount()
        {
            __deleteButtonsHeld = Mathf.Max(0, __deleteButtonsHeld-1);

            if(__deleteButtonsHeld == 0)
            {
                onDeleteStateChanged?.Invoke(false);
            }
        }        

    }
}