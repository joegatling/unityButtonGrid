using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

public static class MainThreadDispatcher
{
    private static readonly Queue<Action> _mainThreadQueue = new Queue<Action>();

    // Call this from any thread to schedule an action on the main thread
    public static void RunOnMainThread(Action action)
    {
        lock (_mainThreadQueue)
        {
            _mainThreadQueue.Enqueue(action);
        }
    }

    // Call this during Editor update to flush the queue
    [InitializeOnLoadMethod]
    private static void Init()
    {
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        lock (_mainThreadQueue)
        {
            while (_mainThreadQueue.Count > 0)
            {
                var action = _mainThreadQueue.Dequeue();
                action?.Invoke();
            }
        }
    }
}