using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MathCoreUIBuilderUtils;

// Genera la escena del menu principal (pantalla de nombre + menu) desde cero.
// Uso: Unity Editor > menu "Math Core > Construir Pantalla de Menu".
public static class MathCoreMenuBuilder
{
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";
    private const string BackgroundSpritePath = "Assets/Art/UI/background_menu.jpg";

    [MenuItem("Math Core/Construir Pantalla de Menu")]
    public static void BuildMainMenuScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        AssetDatabase.Refresh();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject controllerGO = new GameObject("MainMenuController", typeof(MainMenuController));
        controllerGO.transform.SetParent(canvasGO.transform, false);
        MainMenuController controller = controllerGO.GetComponent<MainMenuController>();

        GameObject namePanel = BuildNamePanel(canvasGO.transform,
            out TMP_InputField nameInput, out TMP_Text nameError, out Button saveNameButton);

        GameObject menuPanel = BuildMenuPanel(canvasGO.transform,
            out TMP_Text greetingText, out Button playButton, out Button changeNameButton);

        WireController(controller, namePanel, menuPanel, nameInput, nameError, saveNameButton,
            greetingText, playButton, changeNameButton);

        menuPanel.SetActive(false);

        EnsureFolder("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        MathCoreSceneUtils.RegisterSceneInBuildSettings(ScenePath, 0);

        Selection.activeGameObject = canvasGO;
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Math Core", "Pantalla de menu creada en " + ScenePath, "OK");
        }
    }

    private static GameObject BuildNamePanel(Transform parent, out TMP_InputField nameInput,
        out TMP_Text nameError, out Button saveNameButton)
    {
        Image panelImage = CreateImage("NamePanel", parent, NearBlackBackground);
        Stretch(panelImage.rectTransform);
        Transform panel = panelImage.transform;

        TMP_Text title = CreatePopTitle("Title", panel, "MATH CORE", 90, GoldTitle);
        AnchorTopCenter(title.rectTransform, new Vector2(0f, -160f), new Vector2(900f, 220f));

        TMP_Text prompt = CreateText("Prompt", panel,
            "Hola! Antes de empezar la aventura,\n¿como te llamas?", 40,
            Color.white, TextAlignmentOptions.Center);
        AnchorTopCenter(prompt.rectTransform, new Vector2(0f, -420f), new Vector2(900f, 160f));

        nameInput = CreateInputField("NameInput", panel, "Escribe tu nombre");
        AnchorTopCenter(nameInput.GetComponent<RectTransform>(), new Vector2(0f, -640f), new Vector2(700f, 110f));

        nameError = CreateText("NameError", panel, string.Empty, 30,
            new Color(1f, 0.33f, 0.33f, 1f), TextAlignmentOptions.Center);
        AnchorTopCenter(nameError.rectTransform, new Vector2(0f, -710f), new Vector2(700f, 60f));

        saveNameButton = GreenButtonSprite != null
            ? CreateSpriteButton("SaveNameButton", panel, "GUARDAR Y JUGAR", GreenButtonSprite, Color.white)
            : CreateButton("SaveNameButton", panel, "GUARDAR Y JUGAR", GreenButton, Color.white);
        AnchorTopCenter(saveNameButton.GetComponent<RectTransform>(), new Vector2(0f, -840f), new Vector2(620f, 120f));
        saveNameButton.gameObject.AddComponent<ButtonPressEffect>();

        return panelImage.gameObject;
    }

    private static GameObject BuildMenuPanel(Transform parent, out TMP_Text greetingText,
        out Button playButton, out Button changeNameButton)
    {
        RectTransform panelRect = CreateUIElement("MenuPanel", parent);
        Stretch(panelRect);
        Transform panel = panelRect.transform;

        Image background = CreateImage("Background", panel, Color.white);
        Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundSpritePath);
        if (backgroundSprite != null)
        {
            background.sprite = backgroundSprite;
            CoverFit(background.rectTransform, backgroundSprite, 1080f, 1920f);
        }
        else
        {
            Stretch(background.rectTransform);
            background.color = NearBlackBackground;
            Debug.LogWarning("No se encontro " + BackgroundSpritePath + "; se usa un fondo solido en su lugar.");
        }

        Image overlay = CreateImage("Overlay", panel, new Color(0f, 0f, 0f, 0.78f));
        Stretch(overlay.rectTransform);

        TMP_Text title = CreatePopTitle("Title", panel, "MATH CORE", 90, GoldTitle);
        AnchorTopCenter(title.rectTransform, new Vector2(0f, -180f), new Vector2(900f, 240f));

        changeNameButton = CreateButton("ButtonChangeName", panel, "Cambiar nombre",
            new Color(0f, 0f, 0f, 0f), new Color(0.85f, 0.85f, 0.85f, 1f));
        AnchorBottomCenter(changeNameButton.GetComponent<RectTransform>(), new Vector2(0f, 620f), new Vector2(620f, 60f));
        TMP_Text changeNameLabel = changeNameButton.GetComponentInChildren<TMP_Text>();
        changeNameLabel.fontSize = 26;
        changeNameLabel.fontStyle = FontStyles.Underline;

        // Placa ornamentada con el nombre del jugador y un lapiz decorativo (recursos_menu.png).
        RectTransform namePlaqueRect = CreateUIElement("NamePlaque", panel);
        AnchorBottomCenter(namePlaqueRect, new Vector2(0f, 220f), new Vector2(900f, 366f));
        if (PlaqueFrame != null)
        {
            Image plaqueImage = namePlaqueRect.gameObject.AddComponent<Image>();
            plaqueImage.sprite = PlaqueFrame;
            plaqueImage.raycastTarget = false;
        }

        greetingText = CreateText("Greeting", namePlaqueRect, "Jugador: -", 36, Color.white, TextAlignmentOptions.MidlineLeft);
        AnchorRelative(greetingText.rectTransform, new Vector2(0.10f, 0.63f), new Vector2(0.80f, 0.87f));
        greetingText.rectTransform.offsetMin = Vector2.zero;
        greetingText.rectTransform.offsetMax = Vector2.zero;
        greetingText.raycastTarget = false;

        if (PencilIcon != null)
        {
            RectTransform pencilRect = CreateUIElement("PencilIcon", namePlaqueRect);
            AnchorRelative(pencilRect, new Vector2(0.82f, 0.60f), new Vector2(0.95f, 0.90f));
            Image pencilImage = pencilRect.gameObject.AddComponent<Image>();
            pencilImage.sprite = PencilIcon;
            pencilImage.preserveAspect = true;
            pencilImage.raycastTarget = false;
        }

        // Botones de accion, en fila, cerca del borde inferior.
        playButton = GreenButtonSprite != null
            ? CreateSpriteButton("ButtonPlay", panel, "JUGAR", GreenButtonSprite, Color.white)
            : CreateButton("ButtonPlay", panel, "JUGAR", GreenButton, Color.white);
        AnchorBottomCenter(playButton.GetComponent<RectTransform>(), new Vector2(-230f, 40f), new Vector2(420f, 160f));

        Button settingsButton = OrangeButtonSprite != null
            ? CreateSpriteButton("ButtonSettings", panel, "AJUSTES", OrangeButtonSprite, Color.white)
            : CreateButton("ButtonSettings", panel, "AJUSTES", DarkButton, Color.white);
        AnchorBottomCenter(settingsButton.GetComponent<RectTransform>(), new Vector2(230f, 40f), new Vector2(420f, 160f));
        settingsButton.interactable = false;

        foreach (Button button in new[] { playButton, settingsButton, changeNameButton })
        {
            button.gameObject.AddComponent<ButtonPressEffect>();
        }

        return panelRect.gameObject;
    }

    private static void WireController(MainMenuController controller, GameObject namePanel, GameObject menuPanel,
        TMP_InputField nameInput, TMP_Text nameError, Button saveNameButton,
        TMP_Text greetingText, Button playButton, Button changeNameButton)
    {
        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("namePanel").objectReferenceValue = namePanel;
        serialized.FindProperty("menuPanel").objectReferenceValue = menuPanel;
        serialized.FindProperty("nameInput").objectReferenceValue = nameInput;
        serialized.FindProperty("nameError").objectReferenceValue = nameError;
        serialized.FindProperty("saveNameButton").objectReferenceValue = saveNameButton;
        serialized.FindProperty("greetingText").objectReferenceValue = greetingText;
        serialized.FindProperty("playButton").objectReferenceValue = playButton;
        serialized.FindProperty("changeNameButton").objectReferenceValue = changeNameButton;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureFolder(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
    }
}
