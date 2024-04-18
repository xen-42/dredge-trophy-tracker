using UnityEngine;

namespace TrophyTracker;

public class Loader
{
    /// <summary>
    /// This method is run by Winch to initialize your mod
    /// </summary>
    public static void Initialize()
    {
        var gameObject = new GameObject(nameof(TrophyTracker));
        gameObject.AddComponent<TrophyTracker>();
        GameObject.DontDestroyOnLoad(gameObject);
    }
}