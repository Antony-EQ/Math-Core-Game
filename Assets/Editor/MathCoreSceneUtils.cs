using System.Linq;
using UnityEditor;
using UnityEngine;

// Utilidades compartidas para registrar escenas generadas en Build Settings.
public static class MathCoreSceneUtils
{
    public static void RegisterSceneInBuildSettings(string scenePath, int preferredIndex)
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == scenePath))
        {
            return;
        }

        int insertIndex = Mathf.Clamp(preferredIndex, 0, scenes.Count);
        scenes.Insert(insertIndex, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
