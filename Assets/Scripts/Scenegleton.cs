
using UnityEngine;

public class Scenegleton<T> : MonoBehaviour where T : Scenegleton<T>
{
    static T _instance;
    public static T Instance
    {
        get
        {
            return _instance;
        }
    }

    protected void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        _instance = this as T;
    }
}