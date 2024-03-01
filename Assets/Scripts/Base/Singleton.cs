using UnityEngine;

/// <summary>
/// Implements the singleton pattern.
/// </summary>
/// <typeparam name="T">The type to apply the singleton pattern to.</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T> {
    /// <summary>
    /// The singleton instance.
    /// </summary>
    public static T Instance { get; private set; }

    protected virtual void Awake() {
        if (!Instance) {
            Instance = this as T;
            DontDestroyOnLoad(gameObject);
        } else Destroy(gameObject);
    }
}
