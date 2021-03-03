using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO.Ports;
using System.Linq;

namespace Grids
{
    public class GridWindow : EditorWindow
    {
        bool _redrawRequred = false;

        [MenuItem("Joe/ButtonGrid Window")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            GridWindow window = (GridWindow)EditorWindow.GetWindow(typeof(GridWindow));

            window.Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Button Grid", AssetDatabase.LoadAssetAtPath<Texture>("Assets/Icons/Icon_buttonGridWindow.png"));

            GridController.grid.onButtonStateChanged += OnGridStateChanged;
            GridController.grid.onLedStateChanged += OnGridStateChanged;
        }


        private void Update()
        {
            if(_redrawRequred)
            {
                _redrawRequred = false;

                Repaint();
            }
        }

        void OnGridStateChanged(Vector2Int coords, bool state)
        {
            _redrawRequred = true;
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Device", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (EditorGUILayout.DropdownButton(new GUIContent(string.IsNullOrEmpty(GridController.portName) ? "Select Serial Device" : GridController.portName), FocusType.Passive))
            {
                ShowPortMenu();
            }

            if(GridController.grid.isConnected)
            {
                if(GUILayout.Button("Disconnect", GUILayout.Width(100)))
                {
                    GridController.grid.Disconnect();
                }
            }
            else
            {
                if(GUILayout.Button("Connect", GUILayout.Width(100)))
                {
                    GridController.grid.Connect(GridController.portName);
                }                
            }
            
            EditorGUILayout.EndHorizontal();

            GridController.gridConfig = (GridConfig)EditorGUILayout.ObjectField("Configuration", GridController.gridConfig, typeof(GridConfig), false);
            //EditorGUILayout.ObjectField("Configuration", GridController.grid, typeof(GridConfig), false);

            if(GridController.gridConfig != null)
            {
                for (int y = 7; y >= 0; y--)
                {
                    EditorGUILayout.BeginHorizontal();
                    for (int x = 0; x < 8; x++)
                    {

                        IButtonHandler buttonHandler = GridController.gridConfig.GetButtonHandler(x,y);

                        if(GridController.grid.isConnected)
                        {
                            Color buttonColor = GridController.grid.GetLed(x, y) ? Color.blue : (buttonHandler != null ? Color.white : Color.grey);

                            if(GridController.grid.GetButtonState(x,y))
                            {
                                buttonColor = Color.Lerp(buttonColor, Color.black, 0.5f);
                            }
                            
                            GUI.color = buttonColor;
                        }

                        string typeName = "";
                        if(buttonHandler != null)
                        {
                            typeName = buttonHandler.GetType().Name;
                        }


                        if (GUILayout.Button(new GUIContent("", null, typeName), GUILayout.Height(50)))
                        { 
                            ShowButtonHandlerTypeMenu(x,y);
                        
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            // if (GridController.grid.isConnected)
            // {

            //     EditorGUILayout.Space();



            //     GUI.color = Color.white;
            // }
            // else
            // {
            //     if(!string.IsNullOrEmpty(GridController.portName))
            //     {
            //         if(GUILayout.Button("Connect"))
            //         {
            //             GridController.grid.Connect(GridController.portName);
            //         }
            //     }
            // }
        }



        private void ShowPortMenu()
        {
            GenericMenu menu = new GenericMenu();

            var ports = SerialPort.GetPortNames().Where(x => x.Contains("usbmodem")).ToList();

            for (int i = 0; i < ports.Count; i++)
            {
                string newPortName = ports[i];
                menu.AddItem(new GUIContent(newPortName.Replace("/", "\u2215")), string.Compare(GridController.portName, newPortName, System.StringComparison.Ordinal) == 0, () =>
                {
                    GridController.portName = newPortName;
                });
            }

            menu.ShowAsContext();
        }


        List<System.Type> GetButtonHandlerTypes()
        {
            //var buttonHandlerTypes = new List<System.Type>();
            var type = typeof(IButtonHandler);
            return System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(p => type.IsAssignableFrom(p) && p.IsClass).ToList();
        }
        private void ShowButtonHandlerTypeMenu(int x, int y)
        {
            GenericMenu menu = new GenericMenu();

            var types = GetButtonHandlerTypes();
            IButtonHandler currentButtonHandler = GridController.gridConfig.GetButtonHandler(x,y);

            if(currentButtonHandler != null)
            {
                menu.AddItem(new GUIContent("None"), false, () => 
                {
                    GridController.gridConfig.SetButtonHandler(x,y,null);
                });
                menu.AddSeparator("");
            }            

            for (int i = 0; i < types.Count; i++)
            {
                var type = types[i];
                menu.AddItem(new GUIContent(type.Name), currentButtonHandler != null && currentButtonHandler.GetType() == type, () =>
                {
                    IButtonHandler newHandler = (IButtonHandler)System.Activator.CreateInstance(type);
                    GridController.gridConfig.SetButtonHandler(x,y,newHandler);                    
                });
            }

            

            menu.ShowAsContext();
        }        

    }
}
