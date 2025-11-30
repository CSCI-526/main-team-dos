using UnityEngine;

public class KeepMusicPlaying : MonoBehaviour
{
    private static KeepMusicPlaying instance;

    void Awake()
    {
        
        if (instance != null)
        {
            
            Destroy(gameObject); 
            return;
        }

        
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}