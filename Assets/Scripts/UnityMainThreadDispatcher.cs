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
 
public class UnityMainThreadDispatcher : MonoBehaviour
{
    public static void Enqueue(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException();
        }
		implementation.Enqueue(action);
    }
     
    public static void Enqueue<T1>(Action<T1> action, T1 parameter1)
    {
        if (action == null)
        {
            throw new ArgumentNullException();
        }
		implementation.Enqueue(() => action(parameter1));
    }
     
    public static void Enqueue<T1, T2>(Action<T1, T2> action, T1 parameter1, T2 parameter2)
    {
        if (action == null)
        {
            throw new ArgumentNullException();
        }
		implementation.Enqueue(() => action(parameter1, parameter2));
    }
     
    public static void Enqueue(IEnumerator coroutine)
    {
        if (coroutine == null)
        {
            throw new ArgumentNullException();
        }
		implementation.Enqueue(() => instance.StartCoroutine(coroutine));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (instance == null)
        {
            GameObject gameObject = new GameObject(nameof(UnityMainThreadDispatcher));
            DontDestroyOnLoad(gameObject);
            instance = gameObject.AddComponent<UnityMainThreadDispatcher>();
        }
    }       
     
    private void Update()
    {
		implementation.Update();
    }

	private static ThreadDispatcher implementation = new ThreadDispatcher();
    private static UnityMainThreadDispatcher instance;
}
