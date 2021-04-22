using System.Collections;
using System.Collections.Generic;
using JoeGatling.ButtonGrids;
using JoeGatling.ButtonGrids.LedFunctions;
using UnityEngine;


namespace JoeGatling.ButtonGrids.ButtonHandlers
{
    [HandlerName("Delete Stored Data")]
    public class DeleteStoredDataButtonHandler : IButtonHandler
    {
        public static Vector2Int currentDeletePosition { get; protected set; }

        //public static System.Action<bool> onDeleteStateChanged = delegate{};
        private static int __deleteButtonsHeld = 0;
        public static bool isDeleteButtonHeld => __deleteButtonsHeld > 0;

        private static List<IDeleteStoredData> __allDeleters = new List<IDeleteStoredData>();



        public static void RegisterDeleter(IDeleteStoredData deleter)
        {
            __allDeleters.Add(deleter);
        }

        public static void UnregisterDeleter(IDeleteStoredData deleter)
        {
            __allDeleters.Remove(deleter);
        }

        private GlowingButton _button = null;
        public void Initialize(GlowingButton button)
        {
            _button = button;
            _button.onKeyStateChanged += OnButtonStateChanged;
            _button.ledFunction = LedFunctions.LedOff.instance;
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
                _button.ledFunction = LedOff.instance;
            }
            else
            {
                IncrementDeleteButtonCount();
                _button.ledFunction = LedOn.instance;
            }
        }        

        private void IncrementDeleteButtonCount()
        {
            if(__deleteButtonsHeld == 0)
            {
                currentDeletePosition = new Vector2Int(_button.x, _button.y);

                GridController.overrideLedFunction = new LedFunctions.LedDeleteMode();

                for (int i = 0; i < __allDeleters.Count; i++)
                {
                    __allDeleters[i].OnDeleteStateChanged(true);
                }
            }

            __deleteButtonsHeld++;
        }

        private void DecrementDeleteButtonCount()
        {
            __deleteButtonsHeld = Mathf.Max(0, __deleteButtonsHeld-1);

            if(__deleteButtonsHeld == 0)
            {
                currentDeletePosition = Vector2Int.zero;

                GridController.overrideLedFunction = null;

                for (int i = 0; i < __allDeleters.Count; i++)
                {
                    __allDeleters[i].OnDeleteStateChanged(false);
                }
            }
        }        

    }
}