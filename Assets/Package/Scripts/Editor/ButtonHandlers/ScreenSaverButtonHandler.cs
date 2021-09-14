using System.Collections;
using System.Collections.Generic;
using JoeGatling.ButtonGrids.LedFunctions;
using UnityEditor;
using UnityEngine;

namespace JoeGatling.ButtonGrids.ButtonHandlers
{
    [System.Serializable]
    [HandlerName("Screen Saver")]
    public class ScreenSaverButtonHandler : IButtonHandler
    {
        [SerializeField] private float _buttonCycleTime = 2.0f;

        private GlowingButton _button = null;
        private GlowingButton _overrideButton = null;

        private ILedFunction _ledFunction;

        public Vector2Int currentPosition { get; private set; }

        private double _nextButtonCycleTime = 0;
        private bool _isActive = false;
        private bool _discardNextRelease = false;
        
        private List<int> _cellWeights = null;

        public void Initialize(GlowingButton button)
        {
            Teardown();

            int totalButtons = GridController.grid.width * GridController.grid.height;
            _cellWeights = new List<int>(GridController.grid.width * GridController.grid.height);
            for(int i = 0; i < totalButtons; i++)
            {
                _cellWeights.Add(1);
            }

            _ledFunction = new ScreenSaverLedFunction(this);

            _button = button;
            _button.onKeyStateChanged += OnButtonStateChanged;
            _button.ledFunction = LedOn.instance;

            _overrideButton = new GlowingButton(0,0);
            _overrideButton.onKeyStateChanged += OnOverrideButtonStateChanged;
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
            if(UnityEditor.EditorApplication.timeSinceStartup > _nextButtonCycleTime)
            {
                IncrementAllCellWeights();
                Vector2Int targetPosition = GetWeightedRandomTarget();
                SetCellWeight(targetPosition, 0);
                currentPosition = targetPosition;

                _nextButtonCycleTime += _buttonCycleTime;
            }
        }

        private Vector2Int GetWeightedRandomTarget()
        {
            List<Vector2Int> possibleTargets = new List<Vector2Int>(4);
            

            if (currentPosition.x > 0)
            {
                possibleTargets.Add(currentPosition + Vector2Int.left);
            }

            if (currentPosition.x < GridController.grid.width - 1)
            {
                possibleTargets.Add(currentPosition + Vector2Int.right);
            }

            if (currentPosition.y > 0)
            {
                possibleTargets.Add(currentPosition + Vector2Int.down);
            }

            if (currentPosition.y < GridController.grid.height - 1)
            {
                possibleTargets.Add(currentPosition + Vector2Int.up);
            }

            int totalWeight = 0;

            foreach(var targetCoordinate in possibleTargets)
            {
                totalWeight += GetCellWeight(targetCoordinate);
            }

            int randomWeight = Random.Range(0,totalWeight);

            for(int i = 0; i < possibleTargets.Count; i++)
            {
                if(randomWeight < GetCellWeight(possibleTargets[i]))
                {
                    return possibleTargets[i];
                }
                else
                {
                    randomWeight -= GetCellWeight(possibleTargets[i]);
                }
            }

            return possibleTargets[possibleTargets.Count - 1];

        }

        protected void OnButtonStateChanged(bool state)
        {
            if(state == false)
            {
                _isActive = true;
                _discardNextRelease = true;
                currentPosition = _button.position;
                _nextButtonCycleTime = UnityEditor.EditorApplication.timeSinceStartup + _buttonCycleTime;

                GridController.overrideLedFunction = _ledFunction;
                GridController.overrideButton = _overrideButton;
            }
        }

        protected void OnOverrideButtonStateChanged(bool state)
        {
            if(state == false)
            {
                if (_isActive)
                {
                    _isActive = false;

                    GridController.overrideLedFunction = null;
                    GridController.overrideButton = null;
                }

                _discardNextRelease = false;
            }
        }

        private void IncrementAllCellWeights()
        {
            for (int i = 0; i < GridController.grid.width * GridController.grid.height; i++)
            {
                _cellWeights[i]++;
            }
        }

        private int GetCellWeight(Vector2Int coordinate)
        {
            return _cellWeights[coordinate.y * GridController.grid.width + coordinate.x];
        }

        private void SetCellWeight(Vector2Int coordinate, int value)
        {
            _cellWeights[coordinate.y * GridController.grid.width + coordinate.x] = value; ;
        }
    }

    public class ScreenSaverLedFunction : ILedFunction
    {
        ScreenSaverButtonHandler _screenSaverHandler = default;

        public ScreenSaverLedFunction(ScreenSaverButtonHandler screenSaverHandler)
        {
            _screenSaverHandler = screenSaverHandler;
        }

        public bool GetLedState(GlowingButton button)
        {
            if (_screenSaverHandler != null)
            {
                return button.position == _screenSaverHandler.currentPosition;
            }
            else
            {
                return false;
            }
        }
    }
}