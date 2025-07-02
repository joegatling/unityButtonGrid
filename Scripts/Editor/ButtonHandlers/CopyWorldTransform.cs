using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JoeGatling.ButtonGrids.ButtonHandlers
{
    [System.Serializable]
    [HandlerName("Transform/Copy World Transform")]
    public class CopyWorldTransformButtonHandler : IButtonHandler
    {
        public enum OperationType
        {
            Copy,
            Paste
        }
        
        [SerializeField] private OperationType _operationType = OperationType.Copy;
        
        private static Vector3 _copiedPosition = Vector3.zero;
        private static Quaternion _copiedRotation = Quaternion.identity;
        private static Vector3 _copiedScale = Vector3.one;
        private static bool _hasCopiedData = false;
        
        private GlowingButton _button = null;
        LedFunctions.LedDelegate _defaultLedFunction = null;

        public void Initialize(GlowingButton button)        
        {
            _button = button;
            _button.onKeyStateChanged += OnKeyStateChanged;

            _defaultLedFunction = new LedFunctions.LedDelegate(() => 
            {
                if (_operationType == OperationType.Copy)
                {
                    return Selection.activeGameObject != null;
                }
                else
                {
                    return (Selection.activeGameObject != null && _hasCopiedData);
                }
            });

            _button.ledFunction = _defaultLedFunction;
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
        {}

        void OnKeyStateChanged(bool state)
        {
            if (state == true)
            {
                if (_operationType == OperationType.Copy)
                {
                    if (Selection.activeGameObject != null)
                    {
                        Transform transform = Selection.activeGameObject.transform;

                        _copiedPosition = transform.position;
                        _copiedRotation = transform.rotation;
                        _copiedScale = transform.lossyScale;
                        _hasCopiedData = true;
                    }
                }
                else
                {
                    if (Selection.activeGameObject != null && _hasCopiedData)
                    {
                        Transform transform = Selection.activeGameObject.transform;
                        Undo.RecordObject(transform, "Paste World Transform");

                        transform.position = _copiedPosition;
                        transform.rotation = _copiedRotation;

                        // For lossy scale, we need to calculate the local scale to achieve the desired world scale.
                        Vector3 parentScale = transform.parent ? transform.parent.lossyScale : Vector3.one;
                        transform.localScale = new Vector3(
                            _copiedScale.x / parentScale.x,
                            _copiedScale.y / parentScale.y,
                            _copiedScale.z / parentScale.z
                        );
                        
                        EditorUtility.SetDirty(transform);
                    }
                }
            }
        }

        public string GetShortName()
        {
            return "CWT";
        }
    }
}