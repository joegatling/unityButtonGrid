using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO.Ports;
using System.Linq;
using JoeGatling.ButtonGrids.ButtonHandlers;

namespace JoeGatling.ButtonGrids
{
    public class GridWindow : EditorWindow
    {
        bool _redrawRequred = false;

        Vector2Int _selectedButton = new Vector2Int(0, 0);


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

            EditorGUILayout.Space();

            if(GridController.gridConfig != null)
            {
                for (int y = 7; y >= 0; y--)
                {
                    EditorGUILayout.BeginHorizontal();
                    for (int x = 0; x < 8; x++)
                    {

                        IButtonHandler buttonHandler = GridController.gridConfig.GetButtonHandler(x, y);

                        Color baseColor = Color.white;
                        float brightness = 1.0f;


                        if (GridController.grid.isConnected)
                        {
                            baseColor = buttonHandler != null ? Color.white : Color.grey;

                            brightness = GridController.grid.GetLed(x, y) ? 1.0f : 0.8f;

                            if (GridController.grid.GetButtonState(x, y))
                            {
                                brightness *= 0.75f;
                            }
                        }

                        if(buttonHandler == null)
                        {
                            brightness *= 0.75f;
                        }

                        if (x == _selectedButton.x && y == _selectedButton.y)
                        {
                            baseColor = Color.cyan;
                        }


                        GUI.color = Color.Lerp(Color.black, baseColor, brightness);

                       

                        float width = Screen.width / EditorGUIUtility.pixelsPerPoint;                       

                        string typeDescription = buttonHandler != null ? buttonHandler.GetType().Name: "None";

                        string textToRemove = "ButtonHandler";
                        if(typeDescription.EndsWith(textToRemove))
                        {
                            typeDescription = typeDescription.Substring(0, typeDescription.Length - textToRemove.Length);
                        }

                        GUIContent buttonContent = new GUIContent("", ObjectNames.NicifyVariableName(typeDescription));

                        if (GUILayout.Button(buttonContent, GUILayout.Height(width / 8)))
                        {
                            //ShowButtonHandlerTypeMenu(x,y);
                            _selectedButton = new Vector2Int(x, y);
                        }


                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            GUI.color = Color.white;

            IButtonHandler selectedButtonHandler = GridController.gridConfig.GetButtonHandler(_selectedButton.x, _selectedButton.y);

            string typeName = "";
            if (selectedButtonHandler != null)
            {
                typeName = $"Type: {selectedButtonHandler.GetType().Name}";
            }
            else
            {
                typeName = "Select Type...";
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Button ({_selectedButton.x}, {_selectedButton.y})", EditorStyles.boldLabel);

            if (GUILayout.Button(new GUIContent(typeName)))
            {
                ShowButtonHandlerTypeMenu(_selectedButton.x, _selectedButton.y);
            }

            if (selectedButtonHandler != null)
            {
                var editor = Editor.CreateEditor(GridController.gridConfig);
                var property = editor.serializedObject.FindProperty("_handlers");
                property.isExpanded = true;
                var handlerProperty = property.GetArrayElementAtIndex(GridController.gridConfig.GridCoordsToIndex(_selectedButton.x, _selectedButton.y));
                //var editor = selectedButtonHandler.GetEditor();

                editor.serializedObject.Update();

                if(handlerProperty.hasVisibleChildren)
                {
                    EditorGUILayout.PropertyField(handlerProperty, new GUIContent("Properites"), true);
                }

                EditorGUILayout.Space();
                
                if(GUILayout.Button("Simulate Press"))
                {
                    GridController.grid.SetButtonOverride(_selectedButton.x, _selectedButton.y);
                }
                else
                {
                    GridController.grid.ClearButtonOverride(_selectedButton.x, _selectedButton.y);
                }

                editor.serializedObject.ApplyModifiedProperties();

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

#if UNITY_EDITOR_OSX
            var ports = SerialPort.GetPortNames().Where(x => x.Contains("usbmodem")).ToList();
#else
            var ports = SerialPort.GetPortNames().ToList();
#endif

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
            var buttonHandlerTypes = new List<System.Type>();
            var type = typeof(IButtonHandler);

            foreach(var a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try 
                {
                    buttonHandlerTypes.AddRange(a.GetTypes().Where(p => type.IsAssignableFrom(p) && p.IsClass).ToList());        
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            }
            return buttonHandlerTypes;
            //return System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(p => type.IsAssignableFrom(p) && p.IsClass).ToList();
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

                menu.AddItem(new GUIContent("Expand to all"), false, () =>
                {
                    for(int xx = 0; xx < 8; xx++)
                    {
                        for(int yy = 0; yy < 8; yy++)
                        {
                            if(!(xx == x && yy == y))
                            {
                                IButtonHandler newHandler = (IButtonHandler)System.Activator.CreateInstance(currentButtonHandler.GetType());
                                GridController.gridConfig.SetButtonHandler(xx, yy, newHandler);
                            }

                        }
                    }
                    
                });
                menu.AddItem(new GUIContent("Expand to Row"), false, () =>
                {
                    for (int xx = 0; xx < 8; xx++)
                    {
                        if (!(xx == x))
                        {
                            IButtonHandler newHandler = (IButtonHandler)System.Activator.CreateInstance(currentButtonHandler.GetType());
                            GridController.gridConfig.SetButtonHandler(xx, y, newHandler);
                        }
                    }
                });
                menu.AddItem(new GUIContent("Expand to Col"), false, () =>
                {
                    for (int yy = 0; yy < 8; yy++)
                    {
                        if (!(yy == y))
                        {
                            IButtonHandler newHandler = (IButtonHandler)System.Activator.CreateInstance(currentButtonHandler.GetType());
                            GridController.gridConfig.SetButtonHandler(x, yy, newHandler);
                        }
                    }
                });
                menu.AddSeparator("");
            }
            else
            {
                menu.AddItem(new GUIContent("Clear all"), false, () =>
                {
                    for(int xx = 0; xx < 8; xx++)
                    {
                        for(int yy = 0; yy < 8; yy++)
                        {
                            if(!(xx == x && yy == y))
                            {
                                GridController.gridConfig.SetButtonHandler(xx, yy, null);
                            }
                        }
                    }
                    
                });
                menu.AddItem(new GUIContent("Clear Row"), false, () =>
                {
                    for (int xx = 0; xx < 8; xx++)
                    {
                        if (!(xx == x))
                        {
                            GridController.gridConfig.SetButtonHandler(xx, y, null);
                        }
                    }
                });
                menu.AddItem(new GUIContent("Clear Col"), false, () =>
                {
                    for (int yy = 0; yy < 8; yy++)
                    {
                        if (!(yy == y))
                        {
                            GridController.gridConfig.SetButtonHandler(x, yy, null);
                        }
                    }
                });
                menu.AddSeparator("");                
            }            

            for (int i = 0; i < types.Count; i++)
            {
                var type = types[i];

                string menuName = type.Name;

                System.Attribute[] attributes = System.Attribute.GetCustomAttributes(type);

                foreach (System.Attribute attr in attributes)
                {
                    if (attr is HandlerName)
                    {
                        menuName = (attr as HandlerName).name;
                    }
                }

                menu.AddItem(new GUIContent(menuName), currentButtonHandler != null && currentButtonHandler.GetType() == type, () =>
                {
                    IButtonHandler newHandler = (IButtonHandler)System.Activator.CreateInstance(type);
                    GridController.gridConfig.SetButtonHandler(x,y,newHandler);                    
                });
            }

            



            menu.ShowAsContext();
        }        

    }
}
