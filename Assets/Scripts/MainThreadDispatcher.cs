// References:
// https://answers.unity.com/questions/305882/how-do-i-invoke-functions-on-the-main-thread.html#answer-1417505
// https://github.com/PimDeWitte/UnityMainThreadDispatcher
// https://stackoverflow.com/a/41333540
// https://stackoverflow.com/a/43283451
 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
 
public class MainThreadDispatcher : MonoBehaviour
{
    public static void Enqueue(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException();
        }
        lock (s_locker)
        {
            s_nextActions.Enqueue(action);
        }
    }
     
    public static void Enqueue<T>(Action<T> action, T parameter)
    {
        if (action == null)
        {
            throw new ArgumentNullException();
        }
        Enqueue(() => action(parameter));
    }
     
    public static void Enqueue(IEnumerator coroutine)
    {
        if (coroutine == null)
        {
            throw new ArgumentNullException();
        }
        Enqueue(() => s_instance.StartCoroutine(coroutine));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (s_instance == null)
        {
            var gameObject = new GameObject(nameof(MainThreadDispatcher));
            DontDestroyOnLoad(gameObject);
            s_instance = gameObject.AddComponent<MainThreadDispatcher>();
        }
    }       
     
    private void Update()
    {
        // Use the double buffering pattern. It avoids deadlocks and reduces idle time.
        lock (s_locker)
        {
            var temporaryActions = s_currentActions;
            s_currentActions = s_nextActions;
            s_nextActions = temporaryActions;
        }

        while (s_currentActions.Count > 0)
        {
            var action = s_currentActions.Dequeue();
            Do(action);
        }
    }

    private void Do(Action action)
    {
        // If Target is destroyed, then do nothing.
        if (action.Target == null)
        {
            var methodInfo = action.Method;
            if (methodInfo == null || !methodInfo.IsStatic)
            {
                return;
            }
        }
        if (action.Target as UnityEngine.Object == null)
        {
            return;
        }

        action();
    }

    private static MainThreadDispatcher s_instance;
    private static Queue<Action> s_nextActions = new Queue<Action>();
    private static Queue<Action> s_currentActions = new Queue<Action>();
    private static object s_locker = new object();
}
