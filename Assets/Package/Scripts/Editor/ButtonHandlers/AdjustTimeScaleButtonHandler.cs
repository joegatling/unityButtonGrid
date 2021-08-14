using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JoeGatling.ButtonGrids.ButtonHandlers
{    
    [System.Serializable]
    [HandlerName("Time/Adjust Time Scale")]
    public class AdjustTimeScaleButtonHandler : IButtonHandler
    {
        public enum TimeAdjustmentType
        {
            Reset,
            DecreaseTimeScale,
            IncreaseTimeScale
        }

        [SerializeField] private TimeAdjustmentType _adjustment = TimeAdjustmentType.Reset;

        private GlowingButton _button = null;        
        public void Initialize(GlowingButton button)
        {
            Teardown();

            _button = button;
            _button.onKeyStateChanged += OnButtonStateChanged;            
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
            if(_button.key == true)
            {
                _button.ledFunction = LedFunctions.LedOn.instance;
            }
            else
            {
                if(_adjustment == TimeAdjustmentType.Reset)
                {
                    if(Mathf.Approximately(Time.timeScale, 1.0f))
                    {
                        _button.ledFunction = LedFunctions.LedFlashing.fast;
                    }
                    else
                    {
                        _button.ledFunction = LedFunctions.LedOn.instance;
                    }
                }
                else if(_adjustment == TimeAdjustmentType.DecreaseTimeScale)
                {
                    if(Time.timeScale <  1.0f)
                    {
                        if(Time.timeScale < 1/4.0f)
                        {
                            _button.ledFunction = LedFunctions.LedPulse.veryFast;
                        }
                        else if(Time.timeScale < 1/2.0f)
                        {
                            _button.ledFunction = LedFunctions.LedPulse.fast;
                        }
                        else
                        {
                            _button.ledFunction = LedFunctions.LedPulse.slow;
                        }                        
                    }
                    else
                    {
                        _button.ledFunction = LedFunctions.LedOn.instance;
                    }                
                }    
                else if(_adjustment == TimeAdjustmentType.IncreaseTimeScale)
                {
                    if(Time.timeScale >  1.0f)
                    {
                        if(Time.timeScale > 4)
                        {
                            _button.ledFunction = LedFunctions.LedPulse.veryFast;
                        }
                        else if(Time.timeScale > 2)
                        {
                            _button.ledFunction = LedFunctions.LedPulse.fast;
                        }
                        else
                        {
                            _button.ledFunction = LedFunctions.LedPulse.slow;
                        }                        
                    }
                    else
                    {
                        _button.ledFunction = LedFunctions.LedOn.instance;
                    }                  
                } 
            }
        }

        protected void OnButtonStateChanged(bool state)
        {
            if(state == false)
            {
                if(_adjustment == TimeAdjustmentType.Reset)
                {
                    Time.timeScale = 1.0f;
                }
                else if(_adjustment == TimeAdjustmentType.DecreaseTimeScale)
                {
                    Time.timeScale /= 2.0f;
                }    
                else if(_adjustment == TimeAdjustmentType.IncreaseTimeScale)
                {
                    Time.timeScale *= 2.0f;
                }    
            }
        }
    }
}