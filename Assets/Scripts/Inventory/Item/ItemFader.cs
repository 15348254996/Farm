using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemFader : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    #region 逐渐恢复颜色
    public void FadeIn()
    {
        Color targetColor = new Color(1, 1, 1, 1);
        spriteRenderer.DOColor(targetColor,Settings.fadeDuration);
    }
    #endregion

    #region 逐渐半透明
    public void FadeOut()
    {
        Color targetColor = new Color(1, 1, 1, Settings.targetAlpga);
        spriteRenderer.DOColor(targetColor, Settings.fadeDuration);
    }
    #endregion
}
