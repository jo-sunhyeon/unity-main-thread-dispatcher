// References:
// https://answers.unity.com/questions/305882/how-do-i-invoke-functions-on-the-main-thread.html#answer-1417505
// https://github.com/PimDeWitte/UnityMainThreadDispatcher
// https://stackoverflow.com/a/41333540
// https://stackoverflow.com/a/43283451
 
using System;
using System.Collections;
using System.Collections.Generic;
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
			nextActions.Add(action);
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
			instance = new GameObject(nameof(MainThreadDispatcher)).AddComponent<MainThreadDispatcher>();
			DontDestroyOnLoad(instance.gameObject);
		}
	}       
	 
	private void Update()
	{
		// Use the double buffering pattern. It avoids deadlocks and reduces idle time.
		lock (locker)
		{
			var temporary = currentActions;
			currentActions = nextActions;
			nextActions = temporary;
		}
		 
		foreach (var action in currentActions)
		{
			action();
		}
		currentActions.Clear();
	}
	 
	private static MainThreadDispatcher instance;
	private static List<Action> nextActions = new List<Action>();
	private static List<Action> currentActions = new List<Action>();
	private static object locker = new object();
}
