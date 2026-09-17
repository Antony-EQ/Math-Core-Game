using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Helpers de UI compartidos por los constructores de escenas de Math Core (Assets/Editor/*Builder.cs).
// Estilo visual: fondo casi negro, paneles/botones con borde oscuro + sombra dura desplazada
// (efecto "pixel art 3D"), y titulos con contorno + sombra proyectada via el shader SDF de TMP.
public static class MathCoreUIBuilderUtils
{
    // Nombre que Unity le da por defecto al usar Assets > Create > TextMeshPro > Font Asset
    // sobre Assets/Fonts/PressStart2P-Regular.ttf.
    private const string PixelFontAssetPath = "Assets/Fonts/PressStart2P-Regular SDF.asset";

    private const float BevelBorderThickness = 10f;
    private const float BevelShadowOffset = 10f;

    // Sprite sheet recortado a mano en el Sprite Editor (ver Assets/Art/UI/recursos/recursos_menu.png):
    // _0 = lapiz, _1 = marco de pergamino, _2 = boton verde, _3 = boton naranja.
    private const string RecursosMenuPath = "Assets/Art/UI/recursos/recursos_menu.png";

    private static TMP_FontAsset _pixelFont;
    private static bool _pixelFontLookupDone;
    private static Dictionary<string, Sprite> _recursosMenuSprites;

    public static Sprite PencilIcon => GetRecursosMenuSprite("recursos_menu_0");
    public static Sprite PlaqueFrame => GetRecursosMenuSprite("recursos_menu_1");
    public static Sprite GreenButtonSprite => GetRecursosMenuSprite("recursos_menu_2");
    public static Sprite OrangeButtonSprite => GetRecursosMenuSprite("recursos_menu_3");

    private static Sprite GetRecursosMenuSprite(string spriteName)
    {
        if (_recursosMenuSprites == null)
        {
            _recursosMenuSprites = new Dictionary<string, Sprite>();
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(RecursosMenuPath))
            {
                if (asset is Sprite sprite)
                {
                    _recursosMenuSprites[sprite.name] = sprite;
                }
            }

            if (_recursosMenuSprites.Count == 0)
            {
                Debug.LogWarning("No se encontraron sprites recortados en " + RecursosMenuPath +
                    ". Selecciona el archivo, pon Sprite Mode en Multiple y usa el Sprite Editor > Slice > Automatic.");
            }
        }

        _recursosMenuSprites.TryGetValue(spriteName, out Sprite result);
        return result;
    }

    public static readonly Color NearBlackBackground = new Color(0.035f, 0.035f, 0.05f, 1f);
    public static readonly Color GoldTitle = new Color(1f, 0.84f, 0f, 1f);
    public static readonly Color GreenButton = new Color(0.20f, 0.62f, 0.28f, 1f);
    public static readonly Color DarkButton = new Color(0.16f, 0.17f, 0.20f, 1f);
    public static readonly Color NavyPanel = new Color(0.11f, 0.14f, 0.22f, 1f);

    private static readonly Color TitleOutline = new Color(0.30f, 0.16f, 0.02f, 1f);
    private static readonly Color TitleShadow = new Color(0f, 0f, 0f, 0.9f);

    private static TMP_FontAsset PixelFont
    {
        get
        {
            if (!_pixelFontLookupDone)
            {
                _pixelFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PixelFontAssetPath);
                if (_pixelFont == null)
                {
                    Debug.LogWarning("No se encontro " + PixelFontAssetPath +
                        ". Genera el Font Asset (clic derecho en PressStart2P-Regular.ttf > Create > TextMeshPro > Font Asset) y vuelve a construir la pantalla.");
                }
                _pixelFontLookupDone = true;
            }
            return _pixelFont;
        }
    }

    public static void CreateCamera()
    {
        GameObject cameraGO = new GameObject("Main Camera", typeof(Camera));
        cameraGO.tag = "MainCamera";
        Camera cam = cameraGO.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = NearBlackBackground;
        cam.orthographic = true;
    }

    public static RectTransform CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    public static Image CreateImage(string name, Transform parent, Color color)
    {
        RectTransform rect = CreateUIElement(name, parent);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    public static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, Color color,
        TextAlignmentOptions alignment, FontStyles style = FontStyles.Normal)
    {
        RectTransform rect = CreateUIElement(name, parent);
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.fontStyle = style;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        if (PixelFont != null)
        {
            tmp.font = PixelFont;
        }
        return tmp;
    }

    // Titulo con contorno oscuro + sombra proyectada dura, para el efecto "texto sobresaliendo".
    public static TMP_Text CreatePopTitle(string name, Transform parent, string text, float fontSize, Color faceColor)
    {
        TMP_Text tmp = CreateText(name, parent, text, fontSize, faceColor, TextAlignmentOptions.Center, FontStyles.Bold);

        Material material = tmp.fontMaterial;
        material.EnableKeyword(ShaderUtilities.Keyword_Outline);
        material.SetColor(ShaderUtilities.ID_OutlineColor, TitleOutline);
        material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.18f);

        material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        material.SetColor(ShaderUtilities.ID_UnderlayColor, TitleShadow);
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 1f);
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -1f);
        material.SetFloat(ShaderUtilities.ID_UnderlayDilate, 1f);
        tmp.fontMaterial = material;

        return tmp;
    }

    // Panel en capas: sombra dura desplazada + borde oscuro + relleno, imitando un boton "pixel art" en 3D.
    private static RectTransform CreateBeveledPanel(string name, Transform parent, Color fillColor, out Transform fillTransform)
    {
        RectTransform outer = CreateUIElement(name, parent);

        Color borderColor = Darken(fillColor, 0.5f);
        Color shadowColor = Darken(fillColor, 0.25f);

        Image shadow = CreateImage("Shadow", outer, shadowColor);
        shadow.raycastTarget = false;
        shadow.rectTransform.anchorMin = Vector2.zero;
        shadow.rectTransform.anchorMax = Vector2.one;
        shadow.rectTransform.offsetMin = new Vector2(BevelShadowOffset, -BevelShadowOffset);
        shadow.rectTransform.offsetMax = new Vector2(BevelShadowOffset, -BevelShadowOffset);

        Image border = CreateImage("Border", outer, borderColor);
        border.raycastTarget = false;
        Stretch(border.rectTransform);

        Image fill = CreateImage("Fill", border.transform, fillColor);
        fill.raycastTarget = false;
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = Vector2.one;
        fill.rectTransform.offsetMin = new Vector2(BevelBorderThickness, BevelBorderThickness);
        fill.rectTransform.offsetMax = new Vector2(-BevelBorderThickness, -BevelBorderThickness);

        fillTransform = fill.transform;
        return outer;
    }

    private static Color Darken(Color color, float factor)
    {
        return new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
    }

    public static Button CreateButton(string name, Transform parent, string label, Color fillColor, Color textColor)
    {
        RectTransform outer = CreateBeveledPanel(name, parent, fillColor, out Transform fill);

        Image hitArea = outer.gameObject.AddComponent<Image>();
        hitArea.color = Color.clear;
        Button button = outer.gameObject.AddComponent<Button>();
        button.targetGraphic = hitArea;

        TMP_Text text = CreateText(name + "Label", fill, label, 40, textColor, TextAlignmentOptions.Center, FontStyles.Bold);
        text.raycastTarget = false;
        Stretch(text.rectTransform);

        return button;
    }

    // Boton con arte pixel real (9-slice) en vez del rectangulo generado por color.
    // El sprite debe tener su borde configurado en el Sprite Editor para no verse estirado.
    public static Button CreateSpriteButton(string name, Transform parent, string label, Sprite sprite, Color textColor)
    {
        RectTransform outer = CreateUIElement(name, parent);

        Image image = outer.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;

        Button button = outer.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        TMP_Text text = CreateText(name + "Label", outer, label, 40, textColor, TextAlignmentOptions.Center, FontStyles.Bold);
        text.raycastTarget = false;
        Stretch(text.rectTransform);
        text.rectTransform.offsetMin = new Vector2(20f, 10f);
        text.rectTransform.offsetMax = new Vector2(-20f, -10f);

        return button;
    }

    public static TMP_InputField CreateInputField(string name, Transform parent, string placeholderText)
    {
        RectTransform outer = CreateBeveledPanel(name, parent, NavyPanel, out Transform fill);

        Image hitArea = outer.gameObject.AddComponent<Image>();
        hitArea.color = Color.clear;
        TMP_InputField inputField = outer.gameObject.AddComponent<TMP_InputField>();

        RectTransform textArea = CreateUIElement("Text Area", fill);
        Stretch(textArea);
        // Sin RectMask2D a proposito: con TMP, un texto vacio dentro de una mascara de recorte
        // dispara un NullReferenceException conocido en TextMeshProUGUI.Cull en cada frame.

        TMP_Text placeholder = CreateText("Placeholder", textArea, placeholderText, 32,
            new Color(1f, 1f, 1f, 0.35f), TextAlignmentOptions.MidlineLeft, FontStyles.Italic);
        Stretch(placeholder.rectTransform);
        placeholder.rectTransform.offsetMin = new Vector2(20f, 0f);
        placeholder.rectTransform.offsetMax = new Vector2(-20f, 0f);
        placeholder.raycastTarget = false;

        TMP_Text textComponent = CreateText("Text", textArea, string.Empty, 32,
            Color.white, TextAlignmentOptions.MidlineLeft);
        Stretch(textComponent.rectTransform);
        textComponent.rectTransform.offsetMin = new Vector2(20f, 0f);
        textComponent.rectTransform.offsetMax = new Vector2(-20f, 0f);
        textComponent.raycastTarget = false;

        inputField.textViewport = textArea;
        inputField.textComponent = textComponent;
        inputField.placeholder = placeholder;
        inputField.targetGraphic = hitArea;
        inputField.text = string.Empty;
        inputField.characterLimit = 16;

        return inputField;
    }

    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public static void AnchorTopCenter(RectTransform rect, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    // anchoredPosition.y se mide hacia arriba desde el borde inferior de la pantalla.
    public static void AnchorBottomCenter(RectTransform rect, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    // Anclaje relativo (0..1) al padre, para ubicar elementos dentro de un sprite (p. ej. dentro de la placa ornamentada).
    public static void AnchorRelative(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
