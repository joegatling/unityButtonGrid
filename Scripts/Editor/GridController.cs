using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Grids
{
    [InitializeOnLoad]
    public class GridController
    {
        public static string portName
        {
            get => EditorPrefs.GetString("GridSerialPort", "");
            set
            {
                if(value != portName)
                {
                    EditorPrefs.SetString("GridSerialPort", value);

                    if (value.Length > 0)
                    {
                        grid.Connect(value);
                    }
                    else
                    {
                        grid.Disconnect();
                    }
                }
            }
        }

        public static string configAssetName
        {
            get => EditorPrefs.GetString("GridConfigurationAsset", "");
            set => EditorPrefs.SetString("GridConfigurationAsset", value);            
        }

        public static GridConfig gridConfig
        {
            get
            {
                if(_config == null && configAssetName.Length > 0)
                {
                    _config = AssetDatabase.LoadAssetAtPath<GridConfig>(configAssetName);
                }

                return _config;
            }

            set
            {
                if(_config != value)
                {
                    TeardownAllButtonHandlers();

                    _config = value;

                    if(_config == null)
                    {
                        configAssetName = "";
                    }
                    else
                    {
                        configAssetName = AssetDatabase.GetAssetPath(_config);
                        InitializeAllButtonsHandlers();
                    }
                }
            }
        }

        public static Grid64 grid { get; private set; }

        private static GridConfig _config = default;

        private static bool _wasPlaying = false;

        private static List<IButtonHandler> _initializedButtonHandlers = new List<IButtonHandler>();

        private static List<GlowingButton> _buttons = new List<GlowingButton>();

        static GridController()
        {
            EditorApplication.update += Update;

            grid = new Grid64();
            if (portName != null && grid.IsPortAvailable(portName))
            {
                grid.Connect(portName);
            }

            grid.onButtonStateChanged += OnButtonStateChanged;

            _wasPlaying = EditorApplication.isPlaying;
            
            for(int i = 0; i < grid.width * grid.height; i++)
            {
                _buttons.Add(new GlowingButton());
            }

            InitializeAllButtonsHandlers();
        }

        private static void TeardownAllButtonHandlers()
        {
            for(int i = 0; i < _initializedButtonHandlers.Count; i++)
            {
                if(_initializedButtonHandlers[i] != null)
                {
                    _initializedButtonHandlers[i].Teardown();
                }
                _initializedButtonHandlers[i] = null;
            }
        }

        private static void InitializeAllButtonsHandlers()
        {
            TeardownAllButtonHandlers();        

            for(int y = 0; y < grid.height; y++)
            {
                for(int x = 0; x < grid.width; x++)
                {
                    IButtonHandler buttonHandler = gridConfig != null ? gridConfig.GetButtonHandler(x,y) : null;

                    if(buttonHandler != null)
                    {
                        var button = GetButton(x,y);
                        buttonHandler.Initialize(button);
                    }

                    _initializedButtonHandlers.Add(buttonHandler);
                }
            }
        }

        private static GlowingButton GetButton(int x, int y)
        {
            return _buttons[y * grid.width + x];
        }

        private static IButtonHandler GetInitializedButtonHandler(int x, int y)
        {
            return _initializedButtonHandlers[y * grid.width + x];
        }

        private static void InitializeButtonHandler(int x, int y, IButtonHandler buttonHandler)
        {
            int index = y * grid.width + x;

            if(_initializedButtonHandlers[index] != null)
            {
                _initializedButtonHandlers[index].Teardown();
            }

            _initializedButtonHandlers[index] = buttonHandler;

            if(_initializedButtonHandlers[index] != null)
            {
                _initializedButtonHandlers[index].Initialize(GetButton(x,y));            
            }
        }

        private static void OnButtonStateChanged(Vector2Int coords, bool state)
        {
            //GetButton(coords.x, coords.y).Update(state);
            //if (coords.x == 0 && coords.y == 0)
            //{
            //    if (state == false)
            //    {
            //        Debug.Log("Play");
            //        EditorApplication.isPlaying = !EditorApplication.isPlaying;
            //    }
            //}
        }

        static void Update()
        {
            grid.Update();


            for(int y = 0; y < grid.height; y++)
            {
                for(int x = 0; x < grid.width; x++)
                {
                    var button = GetButton(x,y);
                    var index = y * grid.width + x;
                    var buttonHandler = gridConfig?.GetButtonHandler(x,y);

                    button.Update(grid.GetButtonState(x,y));

                    // Check to see if the buttonhandler has changed.
                    if(buttonHandler != GetInitializedButtonHandler(x,y))
                    {
                        InitializeButtonHandler(x,y,buttonHandler);
                    }

                    if(buttonHandler != null)
                    {
                        buttonHandler.Update();
                    }

                    grid.SetLed(x, y, button.led, false);
                }
            }      

            grid.RefreshLedStates();                  
        }         
    }
}