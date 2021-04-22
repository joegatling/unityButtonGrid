using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JoeGatling.ButtonGrids.ButtonHandlers;

namespace JoeGatling.ButtonGrids
{
    [CreateAssetMenu(menuName = "Joe/Grids/Grid Config")]
    public class GridConfig : ScriptableObject
    {
        [SerializeReference] private List<IButtonHandler> _handlers = new List<IButtonHandler>();

        public int GridCoordsToIndex(int x, int y)
        {
            return y * 8 + x;
        }

        private void OnValidate()
        {
            if(_handlers.Count > 64)
            {
                _handlers.RemoveRange(64, _handlers.Count - 64);
            }
        }

        public void SetButtonHandler(int x, int y, IButtonHandler handler)
        {
            if(x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                int index = GridCoordsToIndex(x, y);
                
                while(_handlers.Count <= index)
                {
                    _handlers.Add(null);
                }

                _handlers[index] = handler;

                UnityEditor.EditorUtility.SetDirty(this);
            }

            
        }

        public IButtonHandler GetButtonHandler(int x, int y)
        {
            int index = GridCoordsToIndex(x, y);

            if (_handlers.Count <= index)
            {
                return null;
            }
            else
            {
                return _handlers[index];
            }
        }
    }
}