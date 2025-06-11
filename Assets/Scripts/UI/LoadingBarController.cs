using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class LoadingBarController : MonoBehaviour
{
    public Slider loadingSlider;
    private Image fillImage;

    private Color goldColor = new Color(1f, 0.843f, 0f);


    void Start()
    {
        fillImage = loadingSlider.fillRect.GetComponent<Image>();

        // Juste animer la couleur et l'effet de pulsation (pas la valeur)
        AnimateVisuals();
    }

    void AnimateVisuals()
    {
        // Change la couleur du fill progressivement vers gold
        fillImage.DOColor(goldColor, 5f); // durée longue pour suivre le chargement

        // Effet pulsation léger en boucle
        fillImage.rectTransform.DOScale(1.05f, 0.7f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}
