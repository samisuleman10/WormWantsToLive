using UnityEngine;

public class BasicEventManager<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    private static bool applicationIsQuitting = false;

    protected virtual void OnDestroy()
    {
     //   Debug.Log("Gets destroyed");
        applicationIsQuitting = true;
    }

    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                return null;
            }

            if (_instance == null)
            {
                var objs = FindObjectsOfType(typeof(T)) as T[];
                if (objs.Length > 0)
                    _instance = objs[0];
                if (objs.Length > 1)
                {
                    Debug.LogError("There is more than one " + typeof(T).Name + " in the scene.");
                }
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = string.Format("_{0}", typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    
}
