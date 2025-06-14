using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Text;
using System.Threading;
using UnityEditor;

namespace JoeGatling.ButtonGrids
{
    public class Grid64
    {
        public int width => 8;
        public int height => 8;

        public event System.Action<Vector2Int, bool> onButtonStateChanged = delegate { };
        public event System.Action<Vector2Int, bool> onLedStateChanged = delegate { };

        private SerialPort _port = default;
        private Thread _dataReceiveThread = null;
        private volatile bool _shouldStopThread = false;

        private readonly object _serialDataLock = new object();
        private readonly object _portLock = new object();


        private string _serialData = "";

        private string[] separators = { " " };

        private readonly object _buttonStateLock = new object();
        private byte[] _buttonStates = new byte[8];
        private byte[] _ledStates = new byte[8];
        private byte[] _buttonOverrides = new byte[8];

        public bool isConnected => _port != null && _port.IsOpen;
        private bool _isLedStateDirty = false;

        private bool _hasConnectionError = false;

        public void Connect(string portName)
        {
            Disconnect();

            if (IsPortAvailable(portName))
            {
                lock (_portLock)
                {
                    _port = new SerialPort(portName);
                    _port.BaudRate = 115200;
                    _port.Parity = Parity.None;
                    _port.DataBits = 8;
                    _port.StopBits = StopBits.One;
                    _port.RtsEnable = true;
                    _port.DtrEnable = true;
                    _port.NewLine = "\r\n";
                    _port.WriteTimeout = 2000;
                    _port.ReadTimeout = 1000;

                    try
                    {
                        _port.Open();
                        _hasConnectionError = false;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e.Message);
                        _hasConnectionError = true;
                        _port.Close();
                        _port = null;
                    }

                    if (!_hasConnectionError)
                    {
                        _shouldStopThread = false;

                        if (_dataReceiveThread == null || _dataReceiveThread.IsAlive == false)
                        {
                            _dataReceiveThread = new Thread(ReadSerialData);
                            _dataReceiveThread.IsBackground = true;
                            _dataReceiveThread.Start();
                        }

                        Initialize();
                    }
                }
                
            }
        }

        private void Initialize()
        {
            for (int i = 0; i < height; i++)
            {
                _buttonStates[i] = 0;
                _buttonOverrides[i] = 0;
            }
            SetAllLeds(false);
        }

