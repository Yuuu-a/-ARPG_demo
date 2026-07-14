using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class AlphaHitTestImage : MonoBehaviour
{
    [Header("透明度来源")]
    [Tooltip("拖入实际显示透明 PNG 的 Image；为空时使用当前物体上的 Image。")]
    [SerializeField] private Image sourceImage;

    [Tooltip("像素 Alpha 大于等于该值时允许点击。")]
    [Range(0.01f, 1f)]
    [SerializeField] private float alphaThreshold = 0.1f;

    private Image targetImage;

    public Image SourceImage => sourceImage != null
        ? sourceImage
        : GetComponent<Image>();

    private void Awake()
    {
        ApplyAlphaHitTest();
    }

    private void OnEnable()
    {
        ApplyAlphaHitTest();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyAlphaHitTest();
    }
#endif

    private void ApplyAlphaHitTest()
    {
        Image resolvedImage = SourceImage;

        if (targetImage != null && targetImage != resolvedImage)
        {
            targetImage.alphaHitTestMinimumThreshold = 0f;
        }

        targetImage = resolvedImage;

        if (targetImage == null)
        {
            if (Application.isPlaying)
            {
                Debug.LogError(
                    $"{nameof(AlphaHitTestImage)} 没有配置 Source Image，" +
                    "当前物体上也没有 Image。",
                    this);
            }

            return;
        }

        targetImage.raycastTarget = true;
        targetImage.alphaHitTestMinimumThreshold = alphaThreshold;

        Sprite sprite = targetImage.sprite;
        if (sprite == null)
        {
            Debug.LogWarning(
                $"{nameof(AlphaHitTestImage)} 需要 Image 配置 Sprite。",
                this);
            return;
        }

        if (!sprite.texture.isReadable)
        {
            Debug.LogError(
                $"{sprite.texture.name} 必须在纹理导入设置中开启 " +
                "Read/Write Enabled，透明像素点击检测才能生效。",
                this);
        }
    }

    public void SetSourceImage(Image image)
    {
        sourceImage = image;
        ApplyAlphaHitTest();
    }
}
