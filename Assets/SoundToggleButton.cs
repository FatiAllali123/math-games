using UnityEngine;
using UnityEngine.UI;


public class SoundToggleButton : MonoBehaviour
{
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    private Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();

        UpdateButtonSprite();
    }

    public void ToggleSound()
    {
        if (SoundManager.Instance == null) return;

        bool isMuted = !SoundManager.Instance.IsMuted();

        SoundManager.Instance.Mute(isMuted);

        UpdateButtonSprite();
    }

    private void UpdateButtonSprite()
    {
        if (SoundManager.Instance == null) return;

        buttonImage.sprite = SoundManager.Instance.IsMuted() ? soundOffSprite : soundOnSprite;
    }
}
