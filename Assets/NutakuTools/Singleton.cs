using UnityEngine;


/// <summary>
/// A singleton class for MonoBehaviour.
/// </summary>
/// <typeparam name="T">The type of the singleton. If the type is "FooBar", this value should be "FooBar".</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
                Debug.LogError(string.Format("Trying to access the Instance of {0} but it is null.\n Is the GameObject that contains this script already loaded?", typeof(T).Name));
            return instance;
        }
        protected set
        {
            instance = value;
        }
    }

    /// <summary>
    /// Replaces the Awake function, as Singleton uses the Awake event.
    /// </summary>
    protected virtual void OnAwake() { }
    public void Awake()
    {
        if (typeof(T) != this.GetType() && !typeof(T).IsAssignableFrom(this.GetType())) throw new System.InvalidOperationException("Type argument does not match singleton type!");

        if (instance == null)
        {
            Instance = this as T;
            OnAwake();
        }

        else
        {
            Debug.LogWarning("Only one instance of " + this.GetType() + " is allowed per scene!");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Set the Singleton Instance to null if we destroy the Instance gameobject
    /// </summary>
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    static public bool IsLoaded()
    {
        return instance != null;
    }
}