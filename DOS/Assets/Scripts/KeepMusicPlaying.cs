using UnityEngine;

public class KeepMusicPlaying : MonoBehaviour
{
    private static KeepMusicPlaying instance;

    void Awake()
    {
        // If an instance of this music player already exists...
        if (instance != null)
        {
            // ...destroy this new one so we don't have duplicate music playing.
            Destroy(gameObject); 
            return;
        }

        // Otherwise, this is the first one. Keep it alive.
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}