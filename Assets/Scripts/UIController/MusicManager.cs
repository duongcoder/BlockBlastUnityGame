using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void PlayMusic()
    {
        if (instance != null && instance.audioSource != null && !instance.audioSource.isPlaying)
        {
            instance.audioSource.Play();
        }
    }

    public static void StopMusic()
    {
        if (instance != null && instance.audioSource != null && instance.audioSource.isPlaying)
        {
            instance.audioSource.Stop();
        }
    }
}
