using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MathCoreUIBuilderUtils;

// Genera la escena de seleccion de mapas.
// Uso: Unity Editor > menu "Math Core > Construir Pantalla de Mapas".
public static class MathCoreMapSelectBuilder
{
    private const string ScenePath = "Assets/Scenes/MapSelect.unity";

    [MenuItem("Math Core/Construir Pantalla de Mapas")]
    public static void BuildMapSelectScene()
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

        GameObject controllerGO = new GameObject("MapSelectController", typeof(MapSelectController));
        controllerGO.transform.SetParent(canvasGO.transform, false);
        MapSelectController controller = controllerGO.GetComponent<MapSelectController>();

        Image background = CreateImage("Background", canvasGO.transform, NearBlackBackground);
        Stretch(background.rectTransform);

        TMP_Text title = CreatePopTitle("Title", canvasGO.transform, "MAPAS", 64, GoldTitle);
        AnchorTopCenter(title.rectTransform, new Vector2(0f, -140f), new Vector2(960f, 120f));

        TMP_Text subtitle = CreateText("Subtitle", canvasGO.transform, "Elige tu destino", 36,
            new Color(0.85f, 0.85f, 0.85f, 1f), TextAlignmentOptions.Center);
        AnchorTopCenter(subtitle.rectTransform, new Vector2(0f, -240f), new Vector2(960f, 70f));

        Button greeceButton = GreenButtonSprite != null
            ? CreateSpriteButton("GreeceButton", canvasGO.transform, "GRECIA", GreenButtonSprite, Color.white)
            : CreateButton("GreeceButton", canvasGO.transform, "GRECIA", GreenButton, Color.white);
        AnchorTopCenter(greeceButton.GetComponent<RectTransform>(), new Vector2(-250f, -640f), new Vector2(460f, 300f));
        greeceButton.gameObject.AddComponent<ButtonPressEffect>();

        Button egyptButton = CreateButton("EgyptButton", canvasGO.transform, "EGIPTO\n(bloqueado)", DarkButton, new Color(0.6f, 0.6f, 0.6f, 1f));
        AnchorTopCenter(egyptButton.GetComponent<RectTransform>(), new Vector2(250f, -640f), new Vector2(460f, 300f));
        egyptButton.interactable = false;

        Button romeButton = CreateButton("RomeButton", canvasGO.transform, "ROMA\n(bloqueado)", DarkButton, new Color(0.6f, 0.6f, 0.6f, 1f));
        AnchorTopCenter(romeButton.GetComponent<RectTransform>(), new Vector2(-250f, -980f), new Vector2(460f, 300f));
        romeButton.interactable = false;

        Button incaButton = CreateButton("IncaButton", canvasGO.transform, "I. INCA\n(bloqueado)", DarkButton, new Color(0.6f, 0.6f, 0.6f, 1f));
        AnchorTopCenter(incaButton.GetComponent<RectTransform>(), new Vector2(250f, -980f), new Vector2(460f, 300f));
        incaButton.interactable = false;

        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("titleText").objectReferenceValue = title;
        serialized.FindProperty("greeceButton").objectReferenceValue = greeceButton;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, ScenePath);
        MathCoreSceneUtils.RegisterSceneInBuildSettings(ScenePath, 1);

        Selection.activeGameObject = canvasGO;
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Math Core", "Pantalla de mapas creada en " + ScenePath, "OK");
        }
    }
}
