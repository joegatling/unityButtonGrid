using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Grids.ButtonHandlers
{
    [System.Serializable]
    public class SaveSelectionButtonHandler : IButtonHandler
    {
        [System.Serializable]
        private class SelectedTransformInfo
        {
            public string root = default;
            public string path = default;
            public string scenePath = default;
            public bool  isActiveObject = false;
        }

        [SerializeReference] Object _activeObject = null;
        [SerializeReference] List<Object> _objects = null;
        [SerializeField] List<SelectedTransformInfo> _selectedTransformInfo = null;
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
                        SaveSelection();
                        _button.ledFunction = LedFunctions.LedFlashing.veryFast;
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
                    LoadSelection();
                   
                }

                _pressComplete = false;

                _button.ledFunction = _defaultLedFunction;
            }

        }


        private void SaveSelection()
        {
            _objects = new List<Object>(UnityEditor.Selection.objects);
            _activeObject = UnityEditor.Selection.activeObject;

            _selectedTransformInfo = new List<SelectedTransformInfo>();
            for(int i = 0; i < UnityEditor.Selection.transforms.Length; i++)
            {
                var objectInfo = new SelectedTransformInfo();

                objectInfo.root = UnityEditor.Selection.transforms[i].root.name;
                objectInfo.path = UnityEditor.AnimationUtility.CalculateTransformPath(UnityEditor.Selection.transforms[i], UnityEditor.Selection.transforms[i].root);
                objectInfo.scenePath = UnityEditor.Selection.transforms[i].gameObject.scene.path;
                objectInfo.isActiveObject = UnityEditor.Selection.activeTransform == UnityEditor.Selection.transforms[i];

                _selectedTransformInfo.Add(objectInfo);
            }

            _hasSavedData = true;
        }

        private void LoadSelection()
        {
            if(_hasSavedData)
            {
                UnityEditor.Selection.activeObject = _activeObject;
                UnityEditor.Selection.objects = _objects.ToArray();

                if(UnityEditor.Selection.objects.Length != _objects.Count && _selectedTransformInfo.Count > 0)
                {
                    Debug.Log("Test");

                    GameObject[] foundGameObjects = new GameObject[_selectedTransformInfo.Count];
                    GameObject activeGameObject = null;

                    for(int i = 0; i < _selectedTransformInfo.Count; i++)
                    {                        
                        var transformInfo = _selectedTransformInfo[i];
                        Scene scene = SceneManager.GetSceneByPath(transformInfo.scenePath);

                        if(scene.IsValid() && scene.isLoaded)
                        {
                            var rootGameObject = scene.GetRootGameObjects().Where(x => x.name == transformInfo.root).FirstOrDefault();

                            if(rootGameObject != null)
                            {
                                if(transformInfo.path.Length == 0)
                                {
                                    foundGameObjects[i] = rootGameObject;
                                }
                                else
                                {
                                    foundGameObjects[i] = rootGameObject.transform.Find(transformInfo.path).gameObject;
     
                                }

                                if(transformInfo.isActiveObject)
                                {
                                    activeGameObject = foundGameObjects[i];
                                }
                            }


                        }
                        //UnityEditor.AnimationUtility.GetAnimatedObject()
                    }

                    UnityEditor.Selection.objects = foundGameObjects;
                    UnityEditor.Selection.activeGameObject = activeGameObject;
                    
                }

            }
        }

        
    }


}