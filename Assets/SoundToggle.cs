using UnityEngine;
using UnityEngine.UI;

public class SoundToggle : MonoBehaviour
{
    public AudioSource audioSource;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    private Image buttonImage;
    private bool isMuted = false;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        GetComponent<Button>().onClick.AddListener(ToggleSound);

        // Set initial sprite
        buttonImage.sprite = isMuted ? soundOffSprite : soundOnSprite;
    }

    void ToggleSound()
    {
        isMuted = !isMuted;
        audioSource.mute = isMuted;
        buttonImage.sprite = isMuted ? soundOffSprite : soundOnSprite;
    }
}