        public void Disconnect()
        {
            lock (_portLock)
            {
                _shouldStopThread = true;

                if (_dataReceiveThread != null && _dataReceiveThread.IsAlive)
                {
                    // Wait up to 1 second for thread to terminate, but do not abort forcibly
                    if (!_dataReceiveThread.Join(1000))
                    {
                        Debug.LogWarning("Data receive thread did not terminate in time.");
                    }
                    _dataReceiveThread = null;
                }

                if (_port != null)
                {
                    try
                    {
                        if (_port.IsOpen)
                        {
                            _port.Close();
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Error closing serial port: {e.Message}");
                    }
                    finally
                    {
                        _port = null;
                    }
                }
            }
        }

        public void Update()
        {
            if (isConnected)
            {
                string serialDataCopy = null;
                lock (_serialDataLock)
                {
                    if (_serialData != null && _serialData.Length > 0)
                    {
                        serialDataCopy = _serialData;
                        _serialData = "";
                    }
                }

                if (!string.IsNullOrEmpty(serialDataCopy))
                {
                    string[] tokens = serialDataCopy.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);

                    if (string.Compare(tokens[0], "/grid/key", System.StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (tokens.Length >= 4)
                        {
                            int x = 0;
                            int y = 0;
                            int state = 0;

                            if (int.TryParse(tokens[1], out x) &&
                                int.TryParse(tokens[2], out y) &&
                                int.TryParse(tokens[3], out state))
                            {
                                SetButtonState(x, y, state == 1);
                            }
                        }
                    }
                }

                if (_isLedStateDirty)
                {
                    RefreshLedStates();
                }
            }
        }

        void PortWriteLine(string command)
        {
            lock (_portLock)
            {
                if (_port != null && _port.IsOpen)
                {
                    try
                    {
                        _port.WriteLine(command);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error writing to serial port: {e.Message}");
                    }
                }
            }
        }

        byte SetBit(ref byte value, int bit, int bitValue)
        {
            if (bitValue == 1)
            {
                value = (byte)(value | (1 << bit));
            }
            else
            {
                value = (byte)(value & ~(1 << bit));
            }

            return value;
        }
        int GetBit(byte value, int bit)
        {
            return (byte)((value & 1 << bit) >> bit);
        }

        void SetButtonState(int x, int y, bool state)
        {
            lock (_buttonStateLock)
            {
                if (GetButtonState(x, y) != state)
                {
                    SetBit(ref _buttonStates[y], x, state ? 1 : 0);

                    onButtonStateChanged?.Invoke(new Vector2Int(x, y), state);
                }
            }
        }
        public bool GetButtonState(int x, int y)
        {
            lock (_buttonStateLock)
            {
                return (GetBit(_buttonStates[y], x) + GetBit(_buttonOverrides[y], x)) > 0;
            }
        }
        public void SetButtonOverride(int x, int y)
        {
            SetBit(ref _buttonOverrides[y], x, 1);
        }
        public void ClearButtonOverride(int x, int y)
        {
            SetBit(ref _buttonOverrides[y], x, 0);
        }

        public void ClearAllButtonOverrides(int x, int y)
        {
            for (int i = 0; i < height; i++)
            {
                _buttonOverrides[i] = 0;
            }
        }

        public bool GetLed(int x, int y)
        {
            lock (_buttonStateLock)
            {
                return GetBit(_ledStates[y], x) == 1 ? true : false;
            }
        }

        public void SetLed(int x, int y, bool state, bool immediate = true)
        {
            bool currentLedState = GetLed(x, y);

            if (currentLedState != state)
            {
                if (isConnected && immediate)
                {
                    PortWriteLine($"/grid/led/set {x} {y} {(state ? 1 : 0)}");                    
                }
                else
                {
                    _isLedStateDirty = true;
                }

                lock (_buttonStateLock)
                {
                    SetBit(ref _ledStates[y], x, state ? 1 : 0);
                }

                onLedStateChanged?.Invoke(new Vector2Int(x, y), state);
            }
        }
        public void SetLed(Vector2Int coords, bool state, bool immediate = true)
        {
            SetLed(coords.x, coords.y, state, immediate);
        }

        public void SetAllLeds(bool state)
        {
            lock (_buttonStateLock)
            {
                for (int y = 0; y < 8; y++)
                {
                    _ledStates[y] = (byte)(state ? 255 : 0);
                }
            }

            if (isConnected)
            {
                PortWriteLine($"/grid/led/all {(state ? 1 : 0)}");
            }
            else
            {
                _isLedStateDirty = true;
            }
        }

        public void RefreshLedStates()
        {
            if (isConnected)
            {
                string command = "/grid/led/map 0 0 ";

                lock (_buttonStateLock)
                {
                    for (int y = 0; y < height; y++)
                    {
                        command += $"{_ledStates[y]} ";
                    }
                }

                PortWriteLine(command);

                _isLedStateDirty = false;
            }
        }

        private void ReadSerialData()
        {
            while (!_shouldStopThread && _port != null && _port.IsOpen)
            {
                lock(_portLock)
                {
                    try
                    {
                        if (_shouldStopThread) break;

                        if (_port.BytesToRead > 0)
                        {
                            string data = _port.ReadLine();

                            lock (_serialDataLock)
                            {
                                _serialData = data;
                            }
                        }
                        else
                        {
                            // Release lock while sleeping to avoid deadlock
                            Monitor.Exit(_portLock);
                            Thread.Sleep(10); // Sleep to avoid busy waiting
                            Monitor.Enter(_portLock);                            
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if (!(ex is ThreadAbortException))
                        {
                            Debug.LogError($"Exception: {ex.GetType()}");
                            if (ex.Message.Length > 0)
                            {
                                Debug.LogError(ex.Message);
                            }
                        }

                        // Clean up and exit thread
                        try
                        {
                            _port?.Close();
                        }
                        catch { }
                        _port = null;
                        _shouldStopThread = true;
                        break;
                    }
                }
            }
        }

        public bool IsPortAvailable(string name)
        {
            List<string> portNames = new List<string>(SerialPort.GetPortNames());
            return portNames.Contains(name);
        }
    }
}
