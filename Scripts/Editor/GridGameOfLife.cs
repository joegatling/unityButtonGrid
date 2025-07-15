using JoeGatling.ButtonGrids.LedFunctions;
using UnityEditor;
using UnityEngine;

namespace JoeGatling.ButtonGrids
{
    public class GridGameOfLife
    {
        private int _width;
        private int _height;
        private bool[,] _cells;

        private double _nextUpdateTime = 0;
        private const double UpdateInterval = 3.0; // Update every second

        public GridGameOfLife(int width, int height)
        {
            _width = width;
            _height = height;
            _cells = new bool[width, height];

            Randomize();

            _nextUpdateTime = EditorApplication.timeSinceStartup + UpdateInterval;

            EditorApplication.update += EditorUpdate;
        }

        // when the object is destroyed
        ~GridGameOfLife()
        {
            EditorApplication.update -= EditorUpdate;
        }

        private void EditorUpdate()
        {
            // Has one second passed since the last update?
            if (EditorApplication.timeSinceStartup > _nextUpdateTime)
            {
                // Update the grid
                Update();
                _nextUpdateTime += UpdateInterval;
            }
        }


        public void SetCell(int x, int y, bool state)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                _cells[x, y] = state;
            }
        }

        public bool GetCell(int x, int y)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                return _cells[x, y];
            }
            return false;
        }

        public void Update()
        {
            bool[,] newCells = new bool[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int aliveNeighbors = CountAliveNeighbors(x, y);

                    if (_cells[x, y])
                    {
                        newCells[x, y] = aliveNeighbors == 2 || aliveNeighbors == 3;
                    }
                    else
                    {
                        newCells[x, y] = aliveNeighbors == 3;
                    }
                }
            }

            _cells = newCells;
        }

        private int CountAliveNeighbors(int x, int y, bool wrapAround = true)
        {               
            int count = 0;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = x + dx;
                    int ny = y + dy;   

                    if (wrapAround)
                    {
                        // Wrap around logic
                        if (nx < 0) nx += _width;
                        if (nx >= _width) nx -= _width;
                        if (ny < 0) ny += _height;
                        if (ny >= _height) ny -= _height;
                    }
                    else
                    {
                        // Out of bounds check
                        if (nx < 0 || nx >= _width || ny < 0 || ny >= _height)
                            continue;
                    }

                    if (GetCell(nx, ny))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public void Clear()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _cells[x, y] = false;
                }
            }
        }
        public void Randomize(float fillProbability = 0.5f)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _cells[x, y] = Random.value < fillProbability;
                }
            }
        }
    }

    public class GameOfLifeLedFunction : ILedFunction
    {
        private GridGameOfLife _gameOfLife;

        public GameOfLifeLedFunction(GridGameOfLife game)
        {
            _gameOfLife = game;
        }

        public bool GetLedState(GlowingButton button)
        {
            return _gameOfLife.GetCell(button.x, button.y);
        }
        
    }
}