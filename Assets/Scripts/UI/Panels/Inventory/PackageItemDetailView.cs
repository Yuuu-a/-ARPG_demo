using TMPro;
using UnityEngine.UI;

public sealed class PackageItemDetailView
{
    private readonly Image iconImage;
    private readonly Image previewImage;
    private readonly TMP_Text itemNameText;
    private readonly TMP_Text itemLevelText;
    private readonly TMP_Text descriptionText;
    private readonly TMP_Text effectDescriptionText;
    private readonly TMP_Text baseAttackText;
    private readonly TMP_Text basicAttributeText;

    public PackageItemDetailView(
        Image iconImage,
        Image previewImage,
        TMP_Text itemNameText,
        TMP_Text itemLevelText,
        TMP_Text descriptionText,
        TMP_Text effectDescriptionText,
        TMP_Text baseAttackText = null,
        TMP_Text basicAttributeText = null)
    {
        this.iconImage = iconImage;
        this.previewImage = previewImage;
        this.itemNameText = itemNameText;
        this.itemLevelText = itemLevelText;
        this.descriptionText = descriptionText;
        this.effectDescriptionText = effectDescriptionText;
        this.baseAttackText = baseAttackText;
        this.basicAttributeText = basicAttributeText;
    }

    public void Show(PackageItemViewData item)
    {
        if (item == null)
        {
            Clear();
            return;
        }

        SetImage(iconImage, item.ItemIcon);
        SetImage(previewImage, item.ItemIcon);
        SetText(itemNameText, item.ItemName);
        SetText(itemLevelText, $"等级 {item.Level}");
        SetText(descriptionText, item.Description);
        SetText(effectDescriptionText, item.EffectDescription);
        SetText(baseAttackText, item.BaseAttackDescription);
        SetText(basicAttributeText, item.BasicAttributeDescription);

        SetFieldVisible(effectDescriptionText,
            !string.IsNullOrEmpty(item.EffectDescription));
        SetFieldVisible(baseAttackText,
            !string.IsNullOrEmpty(item.BaseAttackDescription));
        SetFieldVisible(basicAttributeText,
            !string.IsNullOrEmpty(item.BasicAttributeDescription));
    }

    public void Clear()
    {
        SetImage(iconImage, null);
        SetImage(previewImage, null);
        SetText(itemNameText, string.Empty);
        SetText(itemLevelText, string.Empty);
        SetText(descriptionText, string.Empty);
        SetText(effectDescriptionText, string.Empty);
        SetText(baseAttackText, string.Empty);
        SetText(basicAttributeText, string.Empty);
        SetFieldVisible(effectDescriptionText, false);
        SetFieldVisible(baseAttackText, false);
        SetFieldVisible(basicAttributeText, false);
    }

    public void ShowEmpty(string message)
    {
        Clear();
        SetText(descriptionText, message);
    }

    private static void SetImage(Image image, UnityEngine.Sprite sprite)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value ?? string.Empty;
        }
    }

    private static void SetFieldVisible(TMP_Text text, bool visible)
    {
        if (text != null)
        {
            text.gameObject.SetActive(visible);
        }
    }
}
