using UnityEngine;

public class PersistentCanvas : MonoBehaviour
{
    private static PersistentCanvas instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[PersistentCanvas] Duplicate found, destroying this one");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[PersistentCanvas] Canvas set to persist across scenes");
    }
}