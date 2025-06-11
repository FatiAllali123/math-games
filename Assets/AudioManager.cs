using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre les scènes
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject); // Évite les doublons
        }
    }

    public void ToggleMute()
    {
        if (audioSource != null)
            audioSource.mute = !audioSource.mute;
    }

    public bool IsMuted()
    {
        return audioSource != null && audioSource.mute;
    }
}

