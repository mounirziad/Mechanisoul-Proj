using UnityEngine;
using System.Linq;

public static class GameObjectExtensions
{
    public static T OrNull<T>(this T obj) where T : Object => obj ? obj : null;

    //Returns the object if it still exists in the scene, it will be null if not
    //Used from Git-Amend in order to make a better GOAP system
}
