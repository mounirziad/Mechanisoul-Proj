#nullable enable

using System;
using Unity.Behavior;
using UnityEngine;
using Composite = Unity.Behavior.Composite;
using Unity.Properties;
using System.Reflection;
using System.Collections.Generic;


//Code uses reflection (non-public and public) to find child call methods. Avoids compile time mismatches that caused errors with previous script
//Will assume running as a fallback if wanting to change that just make rawStatusObj == null and it will block that
//Good for boss but don't use large scale as it does use Linq

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Selector", story: "Run children in order until one succeeds", category: "Flow", id: "cedf5bfa1f7458a780fca892df02c1dc")]
public partial class SelectorSequence : Composite
{
    private int currentIndex = 0;
    private bool childStarted = false;
    private bool initialized = false;

    private bool IsResurrectionActive = false;
    private bool isAttacking = false;

    MethodInfo? childStartMethod = null;
    MethodInfo? childUpdateMethod = null;
    MethodInfo? childEndMethod = null;

    private readonly Dictionary<string, MethodInfo> methodCache = new();

    void EnsureChildMethods()
    {
        if (childStartMethod != null || childUpdateMethod != null || childEndMethod != null)
        {
            return;
        }

        var t = this.GetType().BaseType ?? typeof(Composite); //try base type first

        //Search both public and non-public instance methods on the base type and on this type
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        //look for likely method names and pick compatible overloads
        string[] startNames = new[] { "StartNode", "StartChild", "Start" };
        string[] updateNames = new[] { "UpdateNode", "UpdateChild", "Update" };
        string[] endNames = new[] { "EndNode", "EndChild", "End" };

        childStartMethod = FindBestMethod(t, startNames, flags);
        childUpdateMethod = FindBestMethod(t, updateNames, flags);
        childEndMethod = FindBestMethod(t, endNames, flags);

        if (childStartMethod == null) childStartMethod = FindBestMethod(this.GetType(), startNames, flags);
        if (childUpdateMethod == null) childUpdateMethod = FindBestMethod(this.GetType(), updateNames, flags);
        if (childEndMethod == null) childEndMethod = FindBestMethod(this.GetType(), endNames, flags);
    }

    MethodInfo? FindBestMethod(Type t, string[] candidateNames, BindingFlags flags)
    {
        foreach (var name in candidateNames)
        {
            if (!methodCache.TryGetValue(name, out var cachedMethod))
            {
                var methods = t.GetMethods(flags);
                cachedMethod = Array.Find(methods, m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
                methodCache[name] = cachedMethod;
            }

            if (cachedMethod != null)
            {
                return cachedMethod;
            }
        }
        return null;
    }   

    object InvokeChildMethod(MethodInfo method, int childIndex)
    {
        if (method == null)
            throw new InvalidOperationException("Child method not found via reflection.");

        var pType = method.GetParameters()[0].ParameterType;

        //if method expects int
        if (pType == typeof(int) || pType == typeof(System.Int32))
            return method.Invoke(this, new object[] {childIndex });

        //if method expects a node derived type
        if (typeof(Unity.Behavior.Node).IsAssignableFrom(pType) || pType.Name.ToLower().Contains("node"))
        {
            var child = Children[childIndex];
            return method.Invoke(this, new object[] { child });
        }

        return method.Invoke(this, new object[] { (object)childIndex });
    }

    protected override Status OnUpdate()
    {
        if (!initialized)
        {
            currentIndex = 0;
            childStarted = false;
            EnsureChildMethods();
            initialized = true;
        }

        if (Children == null || Children.Count == 0)
        {
            return Status.Failure;
        }

        while (currentIndex < Children.Count)
        {
            if (currentIndex >= Children.Count)
            {
                Debug.LogWarning("Current index exceeds the number of children");
                return Status.Failure;
            }

            var currentChild = Children[currentIndex];

            bool resurrectionInProgress = false;
            bool attackInProgress = false;

            Debug.Log($"Evaluating action {currentIndex}: {currentChild.GetType().Name}");

            //check for resurrection or attack is in progress
            resurrectionInProgress = currentChild?.GetType() == typeof(Resurrection) && IsResurrectionActive;
            attackInProgress = currentChild?.GetType() == typeof(LightningController) && isAttacking;

            if (resurrectionInProgress || attackInProgress)
            {
                Debug.Log($"Action {currentChild.GetType().Name} is in progress");
                return Status.Running; //Keep checking until action completes
            }

            //start the child if not started
            if (!childStarted)
            {
                if (childStartMethod != null)
                {
                    Debug.Log($"Starting action {currentIndex}: {currentChild.GetType().Name}");
                    InvokeChildMethod(childStartMethod, currentIndex);
                }

                childStarted = true;
            }

            object rawStatusObj = null;

            if (childUpdateMethod != null)
            {
                rawStatusObj = InvokeChildMethod(childUpdateMethod, currentIndex);
            }

            Status status;
            if (rawStatusObj is Status s)
            {
                status = s;
            }
            else if (rawStatusObj is int i)
            {
                status = (Status)i;
            }
            else
            {
                status = Status.Running;
            }

            Debug.Log($"Action {currentIndex}: {currentChild.GetType().Name} returned status: {status}");

            switch (status)
            {
                case Status.Running:
                    Debug.Log($"Action {currentIndex} is running");
                    continue;

                case Status.Success:
                    //call end on child
                    Debug.Log($"Action {currentIndex} succeeded");
                    if (childEndMethod != null)
                    {
                        InvokeChildMethod(childEndMethod, currentIndex);
                    }
                    childStarted = false;
                    currentIndex++;
                    return Status.Running;

                case Status.Failure:
                default:
                    Debug.Log($"Action {currentIndex} failed");
                    if (childEndMethod != null)
                        { 
                            InvokeChildMethod(childEndMethod, currentIndex);
                        }
                        childStarted = false;
                        currentIndex++;
                        continue;
            }
        }

        return Status.Failure;
    }

    protected override void OnEnd()
    {
        if (childStarted && currentIndex < Children.Count)
        {
            if (childEndMethod != null)
                InvokeChildMethod(childEndMethod, currentIndex);
            else
            {
                var child = Children[currentIndex];
                var mi = child.GetType().GetMethod("End", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                mi?.Invoke(child, null);
            }
        }

        childStarted = false;
    }
}

#nullable disable

