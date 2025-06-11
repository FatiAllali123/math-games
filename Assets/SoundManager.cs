using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton : un seul SoundManager pour toutes les scènes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ne pas détruire quand on change de scène
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Mute(bool mute)
    {
        if (audioSource != null)
            audioSource.mute = mute;
    }

    public bool IsMuted()
    {
        return audioSource != null && audioSource.mute;
    }
}
