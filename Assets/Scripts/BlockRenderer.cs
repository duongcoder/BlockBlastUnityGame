using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BlockRenderer : MonoBehaviour
{
    public Sprite blockSprite;
    public float sizeMultiplier = 1f;

    private Image image;
    private RectTransform rectTransform;

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Render(BlockDefinition definition, float cellSize)
    {
        if (image == null || definition == null) return;

        Sprite spriteToUse = definition.sprite != null ? definition.sprite : blockSprite;

        if (spriteToUse != null)
        {
            image.sprite = spriteToUse;
            image.enabled = true;
            image.preserveAspect = true;
        }
        else
        {
            image.enabled = false;
            return;
        }

        int width = definition.GetWidth();
        int height = definition.GetHeight();

        float totalW = width * cellSize * sizeMultiplier;
        float totalH = height * cellSize * sizeMultiplier;
        rectTransform.sizeDelta = new Vector2(totalW, totalH);
    }
}