using UnityEngine;
using UnityEngine.UI;

public class MusicController : MonoBehaviour
{
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private Button soundButton;
    [SerializeField] private Sprite soundIcon;
    [SerializeField] private Sprite muteIcon;
    private bool isMuted = false;

    void Start()
    {
        if (backgroundMusicSource == null)
        {
            Debug.LogWarning("AudioSource de la musique de fond non assigné ! Glisse BackgroundMusic dans l'Inspecteur.");
        }
        if (soundButton == null)
        {
            Debug.LogWarning("Bouton Son non assigné ! Glisse SoundButton dans l'Inspecteur.");
        }
        if (soundIcon == null || muteIcon == null)
        {
            Debug.LogWarning("Icônes Son ou Mute non assignées ! Glisse les sprites dans l'Inspecteur.");
        }

        // Forcer l'état initial
        isMuted = false;
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.mute = false;
            // Ne pas appeler Play() si la musique est déjà en cours
            if (!backgroundMusicSource.isPlaying)
            {
                backgroundMusicSource.Play();
            }
            Debug.Log("Musique initialisée : " + !backgroundMusicSource.mute);
        }
        UpdateButtonImage();
    }

    public void ToggleMute()
    {
        Debug.Log("ToggleMute appelé !");
        isMuted = !isMuted;
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.mute = isMuted;
            Debug.Log("Musique muette : " + isMuted);
        }
        UpdateButtonImage();
    }

    private void UpdateButtonImage()
    {
        if (soundButton != null && soundIcon != null && muteIcon != null)
        {
            soundButton.GetComponent<Image>().sprite = isMuted ? muteIcon : soundIcon;
        }
    }
}