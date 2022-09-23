// References:
// https://answers.unity.com/questions/305882/how-do-i-invoke-functions-on-the-main-thread.html#answer-1417505
// https://github.com/PimDeWitte/UnityMainThreadDispatcher
// https://stackoverflow.com/a/41333540
// https://stackoverflow.com/a/43283451
 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
 
public class ThreadDispatcher
{
    public void Enqueue(Action action)
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
     
    public void Update()
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

    private void Do(Action action)
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
        if (canDoAction != null && !canDoAction())
        {
            return;
        }

        action();
    }
	
	public Action canDoAction;
    private Queue<Action> nextActions = new Queue<Action>();
    private Queue<Action> currentActions = new Queue<Action>();
    private object locker = new object();
}