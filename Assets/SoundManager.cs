using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton : un seul SoundManager pour toutes les sc�nes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ne pas d�truire quand on change de sc�ne
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
