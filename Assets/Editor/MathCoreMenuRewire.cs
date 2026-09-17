using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Cablea los botones que el usuario agrego a mano (ButtonPlay, ButtonSettings, ButtonChangeName)
// a la logica de MainMenuController, borra los botones generados por el script original,
// y les agrega el efecto de presion. Opera sobre la escena YA ABIERTA, no crea una nueva.
public static class MathCoreMenuRewire
{
    [MenuItem("Math Core/Cablear Botones Nuevos del Menu")]
    public static void RewireMenuButtons()
    {
        GameObject root = FindRoot("MainMenu");
        if (root == null)
        {
            EditorUtility.DisplayDialog("Math Core",
                "No se encontro un objeto raiz 'MainMenu' en la escena abierta. Abre la escena MainMenu primero.", "OK");
            return;
        }

        GameObject controllerGO = FindChild(root.transform, "MainMenuController");
        MainMenuController controller = controllerGO != null ? controllerGO.GetComponent<MainMenuController>() : null;
        if (controller == null)
        {
            EditorUtility.DisplayDialog("Math Core", "No se encontro MainMenuController en la escena.", "OK");
            return;
        }

        GameObject newPlay = FindChild(root.transform, "ButtonPlay");
        GameObject newSettings = FindChild(root.transform, "ButtonSettings");
        GameObject newChangeName = FindChild(root.transform, "ButtonChangeName");

        if (newPlay == null || newChangeName == null)
        {
            EditorUtility.DisplayDialog("Math Core",
                "No se encontraron ButtonPlay y/o ButtonChangeName. Revisa los nombres exactos en la Hierarchy.", "OK");
            return;
        }

        Undo.RecordObject(controller, "Rewire Menu Buttons");
        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("playButton").objectReferenceValue = newPlay.GetComponent<Button>();
        serialized.FindProperty("changeNameButton").objectReferenceValue = newChangeName.GetComponent<Button>();
        serialized.ApplyModifiedPropertiesWithoutUndo();

        DestroyIfFound(root.transform, "PlayButton");
        DestroyIfFound(root.transform, "SettingsButton");
        DestroyIfFound(root.transform, "ChangeNameButton");

        AddPressEffect(newPlay);
        AddPressEffect(newChangeName);
        if (newSettings != null)
        {
            AddPressEffect(newSettings);
        }

        GameObject saveNameButtonGO = FindChild(root.transform, "SaveNameButton");
        if (saveNameButtonGO != null)
        {
            AddPressEffect(saveNameButtonGO);
        }

        EditorSceneManager.MarkSceneDirty(root.scene);
        EditorUtility.DisplayDialog("Math Core",
            "Listo: ButtonPlay y ButtonChangeName quedaron cableados, los botones viejos se borraron, " +
            "y los botones nuevos (mas GUARDAR Y JUGAR) ya tienen el efecto de presion.", "OK");
    }

    private static void DestroyIfFound(Transform root, string name)
    {
        GameObject found = FindChild(root, name);
        if (found != null)
        {
            Undo.DestroyObjectImmediate(found);
        }
    }

    private static void AddPressEffect(GameObject go)
    {
        if (go.GetComponent<ButtonPressEffect>() == null)
        {
            Undo.AddComponent<ButtonPressEffect>(go);
        }
    }

    private static GameObject FindRoot(string name)
    {
        foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (go.name == name)
            {
                return go;
            }
        }
        return null;
    }

    private static GameObject FindChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child.gameObject;
            }

            GameObject found = FindChild(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
}
