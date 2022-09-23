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
     
    public static void Enqueue<T>(Action<T> action, T parameter)
    {
        if (action == null)
        {
            throw new ArgumentNullException();
        }
		implementation.Enqueue(() => action(parameter));
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
            GameObject gameObject = new GameObject(nameof(MainThreadDispatcher));
            DontDestroyOnLoad(gameObject);
            instance = gameObject.AddComponent<MainThreadDispatcher>();
			instance.canDoAction = action => action.Target as UnityEngine.Object != null;
        }
    }       
     
    private void Update()
    {
		implementation.Update();
    }

	private static ThreadDispatcher implementation;
    private static UnityMainThreadDispatcher instance;
}