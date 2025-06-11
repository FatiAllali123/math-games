using UnityEngine;
using UnityEngine.UI;


public class SoundMuteButton : MonoBehaviour
{
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    private Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        UpdateButtonSprite();
    }

  
    public void OnClickToggleSound()
    {
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.ToggleMute();
        UpdateButtonSprite();
    }

    private void UpdateButtonSprite()
    {
        if (AudioManager.Instance == null) return;

        bool isMuted = AudioManager.Instance.IsMuted();
        buttonImage.sprite = isMuted ? soundOffSprite : soundOnSprite;
    }
}
