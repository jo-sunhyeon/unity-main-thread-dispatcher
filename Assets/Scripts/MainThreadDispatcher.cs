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
        lock (locker)
        {
            nextActions.Enqueue(action);
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
        Enqueue(() => instance.StartCoroutine(coroutine));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (instance == null)
        {
            GameObject gameObject = new GameObject(nameof(MainThreadDispatcher));
            DontDestroyOnLoad(gameObject);
            instance = gameObject.AddComponent<MainThreadDispatcher>();
        }
    }       
     
    private void Update()
    {
        // Use the double buffering pattern. Because it avoids deadlocks and reduces idle time.
        lock (locker)
        {
            Queue<Action> temporaryActions = currentActions;
            currentActions = nextActions;
            nextActions = temporaryActions;
        }

        while (currentActions.Count > 0)
        {
            Action action = currentActions.Dequeue();
            Do(action);
        }
    }

    private static void Do(Action action)
    {
        // Prevent access to the destroyed non-static target.
        if (action.Target == null)
        {
            MethodInfo methodInfo = action.Method;
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

    private static MainThreadDispatcher instance;
    private static Queue<Action> nextActions = new Queue<Action>();
    private static Queue<Action> currentActions = new Queue<Action>();
    private static object locker = new object();
}
