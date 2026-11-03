using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Editor-only helpers that build uGUI/TMP elements from code.
// Place under Assets/Editor/ (compiled into Assembly-CSharp-Editor, which sees the game's internal types).
internal static class SpellTyposUi
{
    private static Sprite _panelSprite;

    internal static Sprite PanelSprite
    {
        get
        {
            if (_panelSprite == null) _panelSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            return _panelSprite;
        }
    }

    internal static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    internal static void Place(RectTransform r, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        r.anchorMin = anchor;
        r.anchorMax = anchor;
        r.pivot = pivot;
        r.anchoredPosition = pos;
        r.sizeDelta = size;
    }

    internal static void PlaceCenter(RectTransform r, Vector2 pos, Vector2 size)
    {
        Place(r, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
    }

    internal static void PlaceTop(RectTransform r, float y, float width, float height)
    {
        Place(r, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -y), new Vector2(width, height));
    }

    internal static void Stretch(RectTransform r, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.pivot = new Vector2(0.5f, 0.5f);
        r.offsetMin = new Vector2(left, bottom);
        r.offsetMax = new Vector2(-right, -top);
    }

    internal static Image NewImage(string name, Transform parent, Color color, bool sliced = true)
    {
        RectTransform rect = NewRect(name, parent);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        if (sliced && PanelSprite != null)
        {
            image.sprite = PanelSprite;
            image.type = Image.Type.Sliced;
        }
        return image;
    }

    internal static TextMeshProUGUI NewText(string name, Transform parent, string text, float size,
        TextAlignmentOptions align, Color? color = null)
    {
        RectTransform rect = NewRect(name, parent);
        var t = rect.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.alignment = align;
        t.color = color ?? Color.white;
        t.raycastTarget = false;
        return t;
    }

    internal static TMP_InputField NewInputField(string name, Transform parent, string placeholder, float fontSize)
    {
        Image bg = NewImage(name, parent, new Color(0.13f, 0.12f, 0.18f, 1f));
        var field = bg.gameObject.AddComponent<TMP_InputField>();

        RectTransform viewport = NewRect("Text Area", bg.transform);
        Stretch(viewport, 16f, 6f, 16f, 6f);
        viewport.gameObject.AddComponent<RectMask2D>();

        TextMeshProUGUI ph = NewText("Placeholder", viewport, placeholder, fontSize,
            TextAlignmentOptions.MidlineLeft, new Color(1f, 1f, 1f, 0.35f));
        ph.fontStyle = FontStyles.Italic;
        Stretch(ph.rectTransform);

        TextMeshProUGUI text = NewText("Text", viewport, "", fontSize, TextAlignmentOptions.MidlineLeft);
        Stretch(text.rectTransform);

        field.textViewport = viewport;
        field.textComponent = text;
        field.placeholder = ph;
        field.targetGraphic = bg;
        return field;
    }

    internal static Button NewButton(string name, Transform parent, string label, float fontSize, Color color)
    {
        Image img = NewImage(name, parent, color);
        var button = img.gameObject.AddComponent<Button>();
        button.targetGraphic = img;
        TextMeshProUGUI t = NewText("Label", img.transform, label, fontSize, TextAlignmentOptions.Center);
        Stretch(t.rectTransform);
        return button;
    }

    internal static Slider NewBar(string name, Transform parent, Color back, Color fill)
    {
        Image bg = NewImage(name, parent, back);
        bg.raycastTarget = false;
        var slider = bg.gameObject.AddComponent<Slider>();

        RectTransform area = NewRect("Fill Area", bg.transform);
        Stretch(area, 3f, 3f, 3f, 3f);

        Image fillImage = NewImage("Fill", area, fill);
        fillImage.raycastTarget = false;
        Stretch(fillImage.rectTransform);

        slider.fillRect = fillImage.rectTransform;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.value = 1f;
        slider.interactable = false;
        return slider;
    }

    internal static RectTransform NewScroll(string name, Transform parent, out RectTransform content)
    {
        Image bg = NewImage(name, parent, new Color(0f, 0f, 0f, 0.35f));
        var scroll = bg.gameObject.AddComponent<ScrollRect>();

        RectTransform viewport = NewRect("Viewport", bg.transform);
        Stretch(viewport, 8f, 8f, 8f, 8f);
        viewport.gameObject.AddComponent<RectMask2D>();

        content = NewRect("Content", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 4f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 25f;
        return bg.rectTransform;
    }
}
